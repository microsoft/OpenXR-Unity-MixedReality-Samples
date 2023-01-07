# Oculus Quest Support

In order to deploy to Oculus Quest, enable the Oculus Quest Support feature on the Android build target:

1. Open the Project Settings window (menu: **Edit > Project Settings**).
2. Select the **XR Plug-in Management** from the list of settings on the left.
3. If necessary, enable **OpenXR** in the list of **Plug-in Providers**. Unity installs the OpenXR plug-in, if not already installed.
4. Select the OpenXR settings page under XR Plug-in Management.
5. Add the **Oculus Touch Controller Profile** to the **Interaction Profiles** list. (You can have multiple profiles enabled in the list, the OpenXR chooses the one to use based on the current device at runtime.)
6. Enable **Oculus Quest Support** under **OpenXR Feature Groups**.

The Android apks that are produced with Quest support enabled can be run on the Quest family of devices.
See the [XR](xref:XR) section of the Unity Manual for more information about developing VR games and applications.

For information about Meta company's Oculus support development roadmap, see [Oculus All In on OpenXR: Deprecates Proprietary APIs](https://developer.oculus.com/blog/oculus-all-in-on-openxr-deprecates-proprietary-apis/).
