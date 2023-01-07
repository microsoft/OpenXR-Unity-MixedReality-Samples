# Technical details

## How it works

The package is a client of the coverage API that was added in Unity 2019.2. For more information, see the [coverage API's documentation](https://docs.unity3d.com/ScriptReference/TestTools.Coverage.html). The package uses a combination of this API and C# reflection to output the test coverage data in the *OpenCover* format. Optionally, a third party report generator will then parse the OpenCover data and present the coverage data as a series of html documents.

## Requirements

This version of the Code Coverage package is compatible with the following versions of the Unity Editor:

* 2019.3 and later

## 3rd party libraries used

* [ReportGenerator](https://github.com/danielpalme/ReportGenerator) - v4.8.9

## Known limitations

Code Coverage includes the following known limitations:

* Code Coverage currently only supports the [OpenCover](https://github.com/OpenCover/opencover) format.
* Code Coverage currently only supports code ran in the Editor and not in Standalone/Player.
* NPath Complexity calculation and Branch Coverage are not implemented at present so they will always appear as zero in the coverage report.