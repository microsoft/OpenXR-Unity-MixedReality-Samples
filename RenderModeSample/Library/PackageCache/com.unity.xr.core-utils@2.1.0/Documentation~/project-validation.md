---
uid: xr-core-utils-project-validation
---
## Project Validation

The Project Validation system helps you avoid common Scene and Project configuration issues for the XR packages you have installed. The XR packages you have installed can include rules for the validation system. The system evaluates these rules when you make a build and whenever you have the Project Validation window open. To open the window, go to your project settings (menu: **Edit &gt; Project Settings**) and then select **Project Validation** in the **XR Plug-in management** section. The rules are checked per platform build target. Some rules may examine the current Scene to verify that the project settings support a feature used in the Scene. 

The image below shows a set of failed validation checks based on the OpenXR, Object Capture, and Google ARCore Plug-in packages.

![Project Validation display](images/ProjectValidation/project-settings-validation.png)<br />*The Project Validation section of the XR Plug-in Management settings*

The Project Validation section contains tabs for each of the platform Build Targets installed in the running instance of the Unity Editor. Select a tab to check the rules for that build target. (You do not need to switch targets in the [Build Settings](xref:BuildSettings) window.) The validation system evaluates the same rules at build time for a given build target.

For each build target, the Project Validation section lists the relevant rules and whether they passed or failed. (The system hides rules that pass validation unless you select the **Show all** option.) If a failed rule has a **Fix** button, you can click it to change the setting that caused the failure. In some cases, a single-click fix is not available. Instead the rule provides an **Edit** button. Click **Edit** to navigate to the location in Unity where you can edit the relevant settings. You can click **Fix all** to correct all rules that have a Fix button. 

> [!NOTE]
> * In a few situations, rules must be evaluated even when they do not apply to the selected build target. When this happens, the validation system automatically marks the rules as passing. You only see such rules if you select the **Show all** option.
> * If a required rule causes the build to fail, you can select the **Ignore build errors** option so that you can build the project anyway. The related XR features are unlikely to work correctly in this scenario.

### Check status icons

The status icons to the left of an individual build validation rule provide more information about the status of the rule.

| Status                                                              | info |
|---------------------------------------------------------------------| ----------- |
| ![success](images/ProjectValidation/project-validation-success.png) | Build validation rule passed because the project/scene is either set up correctly, or the rule is not applicable. These rules are hidden in the Project Validation rules list unless you enable the **Show all** option. |
| ![warning](images/ProjectValidation/project-validation-warning.png) | Build validation rule failed, but will not block the building of the project. These rules are safe to ignore if you have set up your project differently than recommended. |
| ![error](images/ProjectValidation/project-validation-error.png)   | Build validation rule failed and will block building of the project. These rules cannot be ignored and you must fix these issues. |

### Fix or Edit button

The **Fix** button automatically fixes the issue in your project or Scene. The **Edit** button takes you to the appropriate place in the Unity Editor where you can correct the issue in your project. Both the **Fix** and **Edit** buttons provide a tooltip explaining the steps to manually correct the issue.
