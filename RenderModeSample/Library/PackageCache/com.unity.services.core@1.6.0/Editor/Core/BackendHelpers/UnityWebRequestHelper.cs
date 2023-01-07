using Unity.Services.Core.Internal;
using UnityEngine.Networking;

namespace Unity.Services.Core.Editor
{
    static class UnityWebRequestHelper
    {
        internal static bool IsUnityWebRequestReadyForTextExtract(UnityWebRequest unityWebRequest, out string downloadHandlerText)
        {
            if (unityWebRequest?.HasSucceeded() ?? false)
            {
                downloadHandlerText = unityWebRequest.downloadHandler?.text;
                return !string.IsNullOrEmpty(downloadHandlerText);
            }

            downloadHandlerText = null;
            return false;
        }
    }
}
