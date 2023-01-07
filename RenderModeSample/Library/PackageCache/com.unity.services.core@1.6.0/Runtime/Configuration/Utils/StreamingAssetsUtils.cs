#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_WEBGL)
#define READ_STREMAING_ASSETS_WITH_WEB_REQUEST
#endif

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
#if READ_STREMAING_ASSETS_WITH_WEB_REQUEST
using Unity.Services.Core.Internal;
#endif

namespace Unity.Services.Core.Configuration
{
    static class StreamingAssetsUtils
    {
        public static Task<string> GetFileTextFromStreamingAssetsAsync(string path)
        {
            var fullPath = Path.Combine(Application.streamingAssetsPath, path);
#if READ_STREMAING_ASSETS_WITH_WEB_REQUEST
            return UnityWebRequestUtils.GetTextAsync(fullPath);
#else
            var completionSource = new TaskCompletionSource<string>();
            try
            {
                var fileText = File.ReadAllText(fullPath);
                completionSource.SetResult(fileText);
            }
            catch (Exception e)
            {
                completionSource.SetException(e);
            }

            return completionSource.Task;
#endif
        }
    }
}
