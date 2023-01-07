using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Services.Core.Threading.Internal
{
    class UnityThreadUtilsInternal : IUnityThreadUtils
    {
        public static Task PostAsync(Action action)
        {
            return Task.Factory.StartNew(
                action, CancellationToken.None, TaskCreationOptions.None, UnityThreadUtils.UnityThreadScheduler);
        }

        public static Task PostAsync(Action<object> action, object state)
        {
            return Task.Factory.StartNew(
                action, state, CancellationToken.None, TaskCreationOptions.None,
                UnityThreadUtils.UnityThreadScheduler);
        }

        public static Task<T> PostAsync<T>(Func<T> action)
        {
            return Task<T>.Factory.StartNew(
                action, CancellationToken.None, TaskCreationOptions.None, UnityThreadUtils.UnityThreadScheduler);
        }

        public static Task<T> PostAsync<T>(Func<object, T> action, object state)
        {
            return Task<T>.Factory.StartNew(
                action, state, CancellationToken.None, TaskCreationOptions.None,
                UnityThreadUtils.UnityThreadScheduler);
        }

        public static void Send(Action action)
        {
            if (UnityThreadUtils.IsRunningOnUnityThread)
            {
                action();
                return;
            }

            PostAsync(action).Wait();
        }

        public static void Send(Action<object> action, object state)
        {
            if (UnityThreadUtils.IsRunningOnUnityThread)
            {
                action(state);
                return;
            }

            PostAsync(action, state).Wait();
        }

        public static T Send<T>(Func<T> action)
        {
            if (UnityThreadUtils.IsRunningOnUnityThread)
            {
                return action();
            }

            var task = PostAsync(action);
            task.Wait();
            return task.Result;
        }

        public static T Send<T>(Func<object, T> action, object state)
        {
            if (UnityThreadUtils.IsRunningOnUnityThread)
            {
                return action(state);
            }

            var task = PostAsync(action, state);
            task.Wait();
            return task.Result;
        }

        bool IUnityThreadUtils.IsRunningOnUnityThread => UnityThreadUtils.IsRunningOnUnityThread;
        Task IUnityThreadUtils.PostAsync(Action action) => PostAsync(action);
        Task IUnityThreadUtils.PostAsync(Action<object> action, object state) => PostAsync(action, state);
        Task<T> IUnityThreadUtils.PostAsync<T>(Func<T> action) => PostAsync(action);
        Task<T> IUnityThreadUtils.PostAsync<T>(Func<object, T> action, object state) => PostAsync(action, state);
        void IUnityThreadUtils.Send(Action action) => Send(action);
        void IUnityThreadUtils.Send(Action<object> action, object state) => Send(action, state);
        T IUnityThreadUtils.Send<T>(Func<T> action) => Send(action);
        T IUnityThreadUtils.Send<T>(Func<object, T> action, object state) => Send(action, state);
    }
}
