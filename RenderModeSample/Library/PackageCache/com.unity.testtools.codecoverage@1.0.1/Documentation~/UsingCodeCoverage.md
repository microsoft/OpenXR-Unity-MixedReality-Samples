# Using Code Coverage

## Using Code Coverage with Burst compiler

If you use the [Burst package](https://docs.unity3d.com/Packages/com.unity.burst@latest) and have jobs compiled with Burst, you will need to disable Burst compilation in order to get full coverage. To disable Burst compilation you can do **one** of the following:

- Uncheck **Enable Compilation** under **Jobs** > **Burst** > **Enable Compilation**.
- Pass `-burst-disable-compilation` to the command line.

## Using Code Coverage with Code Optimization

Code Optimization was introduced in 2020.1. Code Optimization mode defines whether Unity Editor compiles scripts in Debug or Release mode. Debug mode enables C# debugging and it is required in order to obtain accurate code coverage. To ensure Code optimization is set to Debug mode you can do **one** of the following:

- Switch to Debug mode in the Editor (bottom right corner, select the **Bug icon** > **Switch to debug mode**).
- Using the CompilationPipeline api, set `CompilationPipeline.codeOptimization = CodeOptimization.Debug`.
- Pass `-debugCodeOptimization` to the command line.

## Excluding code from Code Coverage

Any code that should not be contributing to the Code Coverage calculation can be excluded by adding the [`ExcludeFromCoverage`](https://docs.unity3d.com/ScriptReference/TestTools.ExcludeFromCoverageAttribute.html) attribute. This attribute can be added to Assemblies, Classes, Constructors, Methods and Structs. Note that you can also use the .NET [`ExcludeFromCodeCoverage`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.excludefromcodecoverageattribute?view=netcore-2.0) attribute.

## Ignoring tests for Code Coverage

To ignore tests when running with Code Coverage, use the [ConditionalIgnore](https://docs.unity3d.com/Packages/com.unity.test-framework@latest/index.html?subfolder=/manual/reference-attribute-conditionalignore.html) attribute, passing the `"IgnoreForCoverage"` ID.

#### Example
```
public class MyTestClass
{
    [Test, ConditionalIgnore("IgnoreForCoverage", "This test is disabled when ran with code coverage")]
    public void TestNeverRunningWithCodeCoverage()
    {
        Assert.Pass();
    }
}
```