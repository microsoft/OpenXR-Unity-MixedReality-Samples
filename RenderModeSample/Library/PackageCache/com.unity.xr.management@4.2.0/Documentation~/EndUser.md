---
uid: xr-plug-in-management-end-user
---
# End-user documentation

## Installing and using XR Plug-in Management

For instructions on how to install the XR Plug-in Manager, see the [XR Plug-in Framework](https://docs.unity3d.com/2020.1/Documentation/Manual/XRPluginArchitecture.html) page in the Unity Manual.

## Automatic XR loading

By default, XR Plug-in Management initializes automatically and starts your XR environment when the application loads. At runtime, this happens immediately before the first Scene loads. In Play mode, this happens immediately after the first Scene loads, but before `Start` is called on your GameObjects. In both scenarios, XR should be set up before calling the MonoBehaviour [Start](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html) method, so you should be able to query the state of XR in the `Start` method of your GameObjects.

If you want to start XR on a per-Scene basis (for example, to start in 2D and transition into VR), follow these steps:

1. Access the **Project Settings** window (menu: **Edit** &gt; **Project Settings**).
2. Select the **XR Plug-in Management** tab on the left.
3. Disable the **Initialize on start** option for each platform you support.
4. At runtime, call the following methods on `XRGeneralSettings.Instance.Manager` to add/create, remove, and reorder the Loaders from your scripts:

<b>Manual initialization can not be done before Start completes as it depends on graphics initialization within Unity completing.</b>

Initialization of XR must be complete either before the Unity graphics system is setup and initialized (as in Automatic life cycle management) or must be put off till after graphics is completely initialized. The easiest way to check this is to just make sure you do not try to start XR until Start is called on your MonoBehaviour instance.


|Method|Description|
|---|---|
|`InitializeLoader(Async)`|Sets up the XR environment to run manually. Should be called on or after [Start](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html) has finished to avoid conflicts with graphics initialization sequence.|
|`StartSubsystems`|Starts XR and puts your application into XR mode.|
|`StopSubsystems`|Stops XR and takes your application out of XR mode. You can call `StartSubsystems` again to go back into XR mode.|
|`DeinitializeLoader`|Shuts down XR and removes it entirely. You must call `InitializeLoader(Async)` before you can run XR again.|

To handle pause state changes in the Editor, subscribe to the [`EditorApplication.pauseStateChanged`](https://docs.unity3d.com/ScriptReference/EditorApplication-pauseStateChanged.html) API, then stop and start the subsystems according to the new pause state that the `pauseStateChange` delegate method returns.

The following code shows an example of how to manually control XR using XR Plug-in Management:

```csharp
using System.Collections;
using UnityEngine;

using UnityEngine.XR.Management;

public class ManualXRControl
{
    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }

    void StopXR()
    {
        Debug.Log("Stopping XR...");

        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }
}
```

## Automatic XR loading for specific XR loaders

The previous section showed how to manage the entire XR system lifecycle. If you require more granular control, you can manage an individual loader's lifecycle instead.

Use the script above, with some minor tweaks, to manage specific loaders at runtime, as shown in the code example below. You can use the following methods in your script:

|**Method**|**Description**|
|---|---|
|`XRLoader.Initialize`|Sets up the XR environment to run manually and initializes all subsystems for the XR loader.|
|`XRLoader.Start`|Starts XR and requests the XR loader to start all initialized subsystems.|
|`XRLoader.Stop`|Stops XR and requests the XR loader to stop all initialized subsystems. You can call `StartSubsystems` again to go back into XR mode.|
|`XRLoader.Deinitialize`|Shuts down the XR loader and de-initializes all initialized subsystems. You must call `XRLoader.Initialize` before you can use the loader again.|

The following code example demonstrates how to manage individual loaders at runtime.

```csharp
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class RuntimeXRLoaderManager : MonoBehaviour
{
    XRLoader m_SelectedXRLoader;

    void StartXR(int loaderIndex)
    {
        // Once a loader has been selected, prevent the RuntimeXRLoaderManager from
        // losing access to the selected loader
        if (m_SelectedXRLoader == null)
        {
            m_SelectedXRLoader = XRGeneralSettings.Instance.Manager.activeLoaders[loaderIndex];
        }
        StartCoroutine(StartXRCoroutine());
    }

    IEnumerator StartXRCoroutine()
    {
        Debug.Log("Init XR loader");

        var initSuccess = m_SelectedXRLoader.Initialize();
        if (!initSuccess)
        {
            Debug.LogError("Error initializing selected loader.");
        }
        else
        {
            yield return null;
            Debug.Log("Start XR loader");
            var startSuccess = m_SelectedXRLoader.Start();
            if (!startSuccess)
            {
                yield return null;
                Debug.LogError("Error starting selected loader.");
                m_SelectedXRLoader.Deinitialize();
            }
        }
    }

    void StopXR()
    {
        Debug.Log("Stopping XR Loader...");
        m_SelectedXRLoader.Stop();
        m_SelectedXRLoader.Deinitialize();
        m_SelectedXRLoader = null;
        Debug.Log("XR Loader stopped completely.");
    }
}
```

## Customizing build and runtime settings

Any package that needs build or runtime settings should provide a settings data type for use. This data type appears in the **Project Settings** window, underneath a top level **XR** node.

You can use scripts to configure the settings for a specific plug-in, or change the active and inactive plug-ins per build target.

### Example: Accessing custom settings

**Note**: This doesn't install any plug-ins for you. Make sure your plug-ins are installed and available before you try this script.

```csharp
    var metadata = XRPackageMetadataStore.GetMetadataForPackage(my_pacakge_id);
    assets = AssetDatabase.FindAssets($"t:{metadata.settingsType}");
    var assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);

    // Settings access is type specific. You will need information from your plug-in documentation
    // to know how to get at specific instances and properties.

    // You must know the type of the settings you are accessing.
    var directInstance  = AssetDatabase.LoadAssetAtPath(assetPath, typeof(full.typename.for.pluginsettings));

    // You must know the access method for getting build target specific settings data.
    var buildTargetSettings = directInstance.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);

    // Do something with settings...

    // Mark instance dirty and save any changes.
    EditorUtility.SetDirty(directInstance);
    AssetDatabase.SaveAssets();
```

### Example: Configuring plug-ins per build target

**Note**: This doesn't install any plug-ins for you. Make sure your plug-ins are installed and available before you try this script.

Adding a plug-in to the set of assigned plug-ins for a build target:

```csharp
    var buildTargetSettings = XRGeneralSettingsPerBuildTarget.SettingsForBuildTarget(BuildTarget.Standalone);
    var pluginsSettings = buildTargetSettings.AssignedSettings;
    var didAssign = XRPackageMetadataStore.AssignLoader(pluginsSettings, "full.typename.for.pluginloader", BuildTargetGroup.Standalone);

    if (!didAssign)
    {
        // Report error or do something here.
        ...
    }
```

Removing a plug-in from the set of assigned plug-ins for a build target:

```csharp
    var buildTargetSettings = XRGeneralSettingsPerBuildTarget.SettingsForBuildTarget(BuildTarget.Standalone);
    var pluginsSettings = buildTargetSettings.AssignedSettings;
    var didRemove = XRPackageMetadataStore.RemoveLoader(pluginsSettings, "full.typename.for.pluginloader", BuildTargetGroup.Standalone);

    if (!didRemove)
    {
        // Report error or do something here.
        ...
    }
```

### Example: Modifying the loader list

By default, the XR Plug-in Management UI displays loaders in strict alphabetical order, based on their names. In most scenarios, you don't need to change this order. However, if you need the loaders in a different order, you can modify the loaders list from script like so:

```csharp
    var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTarget.Standalone);
    var settingsManager = generalSettings.Manager;

    // Get example loaders as XRLoaders
    var fooLoader = new FooLoader() as XRLoader;
    var barLoader = new BarLoader() as XRLoader;

    // Adding new loaders
    // Append the new FooLoader
    if (!settingsManager.TryAddLoader(fooLoader))
        Debug.Log("Adding new Foo Loader failed! Refer to the documentation for additional information!");

    // Insert the new BarLoader at the start of the list
    if (!settingsManager.TryAddLoader(barLoader, 0))
        Debug.Log("Adding new Bar Loader failed! Refer to the documentation for additional information!");

    // Removing loaders
    if (!settingsManager.TryRemoveLoader(fooLoader))
        Debug.Log("Failed to remove the fooLoader! Refer to the documentation for additional information!");

    // Modifying the loader list order
    var readonlyCurrentLoaders = settingsManager.activeLoaders;

    // Copy the returned read only list
    var currentLoaders = new List<XRLoader>(readonlyCurrentLoaders);

    // Reverse the list
    currentLoaders.Reverse();

    if (!settingsManager.TrySetLoaders(currentLoaders))
        Debug.Log("Failed to set the reordered loader list! Refer to the documentation for additional information!");
```

You would most likely place this script in a custom build script, but that isn't required. Regardless of the script's location location, you should do this as a setup step before you start a build. This is because the first thing that XR Plug-in Manager does at build time is to serialize the loader list to the build target.

**Note:** A new loader that wasn't known at startup can't be added to the loader list at runtime, which causes the modification operation to fail. You can still modify the list during runtime, whether the app runs in Play mode or as a standalone build.

This means that you are able to do the following during runtime:

- Remove loaders from the list of loaders.
- Re-add loaders that were previously removed.
- Reorder the list of loaders.

**Note**: Any operation on the XR Plug-in Manager UI will reset the ordering to the original alphabetical ordering.

## Installing the XR Plug-in Management package

Please see related Unity documentation for [Configuring XR](https://docs.unity3d.com/Manual/configuring-project-for-xr.html ).
