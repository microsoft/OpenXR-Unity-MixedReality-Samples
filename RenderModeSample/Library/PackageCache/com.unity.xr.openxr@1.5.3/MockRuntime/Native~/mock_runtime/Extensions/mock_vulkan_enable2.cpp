#include "../mock.h"

#if defined(XR_USE_GRAPHICS_API_VULKAN)

struct MockVulkan
{
    VkInstance instance = VK_NULL_HANDLE;
    VkPhysicalDevice phsyicalDevice = VK_NULL_HANDLE;
    VkDevice device = VK_NULL_HANDLE;
    VkQueue queue = VK_NULL_HANDLE;
    VkPhysicalDeviceMemoryProperties memoryProperties = {};

    PFN_vkGetInstanceProcAddr vkGetInstanceProcAddr = nullptr;
    PFN_vkAllocateMemory vkAllocateMemory = nullptr;
    PFN_vkCreateImage vkCreateImage = nullptr;
    PFN_vkCreateDevice vkCreateDevice = nullptr;
    PFN_vkEnumeratePhysicalDevices vkEnumeratePhysicalDevices = nullptr;
    PFN_vkBindImageMemory vkBindImageMemory = nullptr;
    PFN_vkDestroyImage vkDestroyImage = nullptr;
    PFN_vkGetImageMemoryRequirements vkGetImageMemoryRequirements = nullptr;
    PFN_vkGetPhysicalDeviceMemoryProperties vkGetPhysicalDeviceMemoryProperties = nullptr;
};

static MockVulkan s_MockVulkan = {};

static bool GetVulkanMemoryTypeIndex(uint32_t typeBits, VkMemoryPropertyFlags properties, uint32_t* index)
{
    for (uint32_t i = 0; i < s_MockVulkan.memoryProperties.memoryTypeCount; i++, typeBits >>= 1)
    {
        // Only consider memory types included in the typeBits mask
        if ((typeBits & 1) == 0)
            continue;

        // If all requested properties are a match then return this memory type
        if ((s_MockVulkan.memoryProperties.memoryTypes[i].propertyFlags & properties) == properties)
        {
            *index = i;
            return true;
        }
    }

    return false;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetVulkanInstanceExtensionsKHR(XrInstance instance, XrSystemId systemId, uint32_t bufferCapacityInput, uint32_t* bufferCountOutput, char* buffer)
{
    if (nullptr == bufferCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    *bufferCountOutput = 0;

    if (bufferCapacityInput > 0 && buffer == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetVulkanDeviceExtensionsKHR(XrInstance instance, XrSystemId systemId, uint32_t bufferCapacityInput, uint32_t* bufferCountOutput, char* buffer)
{
    if (nullptr == bufferCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    *bufferCountOutput = 0;

    if (bufferCapacityInput > 0 && buffer == nullptr)
        return XR_ERROR_VALIDATION_FAILURE;

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetVulkanGraphicsDeviceKHR(XrInstance instance, XrSystemId systemId, VkInstance vkInstance, VkPhysicalDevice* vkPhysicalDevice)
{
    *vkPhysicalDevice = s_MockVulkan.phsyicalDevice;
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateVulkanInstanceKHR(
    XrInstance instance,
    const XrVulkanInstanceCreateInfoKHR* createInfo,
    VkInstance* vulkanInstance,
    VkResult* vulkanResult)
{
    s_MockVulkan = {};
    s_MockVulkan.vkGetInstanceProcAddr = createInfo->pfnGetInstanceProcAddr;

    if (s_MockVulkan.vkGetInstanceProcAddr == nullptr)
        return XR_ERROR_RUNTIME_FAILURE;

    PFN_vkCreateInstance vkCreateInstance = (PFN_vkCreateInstance)s_MockVulkan.vkGetInstanceProcAddr(nullptr, "vkCreateInstance");
    if (nullptr == vkCreateInstance)
        return XR_ERROR_RUNTIME_FAILURE;

    if (VK_SUCCESS != vkCreateInstance(createInfo->vulkanCreateInfo, createInfo->vulkanAllocator, vulkanInstance))
        return XR_ERROR_RUNTIME_FAILURE;

#define VK_GET_PROC_ADDR(func) s_MockVulkan.func = (PFN_##func)s_MockVulkan.vkGetInstanceProcAddr(*vulkanInstance, #func);
    VK_GET_PROC_ADDR(vkCreateImage)
    VK_GET_PROC_ADDR(vkCreateDevice)
    VK_GET_PROC_ADDR(vkDestroyImage)
    VK_GET_PROC_ADDR(vkGetPhysicalDeviceMemoryProperties)
    VK_GET_PROC_ADDR(vkGetImageMemoryRequirements)
    VK_GET_PROC_ADDR(vkAllocateMemory)
    VK_GET_PROC_ADDR(vkBindImageMemory)
    VK_GET_PROC_ADDR(vkEnumeratePhysicalDevices)
#undef VK_GET_PROC_ADDR

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateVulkanDeviceKHR(XrInstance instance, const XrVulkanDeviceCreateInfoKHR* createInfo, VkDevice* device, VkResult* result)
{
    *result = s_MockVulkan.vkCreateDevice(createInfo->vulkanPhysicalDevice, createInfo->vulkanCreateInfo, createInfo->vulkanAllocator, device);
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetVulkanGraphicsRequirements2KHR(XrInstance instance, XrSystemId systemId, XrGraphicsRequirementsVulkanKHR* graphicsRequirements)
{
    graphicsRequirements->minApiVersionSupported = VK_MAKE_VERSION(0, 0, 0);
    graphicsRequirements->maxApiVersionSupported = VK_MAKE_VERSION(255, 255, 255);
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetVulkanGraphicsDevice2KHR(
    XrInstance instance,
    const XrVulkanGraphicsDeviceGetInfoKHR* getInfo,
    VkPhysicalDevice* vulkanPhysicalDevice)
{
    if (nullptr == s_MockVulkan.vkEnumeratePhysicalDevices)
    {
        *vulkanPhysicalDevice = nullptr;
        return XR_SUCCESS;
    }

    uint32_t physicalDeviceCount = 0;
    s_MockVulkan.vkEnumeratePhysicalDevices(getInfo->vulkanInstance, &physicalDeviceCount, nullptr);
    if (physicalDeviceCount == 0)
        return XR_ERROR_RUNTIME_FAILURE;

    std::vector<VkPhysicalDevice> physicalDevices;
    physicalDevices.resize(physicalDeviceCount);
    s_MockVulkan.vkEnumeratePhysicalDevices(getInfo->vulkanInstance, &physicalDeviceCount, physicalDevices.data());
    if (physicalDeviceCount != (uint32_t)physicalDevices.size())
        return XR_ERROR_RUNTIME_FAILURE;

    *vulkanPhysicalDevice = physicalDevices[0];

    return XR_SUCCESS;
}

/// <summary>
/// Hook xrCreateSession to get the necessary vulkan handles
/// </summary>
extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateSession(XrInstance instance, const XrSessionCreateInfo* createInfo, XrSession* session);
extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockVulkan_xrCreateSession(XrInstance instance, const XrSessionCreateInfo* createInfo, XrSession* session)
{
    if (createInfo->next != nullptr)
    {
        XrGraphicsBindingVulkanKHR* bindings = (XrGraphicsBindingVulkanKHR*)createInfo->next;
        if (bindings->type == XR_TYPE_GRAPHICS_BINDING_VULKAN_KHR)
        {
            s_MockVulkan.device = bindings->device;
            s_MockVulkan.instance = bindings->instance;
            s_MockVulkan.phsyicalDevice = bindings->physicalDevice;

            if (s_MockVulkan.vkGetPhysicalDeviceMemoryProperties == nullptr)
                return XR_ERROR_RUNTIME_FAILURE;

            s_MockVulkan.vkGetPhysicalDeviceMemoryProperties(s_MockVulkan.phsyicalDevice, &s_MockVulkan.memoryProperties);
        }
    }

    return xrCreateSession(instance, createInfo, session);
}

static VkImage CreateSwapchainImage(const XrSwapchainCreateInfo* createInfo)
{
    VkImageCreateInfo imageCreateInfo = {};
    imageCreateInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
    imageCreateInfo.imageType = VK_IMAGE_TYPE_2D;
    imageCreateInfo.format = (VkFormat)createInfo->format;
    imageCreateInfo.mipLevels = createInfo->mipCount;
    imageCreateInfo.arrayLayers = createInfo->arraySize;
    imageCreateInfo.samples = VK_SAMPLE_COUNT_1_BIT;
    imageCreateInfo.tiling = VK_IMAGE_TILING_OPTIMAL;
    imageCreateInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
    imageCreateInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
    imageCreateInfo.extent.width = createInfo->width;
    imageCreateInfo.extent.height = createInfo->height;
    imageCreateInfo.extent.depth = 1;
    imageCreateInfo.usage = 0;

    if (createInfo->usageFlags & XR_SWAPCHAIN_USAGE_COLOR_ATTACHMENT_BIT)
        imageCreateInfo.usage |= VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
    if (createInfo->usageFlags & XR_SWAPCHAIN_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT)
        imageCreateInfo.usage |= VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT;

    VkImage vkimage;
    s_MockVulkan.vkCreateImage(s_MockVulkan.device, &imageCreateInfo, nullptr, &vkimage);

    // Allocate memory to back the destination image
    VkMemoryRequirements memRequirements;
    s_MockVulkan.vkGetImageMemoryRequirements(s_MockVulkan.device, vkimage, &memRequirements);

    VkDeviceMemory imageMemory;
    VkMemoryAllocateInfo memAllocInfo = {};
    memAllocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
    memAllocInfo.allocationSize = memRequirements.size;

    if (!GetVulkanMemoryTypeIndex(memRequirements.memoryTypeBits, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT, &memAllocInfo.memoryTypeIndex))
        return VK_NULL_HANDLE;

    s_MockVulkan.vkAllocateMemory(s_MockVulkan.device, &memAllocInfo, nullptr, &imageMemory);
    s_MockVulkan.vkBindImageMemory(s_MockVulkan.device, vkimage, imageMemory, 0);

    return vkimage;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockVulkan_xrCreateSwapchain(XrSession session, const XrSwapchainCreateInfo* createInfo, XrSwapchain* swapchain)
{
    LOG_FUNC();

    *swapchain = (XrSwapchain)CreateSwapchainImage(createInfo);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockVulkan_xrCreateSwapchainHook(XrSession session, const XrSwapchainCreateInfo* createInfo, XrSwapchain* swapchain)
{
    LOG_FUNC();
    MOCK_HOOK_NAMED("xrCreateSwapchain", MockVulkan_xrCreateSwapchain(session, createInfo, swapchain));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockVulkan_xrDestroySwapChain(XrSwapchain swapchain)
{
    s_MockVulkan.vkDestroyImage(s_MockVulkan.device, (VkImage)swapchain, nullptr);
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockVulkan_xrAcquireSwapchainImage(XrSwapchain swapchain, const XrSwapchainImageAcquireInfo* acquireInfo, uint32_t* index)
{
    LOG_FUNC();

    *index = 0;

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockVulkan_xrReleaseSwapchainImage(XrSwapchain swapchain, const XrSwapchainImageReleaseInfo* releaseInfo)
{
    LOG_FUNC();
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockVulkan_xrEnumerateSwapchainFormats(XrSession session, uint32_t formatCapacityInput, uint32_t* formatCountOutput, int64_t* formats)
{
    if (nullptr == formatCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    *formatCountOutput = 3;

    if (formatCapacityInput == 0)
        return XR_SUCCESS;

    if (nullptr == formats)
        return XR_ERROR_VALIDATION_FAILURE;

    if (formatCapacityInput < 3)
        return XR_ERROR_SIZE_INSUFFICIENT;

    formats[0] = VK_FORMAT_R8G8B8A8_SRGB;
    formats[1] = VK_FORMAT_D16_UNORM;
    formats[2] = VK_FORMAT_D24_UNORM_S8_UINT;
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockVulkan_xrEnumerateSwapchainImages(XrSwapchain swapchain, uint32_t imageCapacityInput, uint32_t* imageCountOutput, XrSwapchainImageBaseHeader* images)
{
    LOG_FUNC();

    *imageCountOutput = 1;

    if (images == nullptr)
        return XR_SUCCESS;

    if (imageCapacityInput < *imageCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    XrSwapchainImageVulkanKHR* vkimages = (XrSwapchainImageVulkanKHR*)images;
    vkimages[0].image = (VkImage)swapchain;
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrEndFrame(XrSession session, const XrFrameEndInfo* frameEndInfo);
extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockVulkan_xrEndFrame(XrSession session, const XrFrameEndInfo* frameEndInfo)
{
    return xrEndFrame(session, frameEndInfo);
}

XrResult MockVulkan_GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function)
{
    GET_PROC_ADDRESS_REMAP(xrCreateSession, MockVulkan_xrCreateSession)
    GET_PROC_ADDRESS_REMAP(xrCreateSwapchain, MockVulkan_xrCreateSwapchainHook)
    GET_PROC_ADDRESS_REMAP(xrDestroySwapChain, MockVulkan_xrDestroySwapChain)
    GET_PROC_ADDRESS_REMAP(xrEnumerateSwapchainFormats, MockVulkan_xrEnumerateSwapchainFormats)
    GET_PROC_ADDRESS_REMAP(xrEnumerateSwapchainImages, MockVulkan_xrEnumerateSwapchainImages)
    GET_PROC_ADDRESS_REMAP(xrAcquireSwapchainImage, MockVulkan_xrAcquireSwapchainImage)
    GET_PROC_ADDRESS_REMAP(xrReleaseSwapchainImage, MockVulkan_xrReleaseSwapchainImage)
    GET_PROC_ADDRESS_REMAP(xrEndFrame, MockVulkan_xrEndFrame)
    GET_PROC_ADDRESS(xrCreateVulkanInstanceKHR)
    GET_PROC_ADDRESS(xrGetVulkanGraphicsRequirements2KHR)
    GET_PROC_ADDRESS(xrGetVulkanGraphicsDevice2KHR)
    GET_PROC_ADDRESS(xrCreateVulkanDeviceKHR)
    return XR_ERROR_FUNCTION_UNSUPPORTED;
}

#endif // XR_USE_GRAPHICS_API_VULKAN
