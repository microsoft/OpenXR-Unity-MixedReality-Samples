using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

using Object = UnityEngine.Object;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// This class creates, maintains, and destroys environment probe GameObject components as the
    /// <c>XREnvironmentProbeSubsystem</c> provides updates from environment probes as they are detected in the
    /// environment.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ARUpdateOrder.k_EnvironmentProbeManager)]
    [HelpURL(HelpUrls.ApiWithNamespace + nameof(AREnvironmentProbeManager) + ".html")]
    public sealed class AREnvironmentProbeManager : ARTrackableManager<
        XREnvironmentProbeSubsystem,
        XREnvironmentProbeSubsystemDescriptor,
        XREnvironmentProbeSubsystem.Provider,
        XREnvironmentProbe,
        AREnvironmentProbe>
    {
        /// <summary>
        /// A property of the environment probe subsystem that, if enabled, automatically generates environment probes
        /// for the scene.
        /// This method is obsolete.
        /// Use <see cref="automaticPlacementRequested"/> or <see cref="automaticPlacementEnabled"/> instead.
        /// </summary>
        /// <value>
        /// <c>true</c> if automatic environment probe placement is enabled. Otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Use automaticPlacementRequested or automaticPlacementEnabled instead. (2020-01-16)")]
        public bool automaticPlacement
        {
            get => m_AutomaticPlacement;
            set => automaticPlacementRequested = value;
        }

        bool supportsAutomaticPlacement => descriptor?.supportsAutomaticPlacement == true;

        /// <summary>
        /// If enabled, requests automatic generation of environment probes for the scene.
        /// </summary>
        /// <value>
        /// <c>true</c> if automatic environment probe placement is requested. Otherwise, <c>false</c>.
        /// </value>
        public bool automaticPlacementRequested
        {
            get => supportsAutomaticPlacement ? subsystem.automaticPlacementRequested : m_AutomaticPlacement;
            set
            {
                m_AutomaticPlacement = value;
                SetAutomaticPlacementStateOnSubsystem();
             }
        }

        /// <summary>
        /// <c>true</c> if automatic placement is enabled on the subsystem.
        /// </summary>
        public bool automaticPlacementEnabled => supportsAutomaticPlacement ? subsystem.automaticPlacementEnabled : false;

        [SerializeField]
        [Tooltip("Whether environment probes should be automatically placed in the environment (if supported).")]
        bool m_AutomaticPlacement = true;

        /// <summary>
        /// Specifies the texture filter mode to be used with the environment texture.
        /// </summary>
        /// <value>
        /// The texture filter mode to be used with the environment texture.
        /// </value>
        public FilterMode environmentTextureFilterMode
        {
            get => m_EnvironmentTextureFilterMode;
            set => m_EnvironmentTextureFilterMode = value;
        }
        [SerializeField]
        [Tooltip("The texture filter mode to be used with the reflection probe environment texture.")]
        FilterMode m_EnvironmentTextureFilterMode = FilterMode.Trilinear;

        /// <summary>
        /// Specifies whether the environment textures should be returned as HDR textures.
        /// </summary>
        /// <value>
        /// <c>true</c> if the environment textures should be returned as HDR textures. Otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Use environmentTextureHDRRequested or environmentTextureHDREnabled instead. (2020-01-16)")]
        public bool environmentTextureHDR
        {
            get => m_EnvironmentTextureHDR;
            set => environmentTextureHDRRequested = value;
        }
        [SerializeField]
        [Tooltip("Whether the environment textures should be returned as HDR textures.")]
        bool m_EnvironmentTextureHDR = true;

        bool supportsEnvironmentTextureHDR => descriptor?.supportsEnvironmentTextureHDR == true;

        /// <summary>
        /// Get or set whether high dynamic range environment textures are requested.
        /// </summary>
        /// <value></value>
        public bool environmentTextureHDRRequested
        {
            get => supportsEnvironmentTextureHDR ? subsystem.environmentTextureHDRRequested : m_EnvironmentTextureHDR;
            set
            {
                m_EnvironmentTextureHDR = value;
                SetEnvironmentTextureHDRStateOnSubsystem();
            }
        }

        /// <summary>
        /// Queries whether environment textures will be provided with high dynamic range.
        /// </summary>
        public bool environmentTextureHDREnabled => supportsEnvironmentTextureHDR ? subsystem.environmentTextureHDREnabled : false;

        /// <summary>
        /// Specifies a debug Prefab that will be attached to all environment probes.
        /// </summary>
        /// <value>
        /// A debug Prefab that will be attached to all environment probes.
        /// </value>
        /// <remarks>
        /// Setting a debug Prefab allows for these environment probes to be more readily visualized but is not
        /// required for normal operation of this manager. This script will automatically create reflection probes for
        /// all environment probes reported by the <c>XREnvironmentProbeSubsystem</c>.
        /// </remarks>
        public GameObject debugPrefab
        {
            get => m_DebugPrefab;
            set => m_DebugPrefab = value;
        }
        [SerializeField]
        [Tooltip("A debug prefab that allows for these environment probes to be more readily visualized.")]
        GameObject m_DebugPrefab;

        /// <summary>
        /// Invoked once per frame with lists of environment probes that have been added, updated, and removed since the last frame.
        /// </summary>
        public event Action<AREnvironmentProbesChangedEvent> environmentProbesChanged;

        /// <summary>
        /// Attempts to find the environment probe matching the trackable ID currently in the scene.
        /// </summary>
        /// <param name='trackableId'>The trackable ID of an environment probe to search for.</param>
        /// <returns>
        /// Environment probe in the scene matching the <paramref name="trackableId"/>, or <c>null</c> if no matching
        /// environment probe is found.
        /// </returns>
        public AREnvironmentProbe GetEnvironmentProbe(TrackableId trackableId)
        {
            if (m_Trackables.TryGetValue(trackableId, out var environmentProbe))
                return environmentProbe;

            return null;
        }

        /// <summary>
        /// Creates a new environment probe at <paramref name="pose"/> with <paramref name="scale"/> and <paramref name="size"/>
        /// if supported by the subsystem. Use the <see cref="SubsystemLifecycleManager{TSubsystem,TSubsystemDescriptor,TProvider}.descriptor"/>'s
        /// `supportsManualPlacement` property to determine support for this feature. If successful, a new
        /// <c>GameObject</c> with an <see cref="AREnvironmentProbe"/> is created
        /// immediately; however, the provider might not report the environment probe as added until a future frame. Check the
        /// status of the probe by inspecting its
        /// <see cref="ARTrackable{TSessionRelativeData,TTrackable}.pending"/> property.
        /// </summary>
        /// <param name="pose">The position and rotation at which to create the new environment probe.</param>
        /// <param name="scale">The scale of the new environment probe.</param>
        /// <param name="size">The size (dimensions) of the new environment probe.</param>
        /// <returns>A new <see cref="AREnvironmentProbe"/> if successful, otherwise <c>null</c>.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if this manager is not enabled</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if this manager has no subsystem.</exception>
        /// <exception cref="System.NotSupportedException">Thrown if manual placement is not supported by this subsystem.
        /// Check for support on the <see cref="SubsystemLifecycleManager{TSubsystem,TSubsystemDescriptor,TProvider}.descriptor"/>'s
        ///     `supportsManualPlacement` property.</exception>
        [Obsolete("Add an environment probe using AddComponent<" + nameof(AREnvironmentProbe) + ">(). (2020-10-06)")]
        public AREnvironmentProbe AddEnvironmentProbe(Pose pose, Vector3 scale, Vector3 size)
        {
            if (!enabled)
                throw new InvalidOperationException("Cannot create an environment probe from a disabled environment probe manager.");

            if (subsystem == null)
                throw new InvalidOperationException("Environment probe manager has no subsystem. Enable the manager first.");

            if (!descriptor.supportsManualPlacement)
                throw new NotSupportedException("Manual environment probe placement is not supported by this subsystem.");

            var sessionRelativePose = sessionOrigin.trackablesParent.InverseTransformPose(pose);
            if (subsystem.TryAddEnvironmentProbe(sessionRelativePose, scale, size, out var sessionRelativeData))
            {
                var probe = CreateTrackableImmediate(sessionRelativeData);
                probe.placementType = AREnvironmentProbePlacementType.Manual;
                return probe;
            }

            return null;
        }

        internal bool TryAddEnvironmentProbe(AREnvironmentProbe probe)
        {
            if (!CanBeAddedToSubsystem(probe))
                return false;

            var reflectionProbe = probe.GetComponent<ReflectionProbe>();
            if (reflectionProbe == null)
                throw new InvalidOperationException($"Each {nameof(AREnvironmentProbe)} requires a {nameof(ReflectionProbe)} component.");

            if (!descriptor.supportsManualPlacement)
                throw new NotSupportedException("Manual environment probe placement is not supported by this subsystem.");

            var probeTransform = probe.transform;
            var trackablesParent = sessionOrigin.trackablesParent;
            var poseInSessionSpace = trackablesParent.InverseTransformPose(new Pose(probeTransform.position, probeTransform.rotation));

            var worldToLocalSession = trackablesParent.worldToLocalMatrix;
            var localToWorldProbe = probeTransform.localToWorldMatrix;

            // We want to calculate the "local-to-parent" of the probe if the session origin were its parent.
            //     LTW_session * LTP_probe = LTW_probe
            // =>  LTP_probe = inverse(LTW_session) * LTW_probe
            var localToParentProbe = worldToLocalSession * localToWorldProbe;
            var sessionSpaceScale = localToParentProbe.lossyScale;

            if (subsystem.TryAddEnvironmentProbe(poseInSessionSpace, sessionSpaceScale, reflectionProbe.size, out var sessionRelativeData))
            {
                CreateTrackableFromExisting(probe, sessionRelativeData);
                probe.placementType = AREnvironmentProbePlacementType.Manual;
                return probe;
            }

            return false;
        }

        /// <summary>
        /// Remove an existing environment probe. Support for this feature is provider-specific. Check for support with
        /// the <see cref="SubsystemLifecycleManager{TSubsystem,TSubsystemDescriptor,TProvider}.descriptor"/>'s
        /// `supportsRemovalOfManual` and `supportsRemovalOfAutomatic` properties.
        /// </summary>
        /// <param name="probe">The environment probe to remove</param>
        /// <returns><c>true</c> if the environment probe was removed, otherwise <c>false</c>.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if this manager is not enabled.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if
        ///     <see cref="SubsystemLifecycleManager{TSubsystem,TSubsystemDescriptor,TProvider}.subsystem"/> is `null`.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="probe"/> is `null`.</exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the environment probe was manually placed, but removal of manually placed probes is not supported.
        /// You can check for this case with <see cref="AREnvironmentProbe.placementType"/> and the
        /// <see cref="SubsystemLifecycleManager{TSubsystem,TSubsystemDescriptor,TProvider}.descriptor"/>'s
        /// 'supportsRemovalOfManual` property.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the environment probe was automatically placed, but removal of automatically placed probes is not supported.
        /// You can check for this case with <see cref="AREnvironmentProbe.placementType"/> and the
        /// <see cref="SubsystemLifecycleManager{TSubsystem,TSubsystemDescriptor,TProvider}.descriptor"/>'s
        /// `supportsRemovalOfAutomatic` property.
        /// </exception>
        [Obsolete("Call Destroy() on the " + nameof(AREnvironmentProbe) + " component to remove it. (2020-10-06)")]
        public bool RemoveEnvironmentProbe(AREnvironmentProbe probe)
        {
            if (!enabled)
                throw new InvalidOperationException("Cannot remove an environment probe from a disabled environment probe manager.");

            if (subsystem == null)
                throw new InvalidOperationException("Environment probe manager has no subsystem. Enable the manager first.");

            if (probe == null)
                throw new ArgumentNullException(nameof(probe));

            var desc = descriptor;

            if ((probe.placementType == AREnvironmentProbePlacementType.Manual) && !desc.supportsRemovalOfManual)
                throw new InvalidOperationException("Removal of manually placed environment probes is not supported by this subsystem.");

            if ((probe.placementType == AREnvironmentProbePlacementType.Automatic) && !desc.supportsRemovalOfAutomatic)
                throw new InvalidOperationException("Removal of automatically placed environment probes is not supported by this subsystem.");

            if (subsystem.RemoveEnvironmentProbe(probe.trackableId))
            {
                DestroyPendingTrackable(probe.trackableId);
                return true;
            }

            return false;
        }

        internal bool TryRemoveEnvironmentProbe(AREnvironmentProbe probe)
        {
            if (probe == null)
                throw new ArgumentNullException(nameof(probe));

            if (subsystem == null)
                return false;

            var desc = descriptor;

            if ((probe.placementType == AREnvironmentProbePlacementType.Manual) && !desc.supportsRemovalOfManual)
                throw new InvalidOperationException("Removal of manually placed environment probes is not supported by this subsystem.");

            if ((probe.placementType == AREnvironmentProbePlacementType.Automatic) && !desc.supportsRemovalOfAutomatic)
                throw new InvalidOperationException("Removal of automatically placed environment probes is not supported by this subsystem.");

            if (subsystem.RemoveEnvironmentProbe(probe.trackableId))
            {
                if (m_PendingAdds.ContainsKey(probe.trackableId))
                {
                    m_PendingAdds.Remove(probe.trackableId);
                    m_Trackables.Remove(probe.trackableId);
                }

                probe.pending = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// The name of the `GameObject` for each instantiated <see cref="AREnvironmentProbe"/>.
        /// </summary>
        protected override string gameObjectName => nameof(AREnvironmentProbe);

        /// <summary>
        /// Gets the prefab that should be instantiated for each <see cref="AREnvironmentProbe"/>. May be `null`.
        /// </summary>
        /// <returns>The prefab that should be instantiated for each <see cref="AREnvironmentProbe"/>.</returns>
        protected override GameObject GetPrefab() => m_DebugPrefab;

        /// <summary>
        /// Enables the environment probe functionality by registering listeners for the environment probe events, if
        /// the <c>XREnvironmentProbeSubsystem</c> exists, and enabling environment probes in the AR subsystem manager.
        /// </summary>
        protected override void OnBeforeStart()
        {
            SetAutomaticPlacementStateOnSubsystem();
            SetEnvironmentTextureHDRStateOnSubsystem();
        }

        /// <summary>
        /// Destroys any game objects created by this environment probe manager for each environment probe, and clears
        /// the mapping of environment probes.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var kvp in m_Trackables)
            {
                var environmentProbe = kvp.Value;
                Object.Destroy(environmentProbe.gameObject);
            }
        }

        /// <summary>
        /// Invoked when the base class detects trackable changes.
        /// </summary>
        /// <param name="added">The list of added <see cref="AREnvironmentProbe"/>.</param>
        /// <param name="updated">The list of updated <see cref="AREnvironmentProbe"/>.</param>
        /// <param name="removed">The list of removed <see cref="AREnvironmentProbe"/>.</param>
        protected override void OnTrackablesChanged(
            List<AREnvironmentProbe> added,
            List<AREnvironmentProbe> updated,
            List<AREnvironmentProbe> removed)
        {
            if (environmentProbesChanged != null)
            {
                using (new ScopedProfiler("OnEnvironmentProbesChanged"))
                {
                    environmentProbesChanged(new AREnvironmentProbesChangedEvent(added, updated, removed));
                }
            }
        }

        /// <summary>
        /// Invoked when an <see cref="AREnvironmentProbe"/> is created.
        /// </summary>
        /// <param name="probe">The <see cref="AREnvironmentProbe"/> that was just created.</param>
        protected override void OnCreateTrackable(AREnvironmentProbe probe)
        {
            probe.environmentTextureFilterMode = m_EnvironmentTextureFilterMode;
        }

        /// <summary>
        /// Sets the current state of the <see cref="automaticPlacement"/> property to the
        /// <c>XREnvironmentProbeSubsystem</c>, if the subsystem exists and supports automatic placement.
        /// </summary>
        void SetAutomaticPlacementStateOnSubsystem()
        {
            if (enabled && supportsAutomaticPlacement)
            {
                subsystem.automaticPlacementRequested = m_AutomaticPlacement;
            }
        }

        /// <summary>
        /// Sets the current state of the <see cref="environmentTextureHDR"/> property to the
        /// <c>XREnvironmentProbeSubsystem</c>, if the subsystem exists and supports HDR environment textures.
        /// </summary>
        void SetEnvironmentTextureHDRStateOnSubsystem()
        {
            if (enabled && supportsEnvironmentTextureHDR)
            {
                subsystem.environmentTextureHDRRequested = m_EnvironmentTextureHDR;
            }
        }
    }
}
