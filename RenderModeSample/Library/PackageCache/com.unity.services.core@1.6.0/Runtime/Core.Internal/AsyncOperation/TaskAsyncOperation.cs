using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// Task-based implementation of IAsyncOperation.
    ///
    /// awaitable in tasks
    /// yieldable in coroutines
    /// </summary>
    class TaskAsyncOperation : AsyncOperationBase, INotifyCompletion
    {
        /// <remarks>
        /// The scheduler is also used by the <see cref="TaskAsyncOperation{T}"/> because
        /// <see cref="RuntimeInitializeOnLoadMethodAttribute"/> can't be used in generic objects.
        /// </remarks>
        internal static TaskScheduler Scheduler;

        Task m_Task;

        /// <inheritdoc />
        public override bool IsCompleted => m_Task.IsCompleted;

        /// <inheritdoc />
        public override AsyncOperationStatus Status
        {
            get
            {
                if (m_Task == null)
                {
                    return AsyncOperationStatus.None;
                }

                if (!m_Task.IsCompleted)
                {
                    return AsyncOperationStatus.InProgress;
                }

                if (m_Task.IsCanceled)
                {
                    return AsyncOperationStatus.Cancelled;
                }

                if (m_Task.IsFaulted)
                {
                    return AsyncOperationStatus.Failed;
                }

                return AsyncOperationStatus.Succeeded;
            }
        }

        /// <inheritdoc />
        public override Exception Exception => m_Task?.Exception;

        /// <inheritdoc />
        public override void GetResult() {}

        /// <inheritdoc />
        public override AsyncOperationBase GetAwaiter()
        {
            return this;
        }

        /// <summary>
        /// Creates a new TaskAsyncOperation from a provided Task.
        /// Returns on Unity's main thread context.
        /// </summary>
        /// <param name="task">
        /// The task tracked by this TaskAsyncOperation.
        /// </param>
        public TaskAsyncOperation(Task task)
        {
            // Tests don't run `RuntimeInitializeOnLoadMethod`s?
            if (Scheduler == null)
            {
                SetScheduler();
            }

            m_Task = task;

            task.ContinueWith((t, state) =>
            {
                var self = (TaskAsyncOperation)state;
                self.DidComplete();
            }, this, CancellationToken.None, TaskContinuationOptions.None, Scheduler);
        }

        /// <summary>
        /// Creates and starts a task from the provided Action executing on the C# thread pool.
        /// Returns on Unity's main thread context.
        /// </summary>
        /// <param name="action">
        /// The Action to execute asynchronously.
        /// </param>
        /// <returns>
        /// A TaskAsyncOperation tracking the execution of the provided Action.
        /// </returns>
        public static TaskAsyncOperation Run(Action action)
        {
            var task = new Task(action);
            var ret = new TaskAsyncOperation(task);
            task.Start();
            return ret;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void SetScheduler()
        {
            Scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }
    }

    /// <summary>
    /// Task-based implementation of IAsyncOperation<T>.
    ///
    /// awaitable in tasks
    /// yieldable in coroutines
    /// </summary>
    /// <typeparam name="T">
    /// The return type of the operation's result.
    /// </typeparam>
    class TaskAsyncOperation<T> : AsyncOperationBase<T>
    {
        Task<T> m_Task;

        /// <inheritdoc />
        public override bool IsCompleted => m_Task.IsCompleted;

        /// <inheritdoc />
        public override T Result => m_Task.Result;

        /// <inheritdoc />
        public override T GetResult()
        {
            return m_Task.GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public override AsyncOperationBase<T> GetAwaiter()
        {
            return this;
        }

        /// <inheritdoc />
        public override AsyncOperationStatus Status
        {
            get
            {
                if (m_Task == null)
                {
                    return AsyncOperationStatus.None;
                }

                if (!m_Task.IsCompleted)
                {
                    return AsyncOperationStatus.InProgress;
                }

                if (m_Task.IsCanceled)
                {
                    return AsyncOperationStatus.Cancelled;
                }

                if (m_Task.IsFaulted)
                {
                    return AsyncOperationStatus.Failed;
                }

                return AsyncOperationStatus.Succeeded;
            }
        }

        /// <inheritdoc />
        public override Exception Exception => m_Task?.Exception;

        /// <summary>
        /// Creates a new TaskAsyncOperation from a provided Task.
        /// Returns on Unity's main thread context.
        /// </summary>
        /// <param name="task">
        /// The task tracked by this TaskAsyncOperation.
        /// </param>
        public TaskAsyncOperation(Task<T> task)
        {
            // Tests don't run `RuntimeInitializeOnLoadMethod`s?
            if (TaskAsyncOperation.Scheduler == null)
            {
                TaskAsyncOperation.SetScheduler();
            }

            m_Task = task;

            task.ContinueWith((t, state) =>
            {
                var self = (TaskAsyncOperation<T>)state;
                self.DidComplete();
            }, this, CancellationToken.None, TaskContinuationOptions.None, TaskAsyncOperation.Scheduler);
        }

        /// <summary>
        /// Creates and starts a task from the provided Action executing on the C# thread pool.
        /// Returns on Unity's main thread context.
        /// </summary>
        /// <param name="func">
        /// The Action to execute asynchronously.
        /// </param>
        /// <returns>
        /// A TaskAsyncOperation tracking the execution of the provided Action.
        /// </returns>
        public static TaskAsyncOperation<T> Run(Func<T> func)
        {
            var task = new Task<T>(func);
            var ret = new TaskAsyncOperation<T>(task);
            task.Start();
            return ret;
        }
    }
}
