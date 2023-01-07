using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Codice.Client.BaseCommands;
using Codice.Client.BaseCommands.Config;
using Codice.Client.Commands;
using Codice.Client.Commands.WkTree;
using Codice.LogWrapper;
using GluonGui;
using PlasticGui;
using PlasticGui.WorkspaceWindow;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.Views.IncomingChanges;
using Unity.PlasticSCM.Editor.Views.PendingChanges;

namespace Unity.PlasticSCM.Editor
{
    internal class WorkspaceOperationsMonitor
    {
        public interface IDisableAssetsProcessor
        {
            void Disable();
        }

        internal WorkspaceOperationsMonitor(
            IPlasticAPI plasticApi,
            IDisableAssetsProcessor disableAssetsProcessor,
            bool isGluonMode)
        {
            mPlasticAPI = plasticApi;
            mDisableAssetsProcessor = disableAssetsProcessor;
            mIsGluonMode = isGluonMode;
        }

        internal void RegisterWindow(
            WorkspaceWindow workspaceWindow,
            ViewHost viewHost,
            NewIncomingChangesUpdater incomingChangesUpdater)
        {
            mWorkspaceWindow = workspaceWindow;
            mViewHost = viewHost;
            mNewIncomingChangesUpdater = incomingChangesUpdater;
        }

        internal void UnRegisterWindow()
        {
            mWorkspaceWindow = null;
            mViewHost = null;
            mNewIncomingChangesUpdater = null;
        }

        internal void RegisterPendingChangesView(
            PendingChangesTab pendingChangesTab)
        {
            mPendingChangesTab = pendingChangesTab;
        }

        internal void RegisterIncomingChangesView(
            IIncomingChangesTab incomingChangesTab)
        {
            mIncomingChangesTab = incomingChangesTab;
        }

        internal void UnRegisterViews()
        {
            mPendingChangesTab = null;
            mIncomingChangesTab = null;
        }

        internal void Start()
        {
            mIsRunning = true;

            Thread thread = new Thread(TaskLoopThread);
            thread.IsBackground = true;
            thread.Start();
        }

        internal void Stop()
        {
            SetAsFinished();
        }

        internal void AddAssetsProcessorPathToAdd(
            List<string> paths)
        {
            AddPathsToProcess(
                mAssetsProcessorPathsToAdd, paths,
                mLock, mResetEvent);
        }

        internal void AddAssetsProcessorPathToDelete(
            string path)
        {
            AddPathsToProcess(
                mAssetsProcessorPathsToDelete,
                new List<string> { path },
                mLock, mResetEvent);
        }

        internal void AddAssetsProcessorPathToCheckout(
            List<string> paths)
        {
            AddPathsToProcess(
                mAssetsProcessorPathsToCheckout, paths,
                mLock, mResetEvent);
        }

        internal void AddAssetsProcessorPathToMove(
            string srcPath,
            string dstPath)
        {
            AddPathToMoveToProcess(
                mAssetsProcessorPathsToMove,
                new PathToMove(srcPath, dstPath),
                mLock, mResetEvent);
        }

        internal void AddPathsToCheckout(
            List<string> paths)
        {
            AddPathsToProcess(
                mPathsToCheckout, paths,
                mLock, mResetEvent);
        }

        void TaskLoopThread()
        {
            while (true)
            {
                try
                {
                    if (!mIsRunning)
                        break;

                    ProcessAssetProcessorOperations(
                        mPlasticAPI,
                        mAssetsProcessorPathsToAdd,
                        mAssetsProcessorPathsToDelete,
                        mAssetsProcessorPathsToCheckout,
                        mAssetsProcessorPathsToMove,
                        mLock,
                        mDisableAssetsProcessor);

                    ProcessCheckoutOperation(
                        mPlasticAPI,
                        mPathsToCheckout,
                        mLock);

                    bool hasAssetProcessorOperations = false;
                    bool hasCheckoutOperations = false;
                    HasPendingOperationsToProcess(
                        mAssetsProcessorPathsToAdd,
                        mAssetsProcessorPathsToDelete,
                        mAssetsProcessorPathsToCheckout,
                        mAssetsProcessorPathsToMove,
                        mPathsToCheckout,
                        mLock,
                        out hasAssetProcessorOperations,
                        out hasCheckoutOperations);

                    if (hasAssetProcessorOperations ||
                        hasCheckoutOperations)
                        continue;

                    if (!hasAssetProcessorOperations)
                        EditorDispatcher.Dispatch(AfterAssetProcessorOperation);

                    if (!hasCheckoutOperations)
                        EditorDispatcher.Dispatch(AfterCheckoutOperation);

                    SleepUntilNextWorkload();
                }
                catch (Exception e)
                {
                    mLog.ErrorFormat(
                        "Error running the tasks loop : {0}", e.Message);
                    mLog.DebugFormat(
                        "Stacktrace: {0}", e.StackTrace);
                }
            }
        }

        void AfterAssetProcessorOperation()
        {
            AutoRefresh.PendingChangesView(
                mPendingChangesTab);

            AutoRefresh.IncomingChangesView(
                mIncomingChangesTab);
        }

        void AfterCheckoutOperation()
        {
            RefreshAsset.VersionControlCache();

            if (mIsGluonMode)
            {
                RefreshViewsAfterCheckoutForGluon(mViewHost);
                return;
            }

            if (mNewIncomingChangesUpdater != null)
                mNewIncomingChangesUpdater.Update();

            RefreshViewsAfterCheckoutForDeveloper(mWorkspaceWindow);
        }

        void SetAsFinished()
        {
            if (!mIsRunning)
                return;

            mIsRunning = false;
            mResetEvent.Set();
        }

        void SleepUntilNextWorkload()
        {
            mResetEvent.Reset();
            mResetEvent.WaitOne();
        }

        static void ProcessAssetProcessorOperations(
            IPlasticAPI plasticApi,
            List<string> assetsProcessorPathsToAdd,
            List<string> assetsProcessorPathsToDelete,
            List<string> assetsProcessorPathsToCheckout,
            List<PathToMove> assetsProcessorPathsToMove,
            object lockObj,
            IDisableAssetsProcessor disableAssetsProcessor)
        {
            try
            {
                AssetsProcessorOperations.AddIfNotControlled(
                    plasticApi, ExtractPathsToProcess(
                        assetsProcessorPathsToAdd, lockObj),
                    FilterManager.Get().GetIgnoredFilter());

                AssetsProcessorOperations.DeleteIfControlled(
                    plasticApi, ExtractPathsToProcess(
                        assetsProcessorPathsToDelete, lockObj));

                AssetsProcessorOperations.CheckoutIfControlledAndChanged(
                    plasticApi, ExtractPathsToProcess(
                        assetsProcessorPathsToCheckout, lockObj));

                AssetsProcessorOperations.MoveIfControlled(
                    plasticApi, ExtractPathsToMoveToProcess(
                        assetsProcessorPathsToMove, lockObj));
            }
            catch (Exception ex)
            {
                LogException(ex);

                disableAssetsProcessor.Disable();
            }
        }

        static void ProcessCheckoutOperation(
            IPlasticAPI plasticApi,
            List<string> pathsToProcess,
            object lockObj)
        {
            List<string> paths = ExtractPathsToProcess(
                pathsToProcess, lockObj);

            if (paths.Count == 0)
                return;

            plasticApi.Checkout(
                paths.ToArray(),
                CheckoutModifiers.ProcessSymlinks);
        }

        static void AddPathsToProcess(
            List<string> pathsToProcess,
            List<string> paths,
            object lockObj,
            ManualResetEvent resetEvent)
        {
            lock (lockObj)
            {
                pathsToProcess.AddRange(paths);
            }

            resetEvent.Set();
        }

        static void AddPathToMoveToProcess(
            List<PathToMove> pathsToProcess,
            PathToMove path,
            object lockObj,
            ManualResetEvent resetEvent)
        {
            lock (lockObj)
            {
                pathsToProcess.Add(path);
            }

            resetEvent.Set();
        }

        static List<string> ExtractPathsToProcess(
            List<string> pathsToProcess,
            object lockObj)
        {
            List<string> result;

            lock (lockObj)
            {
                result = new List<string>(pathsToProcess);
                pathsToProcess.Clear();
            }

            return result;
        }

        static List<PathToMove> ExtractPathsToMoveToProcess(
            List<PathToMove> pathsToProcess,
            object lockObj)
        {
            List<PathToMove> result;

            lock (lockObj)
            {
                result = new List<PathToMove>(pathsToProcess);
                pathsToProcess.Clear();
            }

            return result;
        }

        static void HasPendingOperationsToProcess(
            List<string> assetsProcessorPathsToAdd,
            List<string> assetsProcessorPathsToDelete,
            List<string> assetsProcessorPathsToCheckout,
            List<PathToMove> assetsProcessorPathsToMove,
            List<string> pathsToCheckout,
            object lockObj,
            out bool hasAssetProcessorOperations,
            out bool hasCheckoutOperations)
        {
            lock (lockObj)
            {
                hasAssetProcessorOperations =
                    assetsProcessorPathsToAdd.Count > 0 ||
                    assetsProcessorPathsToDelete.Count > 0 ||
                    assetsProcessorPathsToCheckout.Count > 0 ||
                    assetsProcessorPathsToMove.Count > 0;

                hasCheckoutOperations =
                    pathsToCheckout.Count > 0;
            }
        }

        static void RefreshViewsAfterCheckoutForDeveloper(
            IWorkspaceWindow workspaceWindow)
        {
            if (workspaceWindow == null)
                return;

            workspaceWindow.RefreshView(ViewType.BranchExplorerView);
            workspaceWindow.RefreshView(ViewType.PendingChangesView);
            workspaceWindow.RefreshView(ViewType.HistoryView);
        }

        static void RefreshViewsAfterCheckoutForGluon(
            ViewHost viewHost)
        {
            if (viewHost == null)
                return;

            viewHost.RefreshView(ViewType.WorkspaceExplorerView);
            viewHost.RefreshView(ViewType.CheckinView);
            viewHost.RefreshView(ViewType.IncomingChangesView);
            viewHost.RefreshView(ViewType.SearchView);
        }

        static void LogException(Exception ex)
        {
            mLog.WarnFormat("Message: {0}", ex.Message);

            mLog.DebugFormat(
                "StackTrace:{0}{1}",
                Environment.NewLine, ex.StackTrace);
        }

        static class AssetsProcessorOperations
        {
            internal static void AddIfNotControlled(
                IPlasticAPI plasticApi,
                List<string> paths,
                IgnoredFilesFilter ignoredFilter)
            {
                List<string> fullPaths = new List<string>();

                foreach (string path in paths)
                {
                    string fullPath = Path.GetFullPath(path);
                    string fullPathMeta = MetaPath.GetMetaPath(fullPath);

                    if (plasticApi.GetWorkspaceFromPath(fullPath) == null)
                        return;

                    if (plasticApi.GetWorkspaceTreeNode(fullPath) == null &&
                        !ignoredFilter.IsIgnored(fullPath))
                        fullPaths.Add(fullPath);

                    if (File.Exists(fullPathMeta) &&
                        plasticApi.GetWorkspaceTreeNode(fullPathMeta) == null &&
                        !ignoredFilter.IsIgnored(fullPath))
                        fullPaths.Add(fullPathMeta);
                }

                if (fullPaths.Count == 0)
                    return;

                IList checkouts;
                plasticApi.Add(
                    fullPaths.ToArray(),
                    GetDefaultAddOptions(),
                    out checkouts);
            }

            internal static void DeleteIfControlled(
                IPlasticAPI plasticApi,
                List<string> paths)
            {
                foreach (string path in paths)
                {
                    string fullPath = Path.GetFullPath(path);
                    string fullPathMeta = MetaPath.GetMetaPath(fullPath);

                    if (plasticApi.GetWorkspaceTreeNode(fullPath) != null)
                    {
                        plasticApi.DeleteControlled(
                            fullPath, DeleteModifiers.None);
                    }

                    if (plasticApi.GetWorkspaceTreeNode(fullPathMeta) != null)
                    {
                        plasticApi.DeleteControlled(
                            fullPathMeta, DeleteModifiers.None);
                    }
                }
            }

            internal static void MoveIfControlled(
                IPlasticAPI plasticApi,
                List<PathToMove> paths)
            {
                foreach (PathToMove pathToMove in paths)
                {
                    string fullSrcPath = Path.GetFullPath(pathToMove.SrcPath);
                    string fullSrcPathMeta = MetaPath.GetMetaPath(fullSrcPath);

                    string fullDstPath = Path.GetFullPath(pathToMove.DstPath);
                    string fullDstPathMeta = MetaPath.GetMetaPath(fullDstPath);

                    if (plasticApi.GetWorkspaceTreeNode(fullSrcPath) != null)
                    {
                        plasticApi.Move(
                            fullSrcPath, fullDstPath,
                            MoveModifiers.None);
                    }

                    if (plasticApi.GetWorkspaceTreeNode(fullSrcPathMeta) != null)
                    {
                        plasticApi.Move(
                            fullSrcPathMeta, fullDstPathMeta,
                            MoveModifiers.None);
                    }
                }
            }

            internal static void CheckoutIfControlledAndChanged(
                IPlasticAPI plasticApi,
                List<string> paths)
            {
                List<string> fullPaths = new List<string>();

                foreach (string path in paths)
                {
                    string fullPath = Path.GetFullPath(path);
                    string fullPathMeta = MetaPath.GetMetaPath(fullPath);

                    WorkspaceTreeNode node =
                        plasticApi.GetWorkspaceTreeNode(fullPath);
                    WorkspaceTreeNode nodeMeta =
                        plasticApi.GetWorkspaceTreeNode(fullPathMeta);

                    if (node != null && ChangedFileChecker.IsChanged(
                        node.LocalInfo, fullPath, false))
                        fullPaths.Add(fullPath);

                    if (nodeMeta != null && ChangedFileChecker.IsChanged(
                        nodeMeta.LocalInfo, fullPathMeta, false))
                        fullPaths.Add(fullPathMeta);
                }

                if (fullPaths.Count == 0)
                    return;

                plasticApi.Checkout(
                    fullPaths.ToArray(),
                    CheckoutModifiers.None);
            }

            static AddOptions GetDefaultAddOptions()
            {
                AddOptions options = new AddOptions();
                options.AddPrivateParents = true;
                options.NeedCheckPlatformPath = true;
                return options;
            }
        }

        struct PathToMove
        {
            internal readonly string SrcPath;
            internal readonly string DstPath;

            internal PathToMove(string srcPath, string dstPath)
            {
                SrcPath = srcPath;
                DstPath = dstPath;
            }
        }

        object mLock = new object();
        volatile bool mIsRunning;
        volatile ManualResetEvent mResetEvent = new ManualResetEvent(false);

        List<string> mAssetsProcessorPathsToAdd = new List<string>();
        List<string> mAssetsProcessorPathsToDelete = new List<string>();
        List<string> mAssetsProcessorPathsToCheckout = new List<string>();
        List<PathToMove> mAssetsProcessorPathsToMove = new List<PathToMove>();
        List<string> mPathsToCheckout = new List<string>();

        PendingChangesTab mPendingChangesTab;
        IIncomingChangesTab mIncomingChangesTab;

        NewIncomingChangesUpdater mNewIncomingChangesUpdater;
        ViewHost mViewHost;
        IWorkspaceWindow mWorkspaceWindow;

        readonly bool mIsGluonMode = false;
        readonly IDisableAssetsProcessor mDisableAssetsProcessor;
        readonly IPlasticAPI mPlasticAPI;

        static readonly ILog mLog = LogManager.GetLogger("WorkspaceOperationsMonitor");
    }
}
