# About Editor Coroutines

The Editor Coroutines package allows the user to start the execution of [iterator methods](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/yield) within the Editor similar to how we handle [Coroutines](https://docs.unity3d.com/Manual/Coroutines.html) inside [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) scripts during runtime. 

# Installing Editor Coroutines

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html). 

> **Note**: While this package is in preview, the Package Manager needs to be configured to show __Preview Packages__. (Under the __Advanced__ drop-down menu, enable __Show preview packages__.) Then search for the Editor Coroutines package.

<a name="UsingPackageName"></a>

# Using Editor Coroutines

To learn how to use the Editor Coroutines package in your project, please refer to the Scripting API section of the documentation.

# Technical details
## Requirements

This version of Editor Coroutines is compatible with the following versions of the Unity Editor:

* 2018.1 and later (recommended)

> **Note**:  If you install the Memory Profiler package it will automatically install the Editor Coroutines package as a dependency.

## Known limitations

Editor Coroutines version 0.0.1-preview.2 includes the following known limitation(s):

The iterator functions passed to Editor Coroutines do not support yielding any of the instruction classes present inside the Unity Scripting API (e.g., [WaitForSeconds](https://docs.unity3d.com/ScriptReference/WaitForSeconds.html), [WaitForEndOfFrame](https://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html)), except for the [CustomYieldInstruction](https://docs.unity3d.com/ScriptReference/CustomYieldInstruction.html) derived classes with the `MoveNext` method implemented.

> **Tip**: `yield return null` is a way to skip a frame within the Editor.

## Package contents

The following table indicates the root folders in the package where you can find useful resources:

| Location         | Description                                 |
| ---------------- | ------------------------------------------- |
| `Documentation~` | Contains the documentation for the package. |
| `Tests`          | Contains the unit tests for the package.    |

## Document revision history

|Date|Reason|
|---|---|
|June 20, 2019|Removed deprecated manual link.|
|Dec 7, 2018|Api documentation added. Matches package version 0.0.1-preview.4.|
|Oct 11, 2018|Document created. Matches package version 0.0.1-preview.2.|