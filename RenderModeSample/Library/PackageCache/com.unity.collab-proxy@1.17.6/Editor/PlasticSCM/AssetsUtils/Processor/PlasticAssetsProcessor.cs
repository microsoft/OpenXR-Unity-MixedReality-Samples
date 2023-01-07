using System;
using System.Linq;

using Codice.LogWrapper;

namespace Unity.PlasticSCM.Editor.AssetUtils.Processor
{
    internal class PlasticAssetsProcessor : WorkspaceOperationsMonitor.IDisableAssetsProcessor
    {
        internal void SetWorkspaceOperationsMonitor(
            WorkspaceOperationsMonitor workspaceOperationsMonitor)
        {
            mWorkspaceOperationsMonitor = workspaceOperationsMonitor;
        }

        internal void AddToSourceControl(string[] paths)
        {
            if (IsDisableBecauseExceptionHappened(DateTime.Now))
            {
                mLog.Warn(
                    "PlasticAssetsProcessor skipping AddToSourceControl operation " +
                    "because an exception happened in the last 60 seconds");
                return;
            }

            foreach (string path in paths)
                mLog.DebugFormat("AddToSourceControl: {0}", path);

            mWorkspaceOperationsMonitor.AddAssetsProcessorPathToAdd(paths.ToList());
        }

        internal void DeleteFromSourceControl(string path)
        {
            if (IsDisableBecauseExceptionHappened(DateTime.Now))
            {
                mLog.Warn(
                    "PlasticAssetsProcessor skipping DeleteFromSourceControl operation " +
                    "because an exception happened in the last 60 seconds");
                return;
            }

            mLog.DebugFormat("DeleteFromSourceControl: {0}", path);

            mWorkspaceOperationsMonitor.AddAssetsProcessorPathToDelete(path);
        }

        internal void MoveOnSourceControl(string srcPath, string dstPath)
        {
            if (IsDisableBecauseExceptionHappened(DateTime.Now))
            {
                mLog.Warn(
                    "PlasticAssetsProcessor skipping MoveOnSourceControl operation " +
                    "because an exception happened in the last 60 seconds");
                return;
            }

            mLog.DebugFormat("MoveOnSourceControl: {0} to {1}", srcPath, dstPath);

            mWorkspaceOperationsMonitor.AddAssetsProcessorPathToMove(srcPath, dstPath);
        }

        internal void CheckoutOnSourceControl(string[] paths)
        {
            if (IsDisableBecauseExceptionHappened(DateTime.Now))
            {
                mLog.Warn(
                    "PlasticAssetsProcessor skipping CheckoutOnSourceControl operation " + 
                    "because an exception happened in the last 60 seconds");
                return;
            }

            foreach (string path in paths)
                mLog.DebugFormat("CheckoutOnSourceControl: {0}", path);

            mWorkspaceOperationsMonitor.AddAssetsProcessorPathToCheckout(paths.ToList());
        }

        bool IsDisableBecauseExceptionHappened(DateTime now)
        {
            return (now - mLastExceptionDateTime).TotalSeconds < 5;
        }

        void WorkspaceOperationsMonitor.IDisableAssetsProcessor.Disable()
        {
            mLastExceptionDateTime = DateTime.Now;
        }

        DateTime mLastExceptionDateTime = DateTime.MinValue;
        WorkspaceOperationsMonitor mWorkspaceOperationsMonitor;

        static readonly ILog mLog = LogManager.GetLogger("PlasticAssetsProcessor");
    }
}