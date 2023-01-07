using System;
using System.Threading;
using UnityEngine;

namespace Unity.VisualScripting
{
    public static class UnityThread
    {
        public static Thread thread = Thread.CurrentThread;

        public static Action<Action> editorAsync;

        public static bool allowsAPI => !Serialization.isUnitySerializing && Thread.CurrentThread == thread;

        internal static void RuntimeInitialize()
        {
            thread = Thread.CurrentThread;
        }

        public static void EditorAsync(Action action)
        {
            editorAsync?.Invoke(action);
        }
    }
}
