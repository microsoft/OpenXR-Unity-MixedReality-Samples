using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// Base class for asynchronous operations.
    ///
    /// Implemented by: TaskAsyncOperation
    /// </summary>
    abstract class AsyncOperationBase : CustomYieldInstruction, IAsyncOperation, INotifyCompletion
    {
        /// <summary>
        /// Indicates if coroutine should be kept suspended.
        ///
        /// From CustomYieldInstruction.
        /// </summary>
        public override bool keepWaiting => !IsCompleted;

        /// <summary>
        /// Whether this operation is completed.
        ///
        /// Required to make the operation awaitable
        /// </summary>
        public abstract bool IsCompleted { get; }

        /// <summary>
        /// If true, this operation either succeeded, failed, or has been canceled.
        ///
        /// From IAsyncOperation
        /// </summary>
        public bool IsDone => IsCompleted;

        /// <summary>
        /// The current status of this operation.
        ///
        /// From IAsyncOperation
        /// </summary>
        public abstract AsyncOperationStatus Status { get; }

        /// <summary>
        /// The exception that occured during the operation if it failed.
        ///
        /// From IAsyncOperation
        /// </summary>
        public abstract Exception Exception { get; }

        /// <summary>
        /// Result of the operation.
        ///
        /// Required to make the operation awaitable
        /// </summary>
        public abstract void GetResult();

        /// <summary>
        /// Awaiter on the operation.
        ///
        /// Required to make the operation awaitable
        /// </summary>
        public abstract AsyncOperationBase GetAwaiter();

        Action<IAsyncOperation> m_CompletedCallback;

        /// <summary>
        /// Event raised when the operation succeeded or failed.
        /// The argument is the operation that raised the event.
        ///
        /// From IAsyncOperation
        /// </summary>
        public event Action<IAsyncOperation> Completed
        {
            add
            {
                if (IsDone)
                {
                    value(this);
                }
                else
                {
                    m_CompletedCallback += value;
                }
            }
            remove => m_CompletedCallback -= value;
        }

        protected void DidComplete()
        {
            m_CompletedCallback?.Invoke(this);
        }

        /// <summary>Schedules the continuation action that's invoked when the instance completes.</summary>
        /// <param name="continuation">The action to invoke when the operation completes.</param>
        ///
        /// From INotifyCompletion
        public virtual void OnCompleted(Action continuation)
        {
            Completed += op => continuation?.Invoke();
        }
    }

    /// <summary>
    /// Base class for asynchronous operations.
    ///
    /// Implemented by: TaskAsyncOperation<T>
    /// </summary>
    /// <typeparam name="T">
    /// The type of this operation's result
    /// </typeparam>
    abstract class AsyncOperationBase<T> : CustomYieldInstruction, IAsyncOperation<T>, INotifyCompletion
    {
        /// <summary>
        /// Indicates if coroutine should be kept suspended.
        ///
        /// From CustomYieldInstruction.
        /// </summary>
        public override bool keepWaiting => !IsCompleted;

        /// <summary>
        /// Whether this operation is completed.
        ///
        /// Required to make the operation awaitable
        /// </summary>
        public abstract bool IsCompleted { get; }

        /// <summary>
        /// If true, this operation either succeeded, failed, or has been canceled.
        ///
        /// From IAsyncOperation
        /// </summary>
        public bool IsDone => IsCompleted;

        /// <summary>
        /// The current status of this operation.
        ///
        /// From IAsyncOperation
        /// </summary>
        public abstract AsyncOperationStatus Status { get; }

        /// <summary>
        /// The exception that occured during the operation if it failed.
        ///
        /// From IAsyncOperation
        /// </summary>
        public abstract Exception Exception { get; }

        /// <summary>
        /// Result of the operation.
        ///
        /// Required to make the operation awaitable
        /// </summary>
        public abstract T Result { get; }

        /// <summary>
        /// Awaiter on the operation.
        ///
        /// Required to make the operation awaitable
        /// </summary>
        public abstract T GetResult();

        /// <summary>
        /// Awaiter on the operation.
        ///
        /// Required to make the operation awaitable
        /// </summary>
        public abstract AsyncOperationBase<T> GetAwaiter();

        Action<IAsyncOperation<T>> m_CompletedCallback;

        /// <summary>
        /// Event raised when the operation succeeded or failed.
        /// The argument is the operation that raised the event.
        ///
        /// From IAsyncOperation
        /// </summary>
        public event Action<IAsyncOperation<T>> Completed
        {
            add
            {
                if (IsDone)
                {
                    value(this);
                }
                else
                {
                    m_CompletedCallback += value;
                }
            }
            remove => m_CompletedCallback -= value;
        }

        protected void DidComplete()
        {
            m_CompletedCallback?.Invoke(this);
        }

        /// <summary>Schedules the continuation action that's invoked when the instance completes.</summary>
        /// <param name="continuation">The action to invoke when the operation completes.</param>
        ///
        /// From INotifyCompletion
        public virtual void OnCompleted(Action continuation)
        {
            Completed += op => continuation?.Invoke();
        }
    }
}
