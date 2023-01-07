using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Core.Editor
{
    static class JsonHelper
    {
        internal static bool TryJsonDeserialize<T>(string json, ref T dest)
        {
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    dest = JsonConvert.DeserializeObject<T>(json);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            return false;
        }
    }
}
