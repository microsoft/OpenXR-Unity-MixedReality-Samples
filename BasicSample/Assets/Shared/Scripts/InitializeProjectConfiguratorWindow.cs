// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using UnityEditor;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    [InitializeOnLoad]
    public class InitializeProjectConfiguratorWindow
    {
        static InitializeProjectConfiguratorWindow()
        {
            EditorApplication.update += OnInit;
        }

        private static void OnInit()
        {
            // We only want to execute once to initialize, unsubscribe from update event
            EditorApplication.update -= OnInit;

            ProjectConfiguratorWindow.ShowWindow();
        }
    }
}