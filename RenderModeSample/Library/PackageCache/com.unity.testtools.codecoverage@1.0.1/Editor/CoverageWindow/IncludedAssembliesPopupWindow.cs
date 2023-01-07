using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace UnityEditor.TestTools.CodeCoverage
{
    class IncludedAssembliesPopupWindow : PopupWindowContent
    {
        SearchField m_SearchField;
        IncludedAssembliesTreeView m_TreeView;

        const float kWindowHeight = 221;

        public float Width { get; set; }

        class Styles
        {
            public static GUIContent SelectAllButtonLabel = EditorGUIUtility.TrTextContent("Select All");
            public static GUIContent DeselectAllButtonLabel = EditorGUIUtility.TrTextContent("Deselect All");
        }

        public IncludedAssembliesPopupWindow(CodeCoverageWindow parent, string assembliesToInclude)
        {
            m_SearchField = new SearchField();
            m_TreeView = new IncludedAssembliesTreeView(parent, assembliesToInclude);
        }

        public override void OnGUI(Rect rect)
        {
            const int border = 4;
            const int topPadding = 12;
            const int searchHeight = 20;
            const int buttonHeight = 16;
            const int remainTop = topPadding + searchHeight + buttonHeight + border + border;

            float selectAllWidth = EditorStyles.miniButton.CalcSize(Styles.SelectAllButtonLabel).x;
            float deselectAllWidth = EditorStyles.miniButton.CalcSize(Styles.DeselectAllButtonLabel).x;

            Rect searchRect = new Rect(border, topPadding, rect.width - border * 2, searchHeight);
            Rect selectAllRect = new Rect(border, topPadding + searchHeight + border, selectAllWidth, buttonHeight);
            Rect deselectAllRect = new Rect(border + selectAllWidth + border, topPadding + searchHeight + border, deselectAllWidth, buttonHeight);
            Rect remainingRect = new Rect(border, remainTop, rect.width - border * 2, rect.height - remainTop - border);

            m_TreeView.searchString = m_SearchField.OnGUI(searchRect, m_TreeView.searchString);

            if (GUI.Button(selectAllRect, Styles.SelectAllButtonLabel, EditorStyles.miniButton))
            {
                m_TreeView.SelectAll();
            }

            if (GUI.Button(deselectAllRect, Styles.DeselectAllButtonLabel, EditorStyles.miniButton))
            {
                m_TreeView.DeselectAll();
            }

            m_TreeView.OnGUI(remainingRect);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(Mathf.Max(Width, m_TreeView.Width), kWindowHeight);
        }

        public override void OnOpen()
        {
            m_SearchField.SetFocus();
            base.OnOpen();
        }
    }
}