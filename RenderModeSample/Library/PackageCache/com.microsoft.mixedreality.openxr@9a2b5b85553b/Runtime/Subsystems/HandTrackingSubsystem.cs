// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR
{
    internal class HandTrackingSubsystemController : SubsystemController
    {
        // Must be the same as inputs.id in UnitySubsystemsManifest.json
        // and the same for RegisterLifecycleProvider in InputProvider.cpp
        public const string Id = "OpenXR Input Extension";

        private XRInputSubsystem m_inputExtensionSubsystem = null;

        public HandTrackingSubsystemController(IOpenXRContext context) : base(context) { }

        public override void OnSubsystemCreate(ISubsystemPlugin plugin)
        {
            var descriptors = new List<XRInputSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            foreach (var descriptor in descriptors)
            {
                if (string.Compare(descriptor.id, Id, true) == 0)
                {
                    m_inputExtensionSubsystem = descriptor.Create();
                    if (m_inputExtensionSubsystem != null)
                    {
                        break;
                    }
                }
            }
        }

        public override void OnSubsystemStart(ISubsystemPlugin plugin)
        {
            m_inputExtensionSubsystem?.Start();
        }

        public override void OnSubsystemStop(ISubsystemPlugin plugin)
        {
            m_inputExtensionSubsystem?.Stop();
        }

        public override void OnSubsystemDestroy(ISubsystemPlugin plugin)
        {
            m_inputExtensionSubsystem?.Destroy();
            m_inputExtensionSubsystem = null;
        }
    }
}
