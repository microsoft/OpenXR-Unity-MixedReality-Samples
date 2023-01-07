using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Provides access to a device's camera.
    /// </summary>
    /// <remarks>
    /// The <c>XRCameraSubsystem</c> links a Unity <c>Camera</c> to a device camera for video overlay (pass-thru
    /// rendering). It also allows developers to query for environmental light estimation, when available.
    /// </remarks>
    public class XRCameraSubsystem : SubsystemWithProvider<XRCameraSubsystem, XRCameraSubsystemDescriptor, XRCameraSubsystem.Provider>
    {
        /// <summary>
        /// Construct the <c>XRCameraSubsystem</c>.
        /// </summary>
        public XRCameraSubsystem() { }

        /// <summary>
        /// Gets the camera currently in use.
        /// </summary>
        /// <returns></returns>
        public Feature currentCamera => provider.currentCamera.Cameras();

        /// <summary>
        /// Get or set the requested camera (that is, the <see cref="Feature.AnyCamera"/> bits).
        /// </summary>
        public Feature requestedCamera
        {
            get => provider.requestedCamera;
            set => provider.requestedCamera = value.Cameras();
        }

        /// <summary>
        /// Interface for providing camera functionality for the implementation.
        /// </summary>
        public class Provider : SubsystemProvider<XRCameraSubsystem>
        {
            /// <summary>
            /// An instance of the <see cref="XRCpuImage.Api"/> used to operate on <see cref="XRCpuImage"/> objects.
            /// </summary>
            public virtual XRCpuImage.Api cpuImageApi => null;

            /// <summary>
            /// Property to be implemented by the provder to get the material used by <c>XRCameraSubsystem</c> to
            /// render the camera texture.
            /// </summary>
            /// <returns>
            /// The material to render the camera texture.
            /// </returns>
            public virtual Material cameraMaterial => null;

            /// <summary>
            /// Property to be implemented by the provider to determine whether camera permission has been granted.
            /// </summary>
            /// <value>
            /// <c>true</c> if camera permission has been granted. Otherwise, <c>false</c>.
            /// </value>
            public virtual bool permissionGranted => false;

            /// <summary>
            /// <c>True</c> if culling should be inverted during rendering. Some front-facing
            /// camera modes might require this.
            /// </summary>
            public virtual bool invertCulling => false;

            /// <summary>
            /// This property should get the actual camera facing direction.
            /// </summary>
            public virtual Feature currentCamera => Feature.None;

            /// <summary>
            /// This property should get or set the requested camera facing direction,
            /// that is, the <see cref="Feature.AnyCamera"/> bits.
            /// </summary>
            public virtual Feature requestedCamera
            {
                get => Feature.None;
                set { }
            }

            /// <summary>
            /// Method to be implemented by the provider to start the camera for the subsystem.
            /// </summary>
            public override void Start() { }

            /// <summary>
            /// Method to be implemented by the provider to stop the camera for the subsystem.
            /// </summary>
            public override void Stop() { }

            /// <summary>
            /// Method to be implemented by the provider to destroy the camera for the subsystem.
            /// </summary>
            public override void Destroy() { }

            /// <summary>
            /// Method to be implemented by the provider to get the camera frame for the subsystem.
            /// </summary>
            /// <param name="cameraParams">The current Unity <c>Camera</c> parameters.</param>
            /// <param name="cameraFrame">The current camera frame returned by the method.</param>
            /// <returns>
            /// <c>true</c> if the method successfully got a frame. Otherwise, <c>false</c>.
            /// </returns>
            public virtual bool TryGetFrame(
                XRCameraParams cameraParams,
                out XRCameraFrame cameraFrame)
            {
                cameraFrame = default(XRCameraFrame);
                return false;
            }

            /// <summary>
            /// Property to be implemented by the provider to get the current camera focus mode.
            /// </summary>
            public virtual bool autoFocusEnabled => false;

            /// <summary>
            /// Property to be implemented by the provider to get or set the focus mode for the camera.
            /// </summary>
            public virtual bool autoFocusRequested
            {
                get => false;
                set { }
            }

            /// <summary>
            /// Property to be implemented by the provider to get the current light estimation mode in use.
            /// </summary>
            public virtual Feature currentLightEstimation => Feature.None;

            /// <summary>
            /// Property to be implemented by the provider to get or set the light estimation mode.
            /// </summary>
            public virtual Feature requestedLightEstimation
            {
                get => Feature.None;
                set { }
            }

            /// <summary>
            /// Method to be implemented by the provider to get the camera intrinisics information.
            /// </summary>
            /// <param name="cameraIntrinsics">The camera intrinsics information returned from the method.</param>
            /// <returns>
            /// <c>true</c> if the method successfully gets the camera intrinsics information. Otherwise, <c>false</c>.
            /// </returns>
            public virtual bool TryGetIntrinsics(
                out XRCameraIntrinsics cameraIntrinsics)
            {
                cameraIntrinsics = default(XRCameraIntrinsics);
                return false;
            }

            /// <summary>
            /// Method to be implemented by the provider to query the supported camera configurations.
            /// </summary>
            /// <param name="defaultCameraConfiguration">A default value used to fill the returned array before copying
            /// in real values. This ensures future additions to this struct are backwards compatible.</param>
            /// <param name="allocator">The allocation strategy to use for the returned data.</param>
            /// <returns>
            /// The supported camera configurations.
            /// </returns>
            public virtual NativeArray<XRCameraConfiguration> GetConfigurations(XRCameraConfiguration defaultCameraConfiguration,
                                                                                Allocator allocator)
            {
                return new NativeArray<XRCameraConfiguration>(0, allocator);
            }

            /// <summary>
            /// Property to be implemented by the provider to query or set the current camera configuration.
            /// </summary>
            /// <value>
            /// The current camera configuration, if it exists. Otherise, <c>null</c>.
            /// </value>
            /// <exception cref="System.NotSupportedException">Thrown when setting the current configuration if the
            /// implementation does not support camera configurations.</exception>
            /// <exception cref="System.ArgumentException">Thrown when setting the current configuration if the given
            /// configuration is not a valid, supported camera configuration.</exception>
            /// <exception cref="System.InvalidOperationException">Thrown when setting the current configuration if the
            /// implementation is unable to set the current camera configuration.</exception>
            public virtual XRCameraConfiguration? currentConfiguration
            {
                get => null;
                set => throw new NotSupportedException("setting current camera configuration is not supported by this implementation");
            }

            /// <summary>
            /// Get the <see cref="XRTextureDescriptor"/>s associated with the current
            /// <see cref="XRCameraFrame"/>.
            /// </summary>
            /// <returns>The current texture descriptors.</returns>
            /// <param name="defaultDescriptor">A default value which should
            /// be used to fill the returned array before copying in the
            /// real values. This ensures future additions to this struct
            /// are backwards compatible.</param>
            /// <param name="allocator">The allocator to use when creating
            /// the returned <c>NativeArray</c>.</param>
            public virtual NativeArray<XRTextureDescriptor> GetTextureDescriptors(
                XRTextureDescriptor defaultDescriptor,
                Allocator allocator)
            {
                return new NativeArray<XRTextureDescriptor>(0, allocator);
            }

            /// <summary>
            /// Method to be implemented by the provider to get the enabled and disabled shader keywords for the
            /// material.
            /// </summary>
            /// <param name="enabledKeywords">The keywords to enable for the material.</param>
            /// <param name="disabledKeywords">The keywords to disable for the material.</param>
            public virtual void GetMaterialKeywords(out List<string> enabledKeywords, out List<string> disabledKeywords)
            {
                enabledKeywords = null;
                disabledKeywords = null;
            }

            /// <summary>
            /// Method to be implemented by the provider to query for the latest native camera image.
            /// </summary>
            /// <param name="cameraImageCinfo">The metadata required to construct a <see cref="XRCpuImage"/></param>
            /// <returns>
            /// <c>true</c> if the camera image is acquired. Otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera
            /// image.</exception>
            public virtual bool TryAcquireLatestCpuImage(out XRCpuImage.Cinfo cameraImageCinfo)
            {
                throw new NotSupportedException("getting camera image is not supported by this implementation");
            }

            /// <summary>
            /// Create the camera material from the given camera shader name.
            /// </summary>
            /// <param name="cameraShaderName">The name of the camera shader.</param>
            /// <returns>
            /// The created camera material shader.
            /// </returns>
            /// <exception cref="System.InvalidOperationException">Thrown if the shader cannot be found or if a
            /// material cannot be created for the shader.</exception>
            protected Material CreateCameraMaterial(string cameraShaderName)
            {
                var shader = Shader.Find(cameraShaderName);
                if (shader == null)
                {
                    throw new InvalidOperationException($"Could not find shader named '{cameraShaderName}' required "
                                                        + $"for video overlay on camera subsystem.");
                }

                Material material = new Material(shader);
                if (material == null)
                {
                    throw new InvalidOperationException($"Could not create a material for shader named "
                                                        + $"'{cameraShaderName}' required for video overlay on camera "
                                                        + $"subsystem.");
                }

                return material;
            }

            /// <summary>
            /// Method to be implemented by the provider to handle any required platform-specifc functionality
            /// immediately before rendering the camera background. This method will always be called on the render
            /// thread and should only be called by the code responsible for executing background rendering on
            /// mobile AR platforms.
            /// </summary>
            /// <param name="id">Platform specific identifier.</param>
            public virtual void OnBeforeBackgroundRender(int id) {}
        }

        /// <summary>
        /// Get the current focus mode in use by the provider.
        /// </summary>
        public bool autoFocusEnabled => provider.autoFocusEnabled;

        /// <summary>
        /// Get or set the focus mode for the camera.
        /// </summary>
        /// <value>
        /// The focus mode for the camera.
        /// </value>
        public bool autoFocusRequested
        {
            get => provider.autoFocusRequested;
            set => provider.autoFocusRequested = value;
        }

        /// <summary>
        /// Returns the current light estimation mode in use by the provider.
        /// </summary>
        /// <seealso cref="requestedLightEstimation"/>
        public Feature currentLightEstimation => provider.currentLightEstimation.LightEstimation();

        /// <summary>
        /// Get or set the requested light estimation mode.
        /// </summary>
        /// <value>
        /// The light estimation mode.
        /// </value>
        public Feature requestedLightEstimation
        {
            get => provider.requestedLightEstimation.LightEstimation();
            set => provider.requestedLightEstimation = value.LightEstimation();
        }

        /// <summary>
        /// Gets the <see cref="XRTextureDescriptor"/>s associated with the
        /// current frame. The caller owns the returned <c>NativeArray</c>
        /// and is responsible for calling <c>Dispose</c> on it.
        /// </summary>
        /// <returns>An array of texture descriptors.</returns>
        /// <param name="allocator">The allocator to use when creating
        /// the returned <c>NativeArray</c>.</param>
        public NativeArray<XRTextureDescriptor> GetTextureDescriptors(
            Allocator allocator)
        {
            return provider.GetTextureDescriptors(
                default(XRTextureDescriptor),
                allocator);
        }

        /// <summary>
        /// Get the material used by <c>XRCameraSubsystem</c> to render the camera texture.
        /// </summary>
        /// <value>
        /// The material to render the camera texture.
        /// </value>
        public Material cameraMaterial => provider.cameraMaterial;

        /// <summary>
        /// Method to be called on the render thread to handle any required platform-specifc functionality
        /// immediately before rendering the camera background. This method will always be called on the render
        /// thread and should only be called by the code responsible for executing background rendering on
        /// mobile AR platforms.
        /// </summary>
        /// <param name="id">Platform-specific identifier.</param>
        public void OnBeforeBackgroundRender(int id)
        {
            provider.OnBeforeBackgroundRender(id);
        }

        /// <summary>
        /// Returns the camera intrinsics information.
        /// </summary>
        /// <remarks>
        /// > [!NOTE]
        /// > The intrinsics may change each frame. You should call this each frame that you need intrinsics
        /// > in order to ensure you are using the intrinsics for the current frame.
        /// </remarks>
        /// <param name="cameraIntrinsics">The camera intrinsics information returned from the method.</param>
        /// <returns>
        /// <c>true</c> if the method successfully gets the camera intrinsics information. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics)
        {
            return provider.TryGetIntrinsics(out cameraIntrinsics);
        }

        /// <summary>
        /// Queries for the supported camera configurations.
        /// </summary>
        /// <param name="allocator">The allocation strategy to use for the returned data.</param>
        /// <returns>
        /// The supported camera configurations.
        /// </returns>
        public NativeArray<XRCameraConfiguration> GetConfigurations(Allocator allocator)
        {
            return provider.GetConfigurations(default(XRCameraConfiguration), allocator);
        }

        /// <summary>
        /// The current camera configuration.
        /// </summary>
        /// <value>
        /// The current camera configuration, if it exists. Otherise, <c>null</c>.
        /// </value>
        /// <exception cref="System.NotSupportedException">Thrown when setting the current configuration if the
        /// implementation does not support camera configurations.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when setting the current configuration if the given
        /// configuration is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when setting the current configuration if the given
        /// configuration is not a supported camera configuration.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when setting the current configuration if the
        /// implementation is unable to set the current camera configuration.</exception>
        public virtual XRCameraConfiguration? currentConfiguration
        {
            get => provider.currentConfiguration;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "cannot set the camera configuration to null");
                }

                provider.currentConfiguration = value;
            }
        }

        /// <summary>
        /// Set this to <c>true</c> to invert the culling mode during rendering. Some front-facing
        /// camera modes might require this.
        /// </summary>
        public bool invertCulling => provider.invertCulling;

        /// <summary>
        /// Get the latest frame from the provider.
        /// </summary>
        /// <param name="cameraParams">The Unity <c>Camera</c> parameters.</param>
        /// <param name="frame">The camera frame to be populated if the subsystem is running and successfully provides
        /// the latest camera frame.</param>
        /// <returns>
        /// <c>true</c> if the camera frame is successfully returned. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetLatestFrame(
            XRCameraParams cameraParams,
            out XRCameraFrame frame)
        {
            if (running && provider.TryGetFrame(cameraParams, out frame))
            {
                return true;
            }

            frame = default(XRCameraFrame);
            return false;
        }

        /// <summary>
        /// Determines whether camera permission has been granted.
        /// </summary>
        /// <value>
        /// <c>true</c> if camera permission has been granted. Otherwise, <c>false</c>.
        /// </value>
        public bool permissionGranted => provider.permissionGranted;

        /// <summary>
        /// Get the enabled and disabled shader keywords for the material.
        /// </summary>
        /// <param name="enabledKeywords">The keywords to enable for the material.</param>
        /// <param name="disabledKeywords">The keywords to disable for the material.</param>
        public void GetMaterialKeywords(out List<string> enabledKeywords, out List<string> disabledKeywords)
            => provider.GetMaterialKeywords(out enabledKeywords, out disabledKeywords);

        /// <summary>
        /// Attempts to acquire the latest camera image. This provides direct access to the raw pixel data, as well as
        /// to utilities to convert to RGB and Grayscale formats. This method is obsolete. Use
        /// <see cref="TryAcquireLatestCpuImage"/> instead.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="XRCpuImage"/> must be disposed to avoid resource leaks.
        /// </remarks>
        /// <param name="cpuImage">A valid <see cref="XRCpuImage"/> if this method returns <c>true</c>.</param>
        /// <returns>Returns `true` if the image was acquired. Returns `false` otherwise.</returns>
        /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera image.
        /// </exception>
        [Obsolete("Use TryAcquireLatestCpuImage instead. (2020-05-19)")]
        public bool TryGetLatestImage(out XRCpuImage cpuImage) => TryAcquireLatestCpuImage(out cpuImage);

        /// <summary>
        /// Attempts to acquire the latest camera image. This provides direct access to the raw pixel data, as well as
        /// to utilities to convert to RGB and Grayscale formats.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="XRCpuImage"/> must be disposed to avoid resource leaks.
        /// </remarks>
        /// <param name="cpuImage">A valid <see cref="XRCpuImage"/> if this method returns <c>true</c>.</param>
        /// <returns>Returns `true` if the image was acquired. Returns `false` otherwise.</returns>
        /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera image.
        /// </exception>
        public bool TryAcquireLatestCpuImage(out XRCpuImage cpuImage)
        {
            if (provider.cpuImageApi != null && provider.TryAcquireLatestCpuImage(out var cinfo))
            {
                cpuImage = new XRCpuImage(provider.cpuImageApi, cinfo);
                return true;
            }

            cpuImage = default;
            return false;
        }

        /// <summary>
        /// Registers a camera subsystem implementation based on the given subsystem parameters.
        /// </summary>
        /// <param name="cameraSubsystemParams">The parameters defining the camera subsystem functionality implemented
        /// by the subsystem provider.</param>
        /// <returns>
        /// <c>true</c> if the subsystem implementation is registered. Otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the values specified in the
        /// <see cref="XRCameraSubsystemCinfo"/> parameter are invalid. Typically, this will occur
        /// <list type="bullet">
        /// <item>
        /// <description>if <see cref="XRCameraSubsystemCinfo.id"/> is <c>null</c> or empty</description>
        /// </item>
        /// <item>
        /// <description>if <see cref="XRCameraSubsystemCinfo.implementationType"/> is <c>null</c></description>
        /// </item>
        /// <item>
        /// <description>if <see cref="XRCameraSubsystemCinfo.implementationType"/> does not derive from the
        /// <see cref="XRCameraSubsystem"/> class
        /// </description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool Register(XRCameraSubsystemCinfo cameraSubsystemParams)
        {
            XRCameraSubsystemDescriptor cameraSubsystemDescriptor = XRCameraSubsystemDescriptor.Create(cameraSubsystemParams);
            SubsystemDescriptorStore.RegisterDescriptor(cameraSubsystemDescriptor);
            return true;
        }
    }
}
