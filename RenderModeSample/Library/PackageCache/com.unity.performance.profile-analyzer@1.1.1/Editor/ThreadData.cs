using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEditor.Performance.ProfileAnalyzer
{
    [Serializable]
    internal class ThreadData
    {
        public string threadNameWithIndex;
        public int threadGroupIndex;
        public string threadGroupName;
        public int threadsInGroup;
        public List<ThreadFrameTime> frames = new List<ThreadFrameTime>();

        public float msMedian;
        public float msLowerQuartile;
        public float msUpperQuartile;
        public float msMin;
        public float msMax;

        public int medianFrameIndex;
        public int minFrameIndex;
        public int maxFrameIndex;

        public ThreadData(string _threadName)
        {
            threadNameWithIndex = _threadName;

            var info = threadNameWithIndex.Split(':');
            threadGroupIndex = int.Parse(info[0]);
            threadGroupName = info[1];
            threadsInGroup = 1;

            msMedian = 0.0f;
            msLowerQuartile = 0.0f;
            msUpperQuartile = 0.0f;
            msMin = 0.0f;
            msMax = 0.0f;

            medianFrameIndex = -1;
            minFrameIndex = -1;
            maxFrameIndex = -1;
        }

        struct ThreadFrameTimeFrameComparer : IComparer<ThreadFrameTime>
        {
#if UNITY_2017_OR_NEWER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
            [MethodImpl(256)]
#endif
            public int Compare(ThreadFrameTime x, ThreadFrameTime y)
            {
                if (x.frameIndex == y.frameIndex)
                    return 0;
                return x.frameIndex > y.frameIndex ? 1 : -1;
            }
        }

#if UNITY_2017_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [MethodImpl(256)]
#endif
        public ThreadFrameTime? GetFrame(int frameIndex)
        {
            var index = frames.BinarySearch(new ThreadFrameTime(frameIndex, 0, 0), new ThreadFrameTimeFrameComparer());
            return index >= 0 ? (ThreadFrameTime?)frames[index] : null;
        }
    }
}
