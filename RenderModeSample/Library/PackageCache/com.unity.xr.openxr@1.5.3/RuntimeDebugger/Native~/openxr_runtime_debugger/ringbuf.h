#include <cassert>
#include <deque>
#include <sstream>
#include <string>

// Block-based dynamic allocator via ring-buffer.
// Ring buffer that stores "blocks" which dynamically grow and wrap.
// If a section grows large enough that it overlaps another section, the other section is forgotten.
// 8 bit alignment, bring your own synchronization.
// Must call Create first, and Destroy when done.  Must create a section before writing.
// This probably isn't perfect.. some tests are at the bottom and it seems to be stable for the usecase.
struct RingBuf
{
    enum OverflowMode
    {
        kOverflowModeWrap,
        kOverflowModeTruncate,
        kOverflowModeGrowDouble,
    };

    uint8_t* data;
    uint32_t cacheSize;
    OverflowMode overflowMode;

    // must be pointer because of thread_local compiler bug
    std::deque<uint32_t>* offsets;

    void Create(uint32_t csize, OverflowMode overflowMode)
    {
        this->overflowMode = overflowMode;
        if (data == nullptr)
        {
            data = (uint8_t*)malloc(csize);
            cacheSize = csize;
            offsets = new std::deque<uint32_t>();
        }
        Reset();
    }

    void SetOverflowMode(OverflowMode overflowMode)
    {
        this->overflowMode = overflowMode;
    }

    void Reset()
    {
        if (offsets != nullptr)
        {
            offsets->clear();
            offsets->push_back(0);
        }
    }

    void Destroy()
    {
        if (data != nullptr)
        {
            free(data);
            data = nullptr;
            delete (offsets);
            cacheSize = 0;
        }
    }

    void CreateNewBlock()
    {
        if (offsets != nullptr)
            offsets->push_back(offsets->back());
    }

    void DropLastBlock()
    {
        if (offsets != nullptr)
            offsets->pop_back();
    }

    uint8_t* GetForWrite(uint32_t size)
    {
        uint8_t* ret{nullptr};

        if (offsets == nullptr)
            return nullptr;
        if (size > cacheSize && overflowMode == kOverflowModeWrap)
            return nullptr;
        if (offsets->size() < 2)
            return nullptr;

        uint32_t head = offsets->front();
        uint32_t tail = offsets->back();
        offsets->pop_back();

        // need to wrap
        if (tail + size > cacheSize)
        {
            if (overflowMode == kOverflowModeWrap)
            {
                // section grew larger than full buffer and overwrote itself, abort.
                if (offsets->size() == 1)
                    return nullptr;

                if (offsets->back() != tail)
                    offsets->push_back(tail);

                uint32_t front = offsets->front();
                while (front != 0)
                {
                    offsets->pop_front();
                    front = offsets->front();
                }
                offsets->pop_front();

                offsets->push_back(0);

                head = offsets->front();
                tail = offsets->back();
            }
            else if (overflowMode == kOverflowModeGrowDouble)
            {
                while (tail + size > cacheSize)
                {
                    // grow
                    uint32_t newCacheSize = cacheSize * 2;
                    uint8_t* newData = (uint8_t*)malloc(newCacheSize);
                    memcpy(newData, data, cacheSize);
                    free(data);
                    data = newData;
                    cacheSize = newCacheSize;
                }
            }
            else if (overflowMode == kOverflowModeTruncate)
            {
                return nullptr;
            }
            else
            {
                assert(0);
            }
        }

        // need to free space / forget sections
        if (tail <= head && head != 0)
        {
            while (tail + size > head)
            {
                offsets->pop_front();

                head = offsets->front();
                tail = offsets->back();

                if (offsets->size() == 1)
                    break;

                if (*(offsets->begin() + 1) == 0)
                {
                    offsets->pop_front();
                    break;
                }
            }
        }

        offsets->push_back(tail + size);
        ret = &data[tail];

        return ret;
    }

    bool HasDataForRead()
    {
        return offsets != nullptr && offsets->size() > 1 && offsets[0] != offsets[1];
    }

    // returns true if there is more data to read
    // in practice there may be two reads necessary to get all the data, from mid to end and from beginning to mid.
    // reading is a one time operation - data is cleared after read.
    bool GetForReadAndClear(uint8_t** ptr, uint32_t* size)
    {
        if (offsets == nullptr)
        {
            *ptr = nullptr;
            *size = 0;
            return false;
        }

        uint32_t head = offsets->front();
        *ptr = &data[head];

        uint32_t max = 0;
        for (auto it = offsets->begin(); it != offsets->end();)
        {
            if (*it > max)
                max = *it;
            else if (max == 0)
            {
                ++it;
                continue;
            }
            else
                break;
            it = offsets->erase(it);
        }

        *size = max - head;

        return offsets->size() > 1 && size != 0;
    }

    bool GetForRead(uint8_t** ptr, uint32_t* size)
    {
        if (overflowMode == kOverflowModeGrowDouble && offsets != nullptr)
        {
            *ptr = data;
            *size = offsets->back();
            return false;
        }
        *ptr = nullptr;
        *size = 0;
        return false;
    }

    bool MoveFrom(RingBuf& other)
    {
        if (other.HasDataForRead() && offsets != nullptr)
        {
            CreateNewBlock();
            uint8_t* ptr{};
            uint32_t size{};
            bool more = false;
            do
            {
                more = other.GetForReadAndClear(&ptr, &size);
                uint8_t* dst = GetForWrite(size);
                if (dst != nullptr)
                {
                    memcpy(dst, ptr, size);
                }
            } while (more);
        }
        else
        {
            return false;
        }
        return true;
    }

    void Write(Command c)
    {
        uint8_t* wbuf = GetForWrite(sizeof(Command));
        if (wbuf != nullptr)
        {
            *(Command*)wbuf = c;
        }
    }

    void Write(LUT l)
    {
        uint8_t* wbuf = GetForWrite(sizeof(LUT));
        if (wbuf != nullptr)
        {
            *(LUT*)wbuf = l;
        }
    }

    void Write(std::thread::id id)
    {
        std::stringstream ss;
        ss << id;
        Write(ss.str());
    }

    void Write(const std::string& s)
    {
        uint8_t* wbuf = GetForWrite(sizeof(char) * ((uint32_t)s.size() + 1));
        if (wbuf != nullptr)
        {
            memcpy(wbuf, s.c_str(), sizeof(char) * (uint32_t)s.size());
            wbuf[s.size()] = 0;
        }
    }

    void Write(float f)
    {
        uint8_t* wbuf = GetForWrite(sizeof(float));
        if (wbuf != nullptr)
        {
            *(float*)wbuf = f;
        }
    }

    void Write(int32_t i)
    {
        uint8_t* wbuf = GetForWrite(sizeof(int32_t));
        if (wbuf != nullptr)
        {
            *(int32_t*)wbuf = i;
        }
    }

    void Write(int64_t i)
    {
        uint8_t* wbuf = GetForWrite(sizeof(int64_t));
        if (wbuf != nullptr)
        {
            *(int64_t*)wbuf = i;
        }
    }

    void Write(uint32_t u)
    {
        uint8_t* wbuf = GetForWrite(sizeof(uint32_t));
        if (wbuf != nullptr)
        {
            *(uint32_t*)wbuf = u;
        }
    }

    void Write(uint64_t u)
    {
        uint8_t* wbuf = GetForWrite(sizeof(uint64_t));
        if (wbuf != nullptr)
        {
            *(uint64_t*)wbuf = u;
        }
    }
};

//#define CACHE_SIZE 1024 * 1024 * 1
//
//int tests()
//{
//    RingBuf buf{};
//    buf.Create(CACHE_SIZE);
//
//    uint8_t* ptr;
//    uint32_t size;
//
//    // Check that starts out empty
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == 0);
//    buf.Reset();
//
//    // Write
//    buf.CreateNewBlock();
//    auto* wbuf = buf.GetForWrite(4);
//    *(uint32_t*)wbuf = 0x12345678;
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == 4);
//    assert(*(uint32_t*)ptr == 0x12345678);
//    buf.Reset();
//
//    // Write 2
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(4);
//    *(uint32_t*)wbuf = 0x12345678;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    *(uint64_t*)wbuf = 0x123456789abcdef0;
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == 12);
//    assert(*(uint32_t*)ptr == 0x12345678);
//    assert(*(uint64_t*)(ptr + 4) == 0x123456789abcdef0);
//    buf.Reset();
//
//    // Full buf
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(CACHE_SIZE - 16);
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == CACHE_SIZE);
//    buf.Reset();
//
//    // Wrap buf, perfectly lines up
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 1;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 2;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(CACHE_SIZE - 16);
//    for (int i = 0; i < (CACHE_SIZE - 16); ++i)
//        wbuf[i] = 3;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 4;
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == true);
//    assert(size == CACHE_SIZE - 8);
//    assert(ptr[0] == 2);
//    assert(ptr[8] == 3);
//    assert(ptr[size - 1] == 3);
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == 8);
//    assert(ptr[0] == 4);
//    buf.Reset();
//
//    // Wrap buf, not perfect
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 1;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 2;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(CACHE_SIZE - 20);
//    for (int i = 0; i < (CACHE_SIZE - 20); ++i)
//        wbuf[i] = 3;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 4;
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == true);
//    assert(size == CACHE_SIZE - 12);
//    assert(ptr[0] == 2);
//    assert(ptr[8] == 3);
//    assert(ptr[size - 1] == 3);
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == 8);
//    assert(ptr[0] == 4);
//    buf.Reset();
//
//    // Wrap, and fit more in previous first spot
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 1;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 2;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(CACHE_SIZE - 16);
//    for (int i = 0; i < (CACHE_SIZE - 16); ++i)
//        wbuf[i] = 3;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(4);
//    for (int i = 0; i < 4; ++i)
//        wbuf[i] = 4;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(4);
//    for (int i = 0; i < 4; ++i)
//        wbuf[i] = 5;
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == true);
//    assert(size == CACHE_SIZE - 8);
//    assert(ptr[0] == 2);
//    assert(ptr[8] == 3);
//    assert(ptr[size - 1] == 3);
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == 8);
//    assert(ptr[0] == 4);
//    assert(ptr[4] == 5);
//    buf.Reset();
//
//    // Wrap, and consume first two entries
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 1;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 2;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(CACHE_SIZE - 16);
//    for (int i = 0; i < (CACHE_SIZE - 16); ++i)
//        wbuf[i] = 3;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(4);
//    for (int i = 0; i < 4; ++i)
//        wbuf[i] = 4;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 5;
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == true);
//    assert(size == CACHE_SIZE - 8 - 8);
//    assert(ptr[0] == 3);
//    assert(ptr[size - 1] == 3);
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == 12);
//    assert(ptr[0] == 4);
//    assert(ptr[4] == 5);
//    buf.Reset();
//
//    // Wrap, and consume first three entries
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 1;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 2;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(CACHE_SIZE - 16);
//    for (int i = 0; i < (CACHE_SIZE - 16); ++i)
//        wbuf[i] = 3;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(4);
//    for (int i = 0; i < 4; ++i)
//        wbuf[i] = 4;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 5;
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 6;
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == 20);
//    assert(ptr[0] == 4);
//    assert(ptr[4] == 5);
//    assert(ptr[12] == 6);
//    buf.Reset();
//
//    // Section wraps, perfectly
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 1;
//    buf.CreateNewBlock();
//    buf.GetForWrite(CACHE_SIZE);
//    for (int i = 0; i < (CACHE_SIZE); ++i)
//        wbuf[i] = 2;
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == CACHE_SIZE);
//    assert(ptr[0] == 2);
//    assert(ptr[CACHE_SIZE - 1] == 2);
//    buf.Reset();
//
//    // Section wraps, not perfect
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 1;
//    buf.CreateNewBlock();
//    buf.GetForWrite(CACHE_SIZE - 16);
//    for (int i = 0; i < (CACHE_SIZE - 16); ++i)
//        wbuf[i] = 2;
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 2;
//    wbuf = buf.GetForWrite(8);
//    for (int i = 0; i < 8; ++i)
//        wbuf[i] = 2;
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == true);
//    assert(size == CACHE_SIZE - 8);
//    assert(ptr[0] == 2);
//    assert(ptr[size - 1] == 2);
//
//    assert(buf.GetForReadAndClear(&ptr, &size) == false);
//    assert(size == 8);
//    assert(ptr[0] == 2);
//    buf.Reset();
//
//    // section wraps on itself
//    buf.CreateNewBlock();
//    wbuf = buf.GetForWrite(CACHE_SIZE - 8);
//    wbuf = buf.GetForWrite(8);
//
//    wbuf = buf.GetForWrite(8);
//    assert(wbuf == nullptr);
//
//    return 0;
//}