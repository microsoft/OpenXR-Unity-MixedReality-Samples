using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.XR.OpenXR;

using UnityEngine.XR.OpenXR.Features.RuntimeDebugger;

namespace UnityEditor.XR.OpenXR.Features.RuntimeDebugger
{
    internal class DebuggerTreeView : TreeView
    {
        private int _mode;
        public new string searchString { get; set; }
        private bool deep = false;

        public DebuggerTreeView(TreeViewState state, int mode, string search="")
        : base(state)
        {
            _mode = mode;
            searchString = search;
            Reload();
        }

        public void ReloadDeepSearch()
        {
            deep = true;
            Reload();
            deep = false;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(0, -1, "Root");
            if (_mode == 0)
            {
                foreach (var t in DebuggerState._functionCalls)
                {
                    if (string.IsNullOrEmpty(searchString) || (deep ? t.ToString() : t.displayName).IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                        root.AddChild(t);
                }
            }
            else
            {
                if (DebuggerState.xrLut.TryGetValue((UInt32)_mode - 1, out var lut))
                {
                    foreach (var t in lut.Values)
                    {
                        if (string.IsNullOrEmpty(searchString) || (deep ? t.ToString() : t.displayName).IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                            root.AddChild(t);
                    }
                }
            }

            if (root.hasChildren)
                SetupDepthsFromParentsAndChildren(root);
            else
                root.AddChild(new TreeViewItem(0));

            return root;
        }

        protected override void KeyEvent()
        {
            if (Event.current.commandName == "Copy")
            {
                StringBuilder copy = new StringBuilder("");
                foreach (var id in state.selectedIDs)
                {
                    copy.Append(FindItem(id, rootItem).ToString());
                    copy.Append("\n");
                }

                EditorGUIUtility.systemCopyBuffer = copy.ToString();
            }
            base.KeyEvent();
        }
    }

    internal class RuntimeDebuggerWindow : EditorWindow
    {
        private static class Styles
        {
            public static GUIStyle s_Wrap;
        }

        private static void InitStyles()
        {
            if (Styles.s_Wrap != null)
                return;

            Styles.s_Wrap = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 5, 1, 1)
            };

        }

        [MenuItem("Window/Analysis/OpenXR Runtime Debugger")]
        [MenuItem("Window/XR/OpenXR/Runtime Debugger")]
        internal static void Init()
        {
            RuntimeDebuggerWindow w = EditorWindow.GetWindow<RuntimeDebuggerWindow>() as RuntimeDebuggerWindow;
            w.titleContent = new GUIContent("OpenXR Runtime Debugger");
            w.Show();
        }

        private IConnectionState state;
        void OnEnable()
        {
            state = PlayerConnectionGUIUtility.GetConnectionState(this);
            EditorConnection.instance.Initialize();
            EditorConnection.instance.Register(RuntimeDebuggerOpenXRFeature.kPlayerToEditorSendDebuggerOutput, DebuggerState.OnMessageEvent);
        }

        void OnDisable()
        {
            EditorConnection.instance.Unregister(RuntimeDebuggerOpenXRFeature.kPlayerToEditorSendDebuggerOutput, DebuggerState.OnMessageEvent);
            state.Dispose();
        }

        private Vector2 scrollpos = new Vector2();
        private List<TreeViewState> treeViewState = new List<TreeViewState>();
        private DebuggerTreeView treeView;
        private SearchField searchField = null;
        private string searchString = "";
        private int viewMode = 0;

        private string _lastRefreshStats;

        void Clear()
        {
            DebuggerState.Clear();
            treeView = null;
            searchField = new SearchField();
            treeViewState.Clear();
            _lastRefreshStats = "";
            scrollpos = Vector2.zero;
        }

        void OnGUI()
        {
            InitStyles();
            if (searchField == null)
                searchField = new SearchField();
            var debuggerFeatureInfo = FeatureHelpers.GetFeatureWithIdForActiveBuildTarget("com.unity.openxr.features.runtimedebugger");

            if (!debuggerFeatureInfo.enabled)
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("OpenXR Runtime Debugger must be enabled for this build target.", Styles.s_Wrap);
                EditorGUILayout.Space();
                if (GUILayout.Button("Enable Runtime Debugger"))
                {
                    debuggerFeatureInfo.enabled = true;
                }
                EditorGUILayout.EndVertical();
                return;
            }


            PlayerConnectionGUILayout.ConnectionTargetSelectionDropdown(state);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh"))
            {
                DebuggerState.SetDoneCallback(() =>
                {
                    if (treeViewState.Count != DebuggerState.lutNames.Count)
                    {
                        treeViewState.Clear();
                        for (int i = 0; i < DebuggerState.lutNames.Count; ++i)
                        {
                            treeViewState.Add(new TreeViewState());
                        }
                    }

                    treeView = new DebuggerTreeView(treeViewState[viewMode], viewMode, searchString);
                    searchField = new SearchField();

                    var debugger = OpenXRSettings.ActiveBuildTargetInstance.GetFeature<RuntimeDebuggerOpenXRFeature>();
                    if (debugger != null)
                        _lastRefreshStats = $"Last payload size: {DebuggerState._lastPayloadSize} ({((100.0f * DebuggerState._lastPayloadSize / debugger.cacheSize)):F2}% cache full) Number of Frames: {DebuggerState._frameCount}";
                    else
                        _lastRefreshStats = $"Last payload size: {DebuggerState._lastPayloadSize} Number of Frames: {DebuggerState._frameCount}";
                });

                _lastRefreshStats = "Refreshing ...";
                if (EditorApplication.isPlaying)
                {
                    var debugger = OpenXRSettings.Instance.GetFeature<RuntimeDebuggerOpenXRFeature>();
                    if (debugger.enabled)
                    {
                        debugger.RecvMsg(new MessageEventArgs());
                    }
                }
                else
                {
                    EditorConnection.instance.Send(RuntimeDebuggerOpenXRFeature.kEditorToPlayerRequestDebuggerOutput, new byte[] {byte.MinValue});
                }
            }

            if (GUILayout.Button("Clear"))
            {
                Clear();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs")))
            {
                string path = EditorUtility.SaveFilePanel("Save OpenXR Dump", "", state.connectionName, "openxrdump");
                if (path.Length != 0)
                {
                    DebuggerState.SaveToFile(path);
                }
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon")))
            {
                string path = EditorUtility.OpenFilePanelWithFilters("Load OpenXR Dump", "", new[] {"OpenXR Dump", "openxrdump"});
                if (path.Length != 0)
                {
                    Clear();

                    DebuggerState.SetDoneCallback(() =>
                    {
                        if (treeViewState.Count != DebuggerState.lutNames.Count)
                        {
                            treeViewState.Clear();
                            for (int i = 0; i < DebuggerState.lutNames.Count; ++i)
                            {
                                treeViewState.Add(new TreeViewState());
                            }
                        }

                        treeView = new DebuggerTreeView(treeViewState[viewMode], viewMode, searchString);

                        _lastRefreshStats = $"Last payload size: {DebuggerState._lastPayloadSize} Number of Frames: {DebuggerState._frameCount}";
                    });

                    DebuggerState.LoadFromFile(path);
                }
            }


            GUILayout.EndHorizontal();

            GUILayout.Label($"Connections: {EditorConnection.instance.ConnectedPlayers.Count}");
            GUILayout.Label(_lastRefreshStats);
            if (treeView != null)
            {
                GUILayout.BeginHorizontal();
                var newSearchString = searchField.OnGUI(treeView.searchString);
                if (newSearchString != treeView.searchString)
                {
                    treeView.searchString = newSearchString;
                    treeView.Reload();
                }

                if (searchField.HasFocus())
                {
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        treeView.ReloadDeepSearch();
                    }
                }

                if (GUILayout.Button("Deep Search"))
                {
                    treeView.ReloadDeepSearch();
                }

                GUILayout.EndHorizontal();
            }

            int newViewMode = 0;
            if (DebuggerState.lutNames.Count > 0)
            {
                newViewMode = EditorGUILayout.Popup(viewMode, DebuggerState.lutNames.ToArray());
            }
            if (newViewMode != viewMode)
            {
                viewMode = newViewMode;
                treeView = new DebuggerTreeView(treeViewState[viewMode], viewMode, searchString);
                scrollpos = Vector2.zero;
            }

            var treeViewRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            if (treeView != null)
            {
                treeView.OnGUI(treeViewRect);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
