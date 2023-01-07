using System;
using UnityEngine.Profiling;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// An `IDisposable` [profiler](https://docs.unity3d.com/ScriptReference/Profiling.Profiler.html)
    /// object that will begin a profiler sample on instantiation and end the same when disposed.
    /// <para>Example:
    /// <code>
    /// using (new ScopedProfiler("MySample"))
    /// {
    ///     CodeToProfile();
    /// }
    /// </code>
    /// </para>
    /// </summary>
    public struct ScopedProfiler : IDisposable
    {
        /// <summary>
        /// Begins a new profiler sample. Same as [Profiler.BeginSample](https://docs.unity3d.com/ScriptReference/Profiling.Profiler.BeginSample.html).
        /// </summary>
        /// <param name="name">A string to identify the sample in the Profiler window.</param>
        public ScopedProfiler(string name) => Profiler.BeginSample(name);

        /// <summary>
        /// Begins a new profiler sample. Same as [Profiler.BeginSample](https://docs.unity3d.com/ScriptReference/Profiling.Profiler.BeginSample.html).
        /// </summary>
        /// <param name="name">A string to identify the sample in the Profiler window.</param>
        /// <param name="targetObject">An object that provides context to the sample.</param>
        public ScopedProfiler(string name, UnityEngine.Object targetObject) => Profiler.BeginSample(name, targetObject);

        /// <summary>
        /// Ends the current profiling sample. Same as [Profiler.EndSample](https://docs.unity3d.com/ScriptReference/Profiling.Profiler.EndSample.html).
        /// </summary>
        public void Dispose() => Profiler.EndSample();
    }
}
