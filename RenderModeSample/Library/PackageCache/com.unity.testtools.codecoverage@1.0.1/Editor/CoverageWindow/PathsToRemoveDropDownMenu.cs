using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class PathsToRemoveDropDownMenu
    {
        GenericMenu m_Menu;
        ReorderableList m_ReorderableList;
        List<string> m_List;
        CodeCoverageWindow m_Parent;
        PathFilterType m_PathFilterType;

        class Styles
        {
            public static readonly GUIContent RemoveSelectedLabel = EditorGUIUtility.TrTextContent("Remove Selected");
            public static readonly GUIContent RemoveAllLabel = EditorGUIUtility.TrTextContent("Remove All");
        }

        public PathsToRemoveDropDownMenu(CodeCoverageWindow parent, PathFilterType type)
        {
            m_Parent = parent;
            m_PathFilterType = type;
        }

        private void PopulateMenu()
        {
            m_Menu = new GenericMenu();

            if (m_ReorderableList.index >= 0 && m_ReorderableList.index < m_List.Count && m_ReorderableList.HasKeyboardControl())
                m_Menu.AddItem(Styles.RemoveSelectedLabel, false, () => RemoveSelected());
            else
                m_Menu.AddDisabledItem(Styles.RemoveSelectedLabel);

            m_Menu.AddItem(Styles.RemoveAllLabel, false, () => RemoveAll());
        }

        public void Show(Rect position, ReorderableList reorderableList, List<string> list)
        {
            m_ReorderableList = reorderableList;
            m_List = list;
            
            PopulateMenu();

            m_Menu.DropDown(position);
        }

        private void RemoveSelected()
        {
            m_List.RemoveAt(m_ReorderableList.index);
            UpdatePathToFilter();
        }

        private void RemoveAll()
        {
            m_List.Clear();
            UpdatePathToFilter();
        }

        private void UpdatePathToFilter()
        {         
            if (m_PathFilterType == PathFilterType.Include)
            {
                m_Parent.PathsToInclude = string.Join(",", m_List);
            } 
            else
            {
                m_Parent.PathsToExclude = string.Join(",", m_List);
            }

            m_Parent.LoseFocus();
        }
    }
}
