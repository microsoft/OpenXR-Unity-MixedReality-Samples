---
uid: arfoundation-cpu-camera-image
---
# Accessing the device camera image on the CPU

You can access the device camera image on the CPU by using [ARCameraManager.TryAcquireLatestCpuImage](xref:UnityEngine.XR.ARFoundation.ARCameraManager.TryAcquireLatestCpuImage(UnityEngine.XR.ARSubsystems.XRCpuImage@)).

When you call `ARCameraManager.TryAcquireLatestCpuImage`, Unity transfers textures from the GPU to the CPU. This is a resource-intensive process that impacts performance, so you should only use this method when you need to access the pixel data from the device camera to be used by code that runs on the CPU. For example, most computer vision code requires this data to be accessible on the CPU.

The number and format of textures varies by platform.

To interact with the CPU copy of the device camera image, you must first obtain a `XRCpuImage` using the `ARCameraManager`:

```csharp
public bool TryAcquireLatestCpuImage(out XRCpuImage cpuImage)
```

The `XRCpuImage` is a `struct` which represents a native resource. When your application no longer needs it, you must call `Dispose` on it to release it back to the system. You can hold a `XRCpuImage` for multiple frames, but most platforms have a limited number of frames, so failure to `Dispose` them might prevent the system from providing new device camera images.

The `XRCpuImage` gives you access to three features:

- [Raw image planes](#raw-image-planes)
- [Synchronously convert to grayscale and color](#synchronously-convert-to-grayscale-and-color)
- [Asynchronously convert to grayscale and color](#asynchronously-convert-to-grayscale-and-color)

## Raw image planes

> [!NOTE]
> An image "plane", in this context, refers to a channel used in the video format. It's not a planar surface and not related to an `ARPlane`.

Most video formats use a YUV encoding variant, where Y is the luminance plane, and the UV plane(s) contain chromaticity information. U and V can be interleaved or separate planes, and there might be additional padding per pixel or per row.

If you need access to the raw, platform-specific YUV data, you can get each image "plane" using the `XRCpuImage.GetPlane` method.

### Example

```csharp
if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
    return;

// Consider each image plane.
for (int planeIndex = 0; planeIndex < image.planeCount; ++planeIndex)
{
    // Log information about the image plane.
    var plane = image.GetPlane(planeIndex);
    Debug.LogFormat("Plane {0}:\n\tsize: {1}\n\trowStride: {2}\n\tpixelStride: {3}",
        planeIndex, plane.data.Length, plane.rowStride, plane.pixelStride);

    // Do something with the data.
    MyComputerVisionAlgorithm(plane.data);
}

// Dispose the XRCpuImage to avoid resource leaks.
image.Dispose();
```

A `XRCpuImage.Plane` provides direct access to a native memory buffer via a `NativeArray<byte>`. This represents a "view" into the native memory; you don't need to dispose the `NativeArray`, and the data is only valid until the `XRCpuImage` is disposed. You should consider this memory read-only.

## Synchronously convert to grayscale and color

To obtain grayscale or color versions of the camera image, you need to convert the raw plane data. `XRCpuImage` provides both synchronous and asynchronous conversion methods. This section covers the synchronous method.

This method converts the `XRCpuImage` into the `TextureFormat` specified by `conversionParams`, and writes the data to the buffer at `destinationBuffer`. Grayscale images (`TextureFormat.Alpha8` and `TextureFormat.R8`) are typically very fast, while color conversions require CPU-intensive computations.

```csharp
public void Convert(XRCpuImage.ConversionParams conversionParams, IntPtr destinationBuffer, int bufferLength)
```

Here's a more detailed look at `XRCpuImage.ConversionParams`:

```csharp
public struct ConversionParams
{
    public RectInt inputRect;
    public Vector2Int outputDimensions;
    public TextureFormat outputFormat;
    public Transformation transformation;
}
```

|Property|Description|
|-|-|
|`inputRect`|The portion of the `XRCpuImage` to convert. This can be the full image or a sub-rectangle of the image. The `inputRect` must fit completely inside the original image. It can be significantly faster to convert a sub-rectangle of the original image if you know which part of the image you need.|
|`outputDimensions`|The dimensions of the output image. The `XRCpuImage` converter supports downsampling (using nearest neighbor), allowing you to specify a smaller output image than the `inputRect.width` and `inputRect.height` parameters. For example, you could supply `(inputRect.width / 2, inputRect.height / 2)` to get a half resolution image. This can decrease the time it takes to perform a color conversion. The `outputDimensions` must be less than or equal to the `inputRect`'s dimensions (no upsampling).|
|`outputFormat`|The following formats are currently supported<ul><li>`TextureFormat.RGB24`</li><li>`TextureFormat.RGBA24`</li><li>`TextureFormat.ARGB32`</li><li>`TextureFormat.BGRA32`</li><li>`TextureFormat.Alpha8`</li><li>`TextureFormat.R8`</li></ul>You can also use `XRCpuImage.FormatSupported` to test a texture format before calling one of the conversion methods.|
|`transformation`|Use this property to specify a transformation to apply during the conversion, such as mirroring the image across the X or Y axis, or across both axes. This typically doesn't increase the processing time.|

Because you must supply the destination buffer, you also need to know how many bytes you'll need to store the converted image. To get the required number of bytes, use:

```csharp
public int GetConvertedDataSize(Vector2Int dimensions, TextureFormat format)
```
The data produced by the conversion is compatible with `Texture2D` using [`Texture2D.LoadRawTextureData`](https://docs.unity3d.com/ScriptReference/Texture2D.LoadRawTextureData.html).

### Example

```csharp
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CameraImageExample : MonoBehaviour
{
    Texture2D m_Texture;

    void OnEnable()
    {
        cameraManager.cameraFrameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.cameraFrameReceived -= OnCameraFrameReceived;
    }

    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        var conversionParams = new XRCpuImage.ConversionParams
        {
            // Get the entire image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.MirrorY
        };

        // See how many bytes you need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image.
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
        image.Dispose();

        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
        // In this example, you apply it to a texture to visualize it.

        // You've got the data; let's put it into a texture so you can visualize it.
        m_Texture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        m_Texture.LoadRawTextureData(buffer);
        m_Texture.Apply();

        // Done with your temporary data, so you can dispose it.
        buffer.Dispose();
    }
}
```

## Asynchronously convert to grayscale and color

If you don't need the current image immediately, you can convert it asynchronously using `XRCpuImage.ConvertAsync`. You can make as many asynchronous image requests as you like. They're typically ready by the next frame, but since there is no limit on the number of outstanding requests, your request might take some time if there are several images in the queue. Requests are processed in the order they're received.

`XRCpuImage.ConvertAsync` returns an `XRCpuImage.AsyncConversion`. This lets you query the status of the conversion and, once complete, get the pixel data.

Once you have a conversion object, you can query its status to find out if it's done:

```csharp
XRCpuImage.AsyncConversion conversion = image.ConvertAsync(...);
while (!conversion.status.IsDone())
    yield return null;
```
Use the `status` to determine whether the request is complete. If the status is `XRCpuImage.AsyncConversionStatus.Ready`, you can call `GetData<T>` to get the pixel data as a `NativeArray<T>`.

`GetData<T>` returns a `NativeArray<T>` which is a direct "view" into native memory and is valid until you call `Dispose` on the `XRCpuImage.AsyncConversion`. It's an error to access the `NativeArray<T>` after the `XRCpuImage.AsyncConversion` has been disposed. You don't need to dispose the `NativeArray<T>` that `GetData<T>` returns.

> [!IMPORTANT]
> You must explicitly dispose `XRCpuImage.AsyncConversion`s. Failing to dispose an `XRCpuImage.AsyncConversion` will leak memory until the `XRCameraSubsystem` is destroyed. The `XRCameraSubsystem` will remove all async conversions when destroyed.

> [!NOTE]
> You can dispose `XRCpuImage` before the asynchronous conversion completes. The data contained by the `XRCpuImage.AsyncConversion` isn't tied to the `XRCpuImage`.

### Example

```csharp
Texture2D m_Texture;

public void GetImageAsync()
{
    // Get information about the device camera image.
    if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
    {
        // If successful, launch a coroutine that waits for the image
        // to be ready, then apply it to a texture.
        StartCoroutine(ProcessImage(image));

        // It's safe to dispose the image before the async operation completes.
        image.Dispose();
    }
}

IEnumerator ProcessImage(XRCpuImage image)
{
    // Create the async conversion request.
    var request = image.ConvertAsync(new XRCpuImage.ConversionParams
    {
        // Use the full image.
        inputRect = new RectInt(0, 0, image.width, image.height),

        // Downsample by 2.
        outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

        // Color image format.
        outputFormat = TextureFormat.RGB24,

        // Flip across the Y axis.
        transformation = XRCpuImage.Transformation.MirrorY
    });

    // Wait for the conversion to complete.
    while (!request.status.IsDone())
        yield return null;

    // Check status to see if the conversion completed successfully.
    if (request.status != XRCpuImage.AsyncConversionStatus.Ready)
    {
        // Something went wrong.
        Debug.LogErrorFormat("Request failed with status {0}", request.status);

        // Dispose even if there is an error.
        request.Dispose();
        yield break;
    }

    // Image data is ready. Let's apply it to a Texture2D.
    var rawData = request.GetData<byte>();

    // Create a texture if necessary.
    if (m_Texture == null)
    {
        m_Texture = new Texture2D(
            request.conversionParams.outputDimensions.x,
            request.conversionParams.outputDimensions.y,
            request.conversionParams.outputFormat,
            false);
    }

    // Copy the image data into the texture.
    m_Texture.LoadRawTextureData(rawData);
    m_Texture.Apply();

    // Need to dispose the request to delete resources associated
    // with the request, including the raw data.
    request.Dispose();
}
```

There's also a version of `ConvertAsync` which accepts a delegate and doesn't return an `XRCpuImage.AsyncConversion`:

```csharp
public void GetImageAsync()
{
    // Get information about the device camera image.
    if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
    {
        // If successful, launch a coroutine that waits for the image
        // to be ready, then apply it to a texture.
        image.ConvertAsync(new XRCpuImage.ConversionParams
        {
            // Get the full image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            // Color image format.
            outputFormat = TextureFormat.RGB24,

            // Flip across the Y axis.
            transformation = CameraImageTransformation.MirrorY

            // Call ProcessImage when the async operation completes.
        }, ProcessImage);

        // It's safe to dispose the image before the async operation completes.
        image.Dispose();
    }
}

void ProcessImage(XRCpuImage.AsyncConversionStatus status, XRCpuImage.ConversionParams conversionParams, NativeArray<byte> data)
{
    if (status != XRCpuImage.AsyncConversionStatus.Ready)
    {
        Debug.LogErrorFormat("Async request failed with status {0}", status);
        return;
    }

    // Do something useful, like copy to a Texture2D or pass to a computer vision algorithm.
    DoSomethingWithImageData(data);

    // Data is destroyed upon return; no need to dispose.
}
```

In this version, the `NativeArray<byte>` is again a "view" into the native memory associated with the request, and you don't need to dispose it. It's only valid for the duration of the delegate invocation and is destroyed immediately upon return. If you need the data to persist beyond the lifetime of your delegate, make a copy (see [`NativeArray<T>.CopyTo`](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.CopyTo.html) and [`NativeArray<T>.CopyFrom`](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.CopyFrom.html)).
