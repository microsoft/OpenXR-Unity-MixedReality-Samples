#include "../mock.h"

#if defined(XR_USE_GRAPHICS_API_D3D11)

struct MockD3D11
{
    ID3D11Device* device = nullptr;
};

static MockD3D11 s_MockD3D11 = {};

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrGetD3D11GraphicsRequirementsKHR(
    XrInstance instance,
    XrSystemId systemId,
    XrGraphicsRequirementsD3D11KHR* graphicsRequirements)
{
    IDXGIAdapter* pAdapter;
    std::vector<IDXGIAdapter*> vAdapters;
    IDXGIFactory* pFactory = NULL;

    // Create a DXGIFactory object.
    if (FAILED(CreateDXGIFactory(__uuidof(IDXGIFactory), (void**)&pFactory)))
        return XR_ERROR_RUNTIME_FAILURE;

    XrResult xrresult = XR_ERROR_RUNTIME_FAILURE;
    graphicsRequirements->adapterLuid = {};

    for (UINT i = 0; pFactory->EnumAdapters(i, &pAdapter) != DXGI_ERROR_NOT_FOUND; ++i)
    {
        DXGI_ADAPTER_DESC desc = {};
        pAdapter->GetDesc(&desc);
        graphicsRequirements->adapterLuid = desc.AdapterLuid;
        xrresult = XR_SUCCESS;
        break;
    }

    if (pFactory)
        pFactory->Release();

    return xrresult;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockD3D11_xrEnumerateSwapchainFormats(XrSession session, uint32_t formatCapacityInput, uint32_t* formatCountOutput, int64_t* formats)
{
    if (nullptr == formatCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    *formatCountOutput = 6;

    if (formatCapacityInput == 0)
        return XR_SUCCESS;

    if (nullptr == formats)
        return XR_ERROR_VALIDATION_FAILURE;

    if (formatCapacityInput < 6)
        return XR_ERROR_SIZE_INSUFFICIENT;

    formats[0] = DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
    formats[1] = DXGI_FORMAT_B8G8R8A8_UNORM_SRGB;
    formats[2] = DXGI_FORMAT_R8G8B8A8_UNORM;
    formats[3] = DXGI_FORMAT_B8G8R8A8_UNORM;
    formats[4] = DXGI_FORMAT_D16_UNORM;
    formats[5] = DXGI_FORMAT_D32_FLOAT_S8X24_UINT;
    return XR_SUCCESS;
}

static XrResult MockD3D11_xrCreateSwapchain(XrSession session, const XrSwapchainCreateInfo* createInfo, XrSwapchain* swapchain)
{
    if (nullptr == s_MockD3D11.device)
        return XR_ERROR_RUNTIME_FAILURE;

    ID3D11Texture2D* texture;
    D3D11_TEXTURE2D_DESC desc = {};
    desc.Width = createInfo->width;
    desc.Height = createInfo->height;
    desc.MipLevels = createInfo->mipCount;
    desc.ArraySize = createInfo->arraySize;
    desc.SampleDesc.Count = createInfo->sampleCount;
    desc.SampleDesc.Quality = 0;
    desc.Usage = D3D11_USAGE_DEFAULT;
    desc.BindFlags = D3D11_BIND_SHADER_RESOURCE;

    if (createInfo->usageFlags & XR_SWAPCHAIN_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT)
    {
        if (createInfo->format == DXGI_FORMAT_D16_UNORM)
            desc.Format = DXGI_FORMAT_R16_TYPELESS;
        else
            desc.Format = DXGI_FORMAT_R32G8X24_TYPELESS;

        desc.BindFlags |= D3D11_BIND_DEPTH_STENCIL;
    }
    else
    {
        desc.BindFlags |= D3D11_BIND_RENDER_TARGET;
        desc.Format = DXGI_FORMAT_R8G8B8A8_TYPELESS;
        desc.MiscFlags = D3D11_RESOURCE_MISC_SHARED;
    }

    if (FAILED(s_MockD3D11.device->CreateTexture2D(&desc, NULL, &texture)))
        return XR_ERROR_RUNTIME_FAILURE;

    *swapchain = (XrSwapchain)texture;

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockD3D11_xrCreateSwapchainHook(XrSession session, const XrSwapchainCreateInfo* createInfo, XrSwapchain* swapchain)
{
    LOG_FUNC();
    MOCK_HOOK_NAMED("xrCreateSwapchain", MockD3D11_xrCreateSwapchain(session, createInfo, swapchain));
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockD3D11_xrDestroySwapChain(XrSwapchain swapchain)
{
    ID3D11Texture2D* texture = (ID3D11Texture2D*)swapchain;
    texture->Release();
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockD3D11_xrEnumerateSwapchainImages(XrSwapchain swapchain, uint32_t imageCapacityInput, uint32_t* imageCountOutput, XrSwapchainImageBaseHeader* images)
{
    LOG_FUNC();

    *imageCountOutput = 1;

    if (images == nullptr)
        return XR_SUCCESS;

    if (imageCapacityInput < *imageCountOutput)
        return XR_ERROR_VALIDATION_FAILURE;

    XrSwapchainImageD3D11KHR* d3d11images = (XrSwapchainImageD3D11KHR*)images;
    d3d11images[0].texture = (ID3D11Texture2D*)swapchain;
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockD3D11_xrAcquireSwapchainImage(XrSwapchain swapchain, const XrSwapchainImageAcquireInfo* acquireInfo, uint32_t* index)
{
    LOG_FUNC();

    *index = 0;

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockD3D11_xrReleaseSwapchainImage(XrSwapchain swapchain, const XrSwapchainImageReleaseInfo* releaseInfo)
{
    LOG_FUNC();
    return XR_SUCCESS;
}

/// <summary>
/// Hook xrCreateSession to get the necessary D3D11 handles
/// </summary>
extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrCreateSession(XrInstance instance, const XrSessionCreateInfo* createInfo, XrSession* session);
extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockD3D11_xrCreateSession(XrInstance instance, const XrSessionCreateInfo* createInfo, XrSession* session)
{
    s_MockD3D11 = {};

    if (createInfo->next != nullptr)
    {
        XrGraphicsBindingD3D11KHR* bindings = (XrGraphicsBindingD3D11KHR*)createInfo->next;
        if (bindings->type == XR_TYPE_GRAPHICS_BINDING_D3D11_KHR)
        {
            s_MockD3D11.device = bindings->device;
        }
    }

    return xrCreateSession(instance, createInfo, session);
}

XrResult MockD3D11_GetInstanceProcAddr(const char* name, PFN_xrVoidFunction* function)
{
    GET_PROC_ADDRESS_REMAP(xrCreateSession, MockD3D11_xrCreateSession)
    GET_PROC_ADDRESS_REMAP(xrEnumerateSwapchainFormats, MockD3D11_xrEnumerateSwapchainFormats)
    GET_PROC_ADDRESS_REMAP(xrCreateSwapchain, MockD3D11_xrCreateSwapchainHook)
    GET_PROC_ADDRESS_REMAP(xrDestroySwapChain, MockD3D11_xrDestroySwapChain)
    GET_PROC_ADDRESS_REMAP(xrEnumerateSwapchainImages, MockD3D11_xrEnumerateSwapchainImages)
    GET_PROC_ADDRESS_REMAP(xrAcquireSwapchainImage, MockD3D11_xrAcquireSwapchainImage)
    GET_PROC_ADDRESS_REMAP(xrReleaseSwapchainImage, MockD3D11_xrReleaseSwapchainImage)
    GET_PROC_ADDRESS(xrGetD3D11GraphicsRequirementsKHR)
    return XR_ERROR_FUNCTION_UNSUPPORTED;
}

#endif
