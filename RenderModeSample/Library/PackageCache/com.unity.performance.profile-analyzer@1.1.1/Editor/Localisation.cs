using System;
using UnityEngine;

#if UNITY_2019_1_OR_NEWER
#else
namespace UnityEditor.Performance.ProfileAnalyzer
{
    internal class L10n
    {
        internal static string Tr(string s)
        {
            return s;
        }
    }
}
#endif

