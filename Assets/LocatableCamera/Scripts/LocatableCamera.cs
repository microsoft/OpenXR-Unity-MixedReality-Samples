// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using UnityEngine;
using UnityEngine.Windows.WebCam;

namespace Microsoft.MixedReality.OpenXR.Samples
{
    public class LocatableCamera : MonoBehaviour
    {
        [SerializeField]
        private Shader textureShader = null;

        [SerializeField]
        private TextMesh text = null;

        private PhotoCapture photoCaptureObject = null;
        private Resolution cameraResolution = default(Resolution);
        private bool isCapturingPhoto, isReadyToCapturePhoto = false;
        private uint numPhotos = 0;

        private void Start()
        {
            var resolutions = PhotoCapture.SupportedResolutions;
            if (resolutions == null || resolutions.Count() == 0)
            {
                if (text != null)
                {
                    text.text = "Resolutions not available. Did you provide web cam access?";
                }
                return;
            }

            cameraResolution = resolutions.OrderByDescending((res) => res.width * res.height).First();
            PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);

            if (text != null)
            {
                text.text = "Starting camera...";
            }
        }

        private void OnPhotoCaptureCreated(PhotoCapture captureObject)
        {
            if (text != null)
            {
                text.text += "\nPhotoCapture created...";
            }

            photoCaptureObject = captureObject;

            CameraParameters cameraParameters = new CameraParameters(WebCamMode.PhotoMode)
            {
                hologramOpacity = 0.0f,
                cameraResolutionWidth = cameraResolution.width,
                cameraResolutionHeight = cameraResolution.height,
                pixelFormat = CapturePixelFormat.BGRA32
            };

            captureObject.StartPhotoModeAsync(cameraParameters, OnPhotoModeStarted);
        }

        private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                isReadyToCapturePhoto = true;

                if (text != null)
                {
                    text.text = "Ready!\nAir tap in the cube to take a picture.";
                }
            }
            else
            {
                isReadyToCapturePhoto = false;

                if (text != null)
                {
                    text.text = "Unable to start photo mode!";
                }
            }
        }

        /// <summary>
        /// Takes a photo and attempts to load it into the scene using its location data.
        /// </summary>
        public void TakePhoto()
        {
            if (!isReadyToCapturePhoto || isCapturingPhoto)
            {
                return;
            }

            isCapturingPhoto = true;

            if (text != null)
            {
                text.text = "Taking picture...";
            }

            photoCaptureObject.TakePhotoAsync(OnPhotoCaptured);
        }

        private void OnPhotoCaptured(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            if (result.success)
            {
                if (text != null)
                {
                    text.text += "\nTook picture!";
                }

                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = $"Photo{numPhotos++}";
                quad.transform.parent = transform;

                float ratio = cameraResolution.height / (float)cameraResolution.width;
                quad.transform.localScale = new Vector3(quad.transform.localScale.x, quad.transform.localScale.x * ratio, quad.transform.localScale.z);

                Renderer quadRenderer = quad.GetComponent<Renderer>();
                quadRenderer.material = new Material(textureShader);
                Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
                photoCaptureFrame.UploadImageDataToTexture(targetTexture);
                quadRenderer.sharedMaterial.SetTexture("_MainTex", targetTexture);

                if (photoCaptureFrame.hasLocationData)
                {
                    photoCaptureFrame.TryGetCameraToWorldMatrix(out Matrix4x4 cameraToWorldMatrix);

                    Vector3 position = cameraToWorldMatrix.GetColumn(3) - cameraToWorldMatrix.GetColumn(2);
                    Quaternion rotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));

                    photoCaptureFrame.TryGetProjectionMatrix(Camera.main.nearClipPlane, Camera.main.farClipPlane, out Matrix4x4 projectionMatrix);

                    targetTexture.wrapMode = TextureWrapMode.Clamp;

                    quadRenderer.sharedMaterial.SetMatrix("_WorldToCameraMatrix", cameraToWorldMatrix.inverse);
                    quadRenderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);

                    quad.transform.position = position;
                    quad.transform.rotation = rotation;

                    if (text != null)
                    {
                        text.text += $"\nPosition: ({position.x}, {position.y}, {position.z})";
                        text.text += $"\nRotation: ({rotation.x}, {rotation.y}, {rotation.z}, {rotation.w})";
                    }
                }
                else
                {
                    if (text != null)
                    {
                        text.text += "\nNo location data :(";
                    }
                }
            }
            else
            {
                if (text != null)
                {
                    text.text += "\nPicture taking failed: " + result.hResult;
                }
            }

            isCapturingPhoto = false;
        }
    }
}
