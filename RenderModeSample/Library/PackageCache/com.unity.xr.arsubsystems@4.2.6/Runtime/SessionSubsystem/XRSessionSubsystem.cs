using System;
using Unity.Collections;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// This subsystem controls the lifecycle of an XR session. Some platforms,
    /// particularly those that have non-XR modes, need to be able to turn the
    /// session on and off to enter and exit XR modes of operation.
    /// </summary>
    public class XRSessionSubsystem
        : SubsystemWithProvider<XRSessionSubsystem, XRSessionSubsystemDescriptor, XRSessionSubsystem.Provider>
    {
        /// <summary>
        /// Returns an implementation-defined pointer associated with the session.
        /// </summary>
        public IntPtr nativePtr => provider.nativePtr;

        /// <summary>
        /// Returns a unique session identifier for this session.
        /// </summary>
        public Guid sessionId => provider.sessionId;

        /// <summary>
        /// Asynchronously retrieves the <see cref="SessionAvailability"/>. Used to determine whether
        /// the current device supports XR and if the necessary software is installed.
        /// </summary>
        /// <remarks>
        /// This platform-agnostic method is typically implemented by a platform-specific package.
        /// </remarks>
        /// <returns>A <see cref="Promise{SessionAvailability}"/> which can be used to determine when the
        /// availability has been determined and to retrieve the result.</returns>
        public Promise<SessionAvailability> GetAvailabilityAsync() => provider.GetAvailabilityAsync();

        /// <summary>
        /// Asynchronously attempts to install XR software on the current device.
        /// Throws if <see cref="XRSessionSubsystemDescriptor.supportsInstall"/> is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// This platform-agnostic method is typically implemented by a platform-specific package.
        /// </remarks>
        /// <returns>A <see cref="Promise{SessionInstallationStatus}"/> which can be used to determine when the
        /// installation completes and to retrieve the result.</returns>
        public Promise<SessionInstallationStatus> InstallAsync()
        {
            if (!subsystemDescriptor.supportsInstall)
                throw new NotSupportedException("InstallAsync is not supported on this platform.");

            return provider.InstallAsync();
        }

        /// <summary>
        /// Do not call this directly. Call create on a valid <see cref="XRSessionSubsystemDescriptor"/> instead.
        /// </summary>
        public XRSessionSubsystem()
        {
            m_ConfigurationChooser = m_DefaultConfigurationChooser;
        }

        /// <summary>
        /// Restarts a session. [Stop](xref:UnityEngine.Subsystem.Stop) and [Start](xref:UnityEngine.Subsystem.Start)
        /// pause and resume a session, respectively. <c>Restart</c> resets the session state and clears and any
        /// detected trackables.
        /// </summary>
        public void Reset() => provider.Reset();

        /// <summary>
        /// Determines the <see cref="Configuration"/> the session will use given the requested <paramref name="features"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method uses the current <see cref="configurationChooser"/> to choose a configuration using the requested
        /// <paramref name="features"/> and the capabilities of the available configurations. The configuration chooser
        /// is customizable, see <see cref="ConfigurationChooser"/>. If you do not set a configuration chooser, the
        /// <see cref="DefaultConfigurationChooser"/> is used.
        /// </para><para>
        /// You can use this method to determine what would happen if you were to enable or disable a particular feature.
        /// For example, you can use this method to determine which features would be enabled if you were to change the
        /// camera mode from <see cref="Feature.WorldFacingCamera"/> to <see cref="Feature.UserFacingCamera"/>.
        /// </para>
        /// </remarks>
        /// <param name="features">A set of requested <see cref="Feature"/>s.</param>
        /// <returns>The <see cref="Configuration"/> the session would use given the requested <paramref name="features"/>,
        /// or <c>null</c> if configuration introspection is not available.</returns>
        /// <seealso cref="Configuration"/>
        /// <seealso cref="ConfigurationChooser"/>
        /// <seealso cref="ConfigurationDescriptor"/>
        /// <seealso cref="Feature"/>
        public Configuration? DetermineConfiguration(Feature features)
        {
            var descriptors = GetConfigurationDescriptors(Allocator.Temp);
            if (descriptors.IsCreated)
            {
                using (descriptors)
                {
                    if (descriptors.Length > 0)
                    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        if (m_FirstUpdate)
                        {
                            var sb = new System.Text.StringBuilder();
                            foreach (var descriptor in descriptors)
                            {
                                sb.Append($"Configuration Descriptor {HexString(descriptor.identifier)} (rank {descriptor.rank}): {descriptor.capabilities.ToStringList()}\n");
                            }
                            Debug.Log(sb.ToString());
                            m_FirstUpdate = false;
                        }
#endif
                        return m_ConfigurationChooser.ChooseConfiguration(descriptors, features);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Trigger the session's update loop.
        /// </summary>
        /// <param name="updateParams">Data needed by the session to perform its update.</param>
        public void Update(XRSessionUpdateParams updateParams)
        {
            currentConfiguration = DetermineConfiguration(requestedFeatures);
            if (currentConfiguration.HasValue)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                DebugPrintConfigurationChange(currentConfiguration.Value, requestedFeatures);
#endif
                provider.Update(updateParams, currentConfiguration.Value);
            }
            else
            {
                provider.Update(updateParams);
            }
        }

        /// <summary>
        /// The current <see cref="Configuration"/> in use by the session.
        /// </summary>
        /// <seealso cref="XRSessionSubsystem.DetermineConfiguration(Feature)"/>
        /// <seealso cref="Feature"/>
        public Configuration? currentConfiguration { get; private set; }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        unsafe string HexString(IntPtr ptr) => sizeof(IntPtr) == 4 ? $"0x{ptr.ToInt32():x}" : $"0x{ptr.ToInt64():x}";

        void DebugPrintConfigurationChange(Configuration configuration, Feature desiredFeatures)
        {
            if (configuration != m_PreviousConfiguration ||
                desiredFeatures != m_PreviousDesiredFeatures)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append($"Using session configuration {HexString(configuration.descriptor.identifier)}");
                if (configuration.descriptor.identifier == m_PreviousConfiguration.descriptor.identifier)
                {
                    sb.Append(" (unchanged)");
                }
                sb.Append("\n\tRequested Features: ");
                sb.Append(desiredFeatures.ToStringList());
                sb.Append("\n\tSupported Features: ");
                sb.Append(configuration.features.ToStringList());
                sb.Append("\n\tRequested features not satisfied: ");
                sb.Append(desiredFeatures.SetDifference(configuration.features).ToStringList());
                Debug.Log(sb.ToString());
                m_PreviousConfiguration = configuration;
                m_PreviousDesiredFeatures = desiredFeatures;
            }
        }
        Feature m_PreviousDesiredFeatures;
        Configuration m_PreviousConfiguration;
        bool m_FirstUpdate = true;
#endif

        /// <summary>
        /// Get the requested <see cref="Feature"/>s. These are used to determine the session's <see cref="Configuration"/>.
        /// </summary>
        /// <seealso cref="XRSessionSubsystem.DetermineConfiguration(Feature)"/>
        public Feature requestedFeatures => provider.requestedFeatures;

        /// <summary>
        /// Get the list of supported configuration descriptors. The session can have multiple, discrete modes of operation.
        /// A configuration represents the capabilities of a mode of operation, which can be a subset of the session's overal
        /// capabilities. That is, the session might support many features, but not all at the same time. This is used by
        /// <see cref="XRSessionSubsystem.DetermineConfiguration(Feature)"/> to determine the best configuration given a set
        /// of requested features.
        /// </summary>
        /// <param name="allocator">The [allocator](https://docs.unity3d.com/ScriptReference/Unity.Collections.Allocator.html)
        /// to use for the returned [NativeArray](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html).</param>
        /// <returns>Allocates a new [NativeArray](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html)
        /// and populates it with descriptors describing the supported configurations. The caller
        /// owns the memory and is responsible for calling [Dispose](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.Dispose.html)
        /// on the <c>NativeArray</c>.</returns>
        public NativeArray<ConfigurationDescriptor> GetConfigurationDescriptors(Allocator allocator) => provider.GetConfigurationDescriptors(allocator);

        /// <summary>
        /// Should be invoked when the application is paused.
        /// </summary>
        public void OnApplicationPause() =>  provider.OnApplicationPause();

        /// <summary>
        /// Should be invoked when the application is resumed.
        /// </summary>
        public void OnApplicationResume() => provider.OnApplicationResume();

        /// <summary>
        /// Gets the <see cref="TrackingState"/> for the session.
        /// </summary>
        public TrackingState trackingState => provider.trackingState;

        /// <summary>
        /// The requested tracking mode. Query for support with <c>SubsystemDescriptor.supportedTrackingModes</c>.
        /// </summary>
        public Feature requestedTrackingMode
        {
            get => provider.requestedTrackingMode.TrackingModes();
            set => provider.requestedTrackingMode = value.TrackingModes();
        }

        /// <summary>
        /// Get the current tracking mode in use by the subsystem.
        /// </summary>
        public Feature currentTrackingMode => provider.currentTrackingMode.TrackingModes();

        /// <summary>
        /// Get or set the <see cref="ConfigurationChooser"/> used by
        /// <see cref="XRSessionSubsystem.DetermineConfiguration(Feature)"/>.
        /// If set to <c>null</c>, the <see cref="DefaultConfigurationChooser"/>
        /// is used.
        /// </summary>
        public ConfigurationChooser configurationChooser
        {
            get => m_ConfigurationChooser;
            set
            {
                if (ReferenceEquals(value, null))
                {
                    m_ConfigurationChooser = m_DefaultConfigurationChooser;
                }
                else
                {
                    m_ConfigurationChooser = value;
                }
            }
        }
        ConfigurationChooser m_DefaultConfigurationChooser = new DefaultConfigurationChooser();

        ConfigurationChooser m_ConfigurationChooser;

        /// <summary>
        /// Gets the <see cref="NotTrackingReason"/> for the session.
        /// </summary>
        public NotTrackingReason notTrackingReason => provider.notTrackingReason;

        /// <summary>
        /// Whether the AR session update is synchronized with the Unity frame rate.
        /// If <c>true</c>, <see cref="Update(XRSessionUpdateParams)"/> should block until the next AR frame is available.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Thrown if <see cref="XRSessionSubsystemDescriptor.supportsMatchFrameRate"/> is <c>False</c>.</exception>
        public bool matchFrameRateEnabled
        {
            get => provider.matchFrameRateEnabled;
        }

        /// <summary>
        /// Get or set whether the match frame rate feature should be enabled.
        /// When enabled, the AR session update is synchronized with the Unity frame rate.
        /// </summary>
        /// <seealso cref="matchFrameRateEnabled"/>
        public bool matchFrameRateRequested
        {
            get => provider.matchFrameRateRequested;
            set => provider.matchFrameRateRequested = value;
        }

        /// <summary>
        /// The native update rate of the AR Session.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Thrown if <see cref="XRSessionSubsystemDescriptor.supportsMatchFrameRate"/> is <c>False</c>.</exception>
        public int frameRate => provider.frameRate;

        /// <summary>
        /// The API this subsystem uses to interop with
        /// different provider implementations.
        /// </summary>
        public class Provider : SubsystemProvider<XRSessionSubsystem>
        {
            /// <summary>
            /// Invoked to start or resume a session. This is different from <see cref="OnApplicationResume"/>.
            /// </summary>
            public override void Start() {}

            /// <summary>
            /// Invoked to pause a running session. This is different from <see cref="OnApplicationPause"/>.
            /// </summary>
            public override void Stop() {}

            /// <summary>
            /// Perform any per-frame update logic here.
            /// </summary>
            /// <param name="updateParams">Parameters about the current state that may be needed to inform the session.</param>
            public virtual void Update(XRSessionUpdateParams updateParams) { }

            /// <summary>
            /// Perform any per-frame update logic here. The session should use the configuration indicated by
            /// <paramref name="configuration.descriptor.identifier"/>, which should be one of the ones returned
            /// by <see cref="GetConfigurationDescriptors(Unity.Collections.Allocator)"/>.
            /// </summary>
            /// <param name="updateParams">Parameters about the current state that might be needed to inform the session.</param>
            /// <param name="configuration">The configuration the session should use.</param>
            public virtual void Update(XRSessionUpdateParams updateParams, Configuration configuration) { }

            /// <summary>
            /// Should return the features requested by the enabling of other <c>Subsystem</c>s.
            /// </summary>
            public virtual Feature requestedFeatures => Feature.None;

            /// <summary>
            /// Get or set the requested tracking mode (for example, the <see cref="Feature.AnyTrackingMode"/> bits).
            /// </summary>
            public virtual Feature requestedTrackingMode
            {
                get => Feature.None;
                set {}
            }

            /// <summary>
            /// Get the current tracking mode (for example, the <see cref="Feature.AnyTrackingMode"/> bits).
            /// </summary>
            public virtual Feature currentTrackingMode => Feature.None;

            /// <summary>
            /// This getter should allocate a new <c>NativeArray</c> using <paramref name="allocator"/>
            /// and populate it with the supported <see cref="ConfigurationDescriptor"/>s.
            /// </summary>
            /// <param name="allocator">The <c>[Allocator](https://docs.unity3d.com/ScriptReference/Unity.Collections.Allocator.html)</c>
            /// to use to create the returned <c>NativeArray</c>.</param>
            /// <returns>A newly allocated <c>NativeArray</c> of <see cref="ConfigurationDescriptor"/>s that describes the capabilities
            /// of all the supported configurations.</returns>
            public virtual NativeArray<ConfigurationDescriptor> GetConfigurationDescriptors(Allocator allocator) => default;

            /// <summary>
            /// Stop the session and destroy all associated resources.
            /// </summary>
            public override void Destroy() { }

            /// <summary>
            /// Reset the session. The behavior should be equivalent to destroying and recreating the session.
            /// </summary>
            public virtual void Reset() { }

            /// <summary>
            /// Invoked when the application is paused.
            /// </summary>
            public virtual void OnApplicationPause() { }

            /// <summary>
            /// Invoked when the application is resumed.
            /// </summary>
            public virtual void OnApplicationResume() { }

            /// <summary>
            /// Get a pointer to an object associated with the session.
            /// Callers should be able to manipulate the session in their own code using this.
            /// </summary>
            public virtual IntPtr nativePtr => IntPtr.Zero;

            /// <summary>
            /// Get the session's availability, such as whether the platform supports XR.
            /// </summary>
            /// <returns>A <see cref="Promise{T}"/> that the caller can yield on until availability is determined.</returns>
            public virtual Promise<SessionAvailability> GetAvailabilityAsync()
            {
                return Promise<SessionAvailability>.CreateResolvedPromise(SessionAvailability.None);
            }

            /// <summary>
            /// Attempt to update or install necessary XR software. Will only be called if
            /// <see cref="XRSessionSubsystemDescriptor.supportsInstall"/> is true.
            /// </summary>
            /// <returns></returns>
            public virtual Promise<SessionInstallationStatus> InstallAsync()
            {
                return Promise<SessionInstallationStatus>.CreateResolvedPromise(SessionInstallationStatus.ErrorInstallNotSupported);
            }

            /// <summary>
            /// Get the <see cref="TrackingState"/> for the session.
            /// </summary>
            public virtual TrackingState trackingState => TrackingState.None;

            /// <summary>
            /// Get the <see cref="NotTrackingReason"/> for the session.
            /// </summary>
            public virtual NotTrackingReason notTrackingReason => NotTrackingReason.Unsupported;

            /// <summary>
            /// Get a unique identifier for this session.
            /// </summary>
            public virtual Guid sessionId => Guid.Empty;

            /// <summary>
            /// Whether the AR session update is synchronized with the Unity frame rate.
            /// If <c>true</c>, <see cref="Update(XRSessionUpdateParams)"/> will block until the next AR frame is available.
            /// </summary>
            public virtual bool matchFrameRateEnabled => false;

            /// <summary>
            /// Whether the AR session update should be synchronized with the Unity frame rate.
            /// If <c>true</c>, <see cref="Update(XRSessionUpdateParams)"/> should block until the next AR frame is available.
            /// Must be implemented if
            /// <see cref="XRSessionSubsystemDescriptor.supportsMatchFrameRate"/>
            /// is <c>True</c>.
            /// </summary>
            public virtual bool matchFrameRateRequested
            {
                get => false;
                set
                {
                    if (value)
                    {
                        throw new NotSupportedException("Matching frame rate is not supported.");
                    }
                }
            }

            /// <summary>
            /// The native update rate of the AR Session. Must be implemented if
            /// <see cref="XRSessionSubsystemDescriptor.supportsMatchFrameRate"/>
            /// is <c>True</c>.
            /// </summary>
            public virtual int frameRate =>
                throw new NotSupportedException("Querying the frame rate is not supported by this session subsystem.");
        }
    }
}
