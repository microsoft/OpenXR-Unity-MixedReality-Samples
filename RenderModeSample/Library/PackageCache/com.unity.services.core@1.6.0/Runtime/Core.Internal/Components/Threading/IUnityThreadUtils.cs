using System;
using System.Threading.Tasks;
using Unity.Services.Core.Internal;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Threading.Internal
{
    /// <summary>
    /// This component is an utility to simplify working with the Unity thread.
    /// </summary>
    public interface IUnityThreadUtils : IServiceComponent
    {
        /// <summary>
        /// Check if the calling thread is the Unity thread.
        /// </summary>
        bool IsRunningOnUnityThread { get; }

        /// <summary>
        /// Create a task out of the given <paramref name="action"/> that will be invoked on the Unity thread.
        /// </summary>
        /// <param name="action">
        /// The action to invoke on the Unity thread.
        /// </param>
        /// <returns>
        /// Return the created task.
        /// </returns>
        Task PostAsync([NotNull] Action action);

        /// <summary>
        /// Create a task out of the given <paramref name="action"/> that will be invoked on the Unity thread.
        /// </summary>
        /// <param name="action">
        /// The action to invoke on the Unity thread.
        /// <paramref name="state"/> will be passed as its argument.
        /// </param>
        /// <param name="state">
        /// The captured state to pass to <paramref name="action"/> when invoking it.
        /// </param>
        /// <returns>
        /// Return the created task.
        /// </returns>
        Task PostAsync([NotNull] Action<object> action, object state);

        /// <summary>
        /// Create a task out of the given <paramref name="action"/> that will be invoked on the Unity thread.
        /// </summary>
        /// <param name="action">
        /// The action to invoke on the Unity thread.
        /// </param>
        /// <typeparam name="T">
        /// The type of the return of the invoked action.
        /// Can be any type.
        /// </typeparam>
        /// <returns>
        /// Return the created task.
        /// </returns>
        Task<T> PostAsync<T>([NotNull] Func<T> action);

        /// <summary>
        /// Create a task out of the given <paramref name="action"/> that will be invoked on the Unity thread.
        /// </summary>
        /// <param name="action">
        /// The action to invoke on the Unity thread.
        /// <paramref name="state"/> will be passed as its argument.
        /// </param>
        /// <param name="state">
        /// The captured state to pass to <paramref name="action"/> when invoking it.
        /// </param>
        /// <typeparam name="T">
        /// The type of the return of the invoked action.
        /// Can be any type.
        /// </typeparam>
        /// <returns>
        /// Return the created task.
        /// </returns>
        Task<T> PostAsync<T>([NotNull] Func<object, T> action, object state);

        /// <summary>
        /// Execute the given <paramref name="action"/> on the Unity thread.
        /// Wait for the execution to finish before resuming this thread.
        /// </summary>
        /// <param name="action">
        /// The action to invoke on the Unity thread.
        /// </param>
        void Send([NotNull] Action action);

        /// <summary>
        /// Execute the given <paramref name="action"/> on the Unity thread.
        /// Wait for the execution to finish before resuming this thread.
        /// </summary>
        /// <param name="action">
        /// The action to invoke on the Unity thread.
        /// <paramref name="state"/> will be passed as its argument.
        /// </param>
        /// <param name="state">
        /// The captured state to pass to <paramref name="action"/> when invoking it.
        /// </param>
        void Send([NotNull] Action<object> action, object state);

        /// <summary>
        /// Execute the given <paramref name="action"/> on the Unity thread.
        /// Wait for the execution to finish before resuming this thread.
        /// </summary>
        /// <param name="action">
        /// The action to invoke on the Unity thread.
        /// </param>
        /// <typeparam name="T">
        /// The type of the return of the invoked action.
        /// Can be any type.
        /// </typeparam>
        /// <returns>
        /// Return what the action returned.
        /// </returns>
        T Send<T>([NotNull] Func<T> action);

        /// <summary>
        /// Execute the given <paramref name="action"/> on the Unity thread.
        /// Wait for the execution to finish before resuming this thread.
        /// </summary>
        /// <param name="action">
        /// The action to invoke on the Unity thread.
        /// <paramref name="state"/> will be passed as its argument.
        /// </param>
        /// <param name="state">
        /// The captured state to pass to <paramref name="action"/> when invoking it.
        /// </param>
        /// <typeparam name="T">
        /// The type of the return of the invoked action.
        /// Can be any type.
        /// </typeparam>
        /// <returns>
        /// Return what the action returned.
        /// </returns>
        T Send<T>([NotNull] Func<object, T> action, object state);
    }
}
