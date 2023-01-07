using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Performance.ProfileAnalyzer
{
    // Using Profiler.Begin/End calls is tricky, because any scope that could throw ExitGUI,
    // which is likely most parts of this code base, would cause Begin-End-Mismatches if such an exception is thrown in between of them.
    // On versions from 2018.3 and up this issue can be avoided by via using(ProfilerMarker.Auto(){}.
    // Faking this behavior for pre 2018.3 versions, so that construction calls Profiler.Begin and Disposal calls Profiler.End
    // won't work either and still cause Begin-End-Mismatches, especially in Deep Profiling, because the begin and end calls would be
    // outside of the scope they would be expected to occure in
    // So any version without ProfilerMarker API can use this stand in instead and just not add instrumentation.

    // Paste this into any files that need these markers and then use ProfilerMarkerAbstracted:

    // #if UNITY_2018_3_OR_NEWER
    // using ProfilerMarkerAbstracted = Unity.Profiling.ProfilerMarker;
    // #else
    // using ProfilerMarkerAbstracted = UnityEditor.Performance.ProfileAnalyzer.FakeScopedMarker;
    // #endif

#if !UNITY_2018_3_OR_NEWER
    internal struct FakeScopedMarker
    {
        public FakeScopedMarker(string markerName)
        {
        }
        public IDisposable Auto() { return null; }
    }
#endif
}
