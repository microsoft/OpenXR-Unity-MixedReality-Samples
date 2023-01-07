// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine.XR;

namespace Microsoft.MixedReality.OpenXR
{
    internal class MeshSubsystemController : SubsystemController
    {
        // Must be the same as meshings.id in UnitySubsystemsManifest.json
        // and the same for RegisterLifecycleProvider in InputProvider.cpp
        public const string Id = "OpenXR Mesh Extension";

        private readonly NativeLibToken nativeLibToken;
        private static List<XRMeshSubsystemDescriptor> s_MeshDescriptors = new List<XRMeshSubsystemDescriptor>();

        public MeshSubsystemController(NativeLibToken token, IOpenXRContext context) : base(context)
        {
            nativeLibToken = token;
        }

        public override void OnSubsystemCreate(ISubsystemPlugin plugin)
        {
            plugin.CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>(s_MeshDescriptors, Id);
        }

        public override void OnSubsystemDestroy(ISubsystemPlugin plugin)
        {
            plugin.DestroySubsystem<XRMeshSubsystem>();
        }
    }
}
