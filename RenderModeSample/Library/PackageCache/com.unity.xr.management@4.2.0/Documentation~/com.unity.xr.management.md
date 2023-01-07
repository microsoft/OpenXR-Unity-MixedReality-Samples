---
uid: xr-plug-in-management-manual
---
# About the XR Plug-in Management package

Use the **XR Plug-in Management** package to help streamline **XR plug-in** lifecycle management and potentially provide users with build time UI through the Unity **Unified Settings** system.

## Installation

To use this package, you need to add a reference to it in your Project's `Packages/manifest.json` file. There are three ways you can reference a specific version of a package, depending on how you use it.

### Using a production version of the package

For a released version of the package in production, referencing the package is no different than any other released package. If you can see the package in the **Package Manager** window, you can select and install it from there. Alternatively, you can edit add it manually to `manifest.json` like this:

```json
    "dependencies": {
        //...
        "com.unity.xr.management":"<full version number>"
    }
```

### Using a local clone of the package

If you want to use a cloned version of the package directly, you can point the Package Manager at a local folder as the location from which to get the package from.

```json
    "dependencies": {
        //...
        "com.unity.xr.management":"file:path/to/package/root"
    }
```

**Note:** The root of the package folder isn't necessarily the root of the cloned repository. The root of the package folder is the folder where the `package.json` file is located.

### Using the XR Plug-in Management package

There are two target audiences for XR Plug-in Management: the end user and the provider. You can find documentation for both audiences here:

* [End-user documentation](./EndUser.md)
* [Provider documentation](./Provider.md)

## Technical details

### Requirements

This version of XR Plug-in Management is compatible with the following versions of the Unity Editor:

* 2019.4.15f1 and later

### Known limitations

Attempting to manually initialize XR using [XRManagerSettings.InitializeLoader](https://docs.unity3d.com/Packages/com.unity.xr.management@4.0/api/UnityEngine.XR.Management.XRManagerSettings.html#UnityEngine_XR_Management_XRManagerSettings_InitializeLoader) from [Awake](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html) could potentially interfere with graphics initialization. If you wish to manually initialize XR then call `InitializeLoader` from [Start](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html) to ensure the graphics initialization has completed.

### Package contents

This version of XR Plug-in Management includes:

* **XRManagerSettings** - This is a **ScriptableObject** that you can use to manage **XRLoader** instances and their lifecycle.
* **XRLoader** - This is the base class all Loaders should derive from. It provides a basic API that the **XRManagerSettings** can use to manage lifecycle, and a simple API you can use to request specific subsystems from the Loader.
* **XRConfigurationData** - This is an attribute that allows for build and runtime settings to be hosted within the **Unified Settings** window. All instances display under the top-level **XR** entry within the **Unified Settings** window, using the name supplied in the script as part of the attribute. The management package uses the **EditorBuildSettings** config object API, stored with the key provided in the attribute, to maintain and manage the lifecycle for one instance of the build settings. You can access the configuration settings instance by retrieving the instance associated with the chosen key (as set in the attribute) from **EditorBuildSettings**.
* **XRPackageInitializationBase** - Helper class to derive from that simplifies package initialization. Helps to create default instances of the package's `XRLoader` and default settings when you install the package. Initialization only runs once, and you shouldn't depend on the user to create the specified instances on their own.
* **XRBuildHelper** - Abstract class useful for handling some of the boilerplate around moving settings from the Editor to the runtime. If you derive from this class and specify the appropriate settings type, the system moves settings of that type from `EditorUserBuildSettings` to `PlayerSettings` so that the system can use them at runtime.
* **XRGeneralSettings** - Contains settings that apply to all XR Plug-ins, rather than any single provider.
* **Samples folder** - Contains an implementation of all parts of XR Plug-in Management. You can copy this folder to your Project or package to start implementing XR Plug-in Management for your needs.
