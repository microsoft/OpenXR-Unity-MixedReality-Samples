using System.Linq;
using System;
using UnityEngine;
using UnityEditor.TestTools.CodeCoverage.Utils;
using UnityEditor.TestTools.CodeCoverage.Analytics;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class PathsToAddHandler
    {
        string m_PathsToFilter;
        CodeCoverageWindow m_Parent;
        PathFilterType m_PathFilterType;

        private readonly string kSelectIncludedDirectoryMessage = L10n.Tr($"Select directory to include");
        private readonly string kSelectIncludedFileMessage = L10n.Tr("Select file to include");
        private readonly string kSelectExcludedDirectoryMessage = L10n.Tr($"Select directory to exclude");
        private readonly string kSelectExcludedFileMessage = L10n.Tr("Select file to exclude");

        class Styles
        {
            public static readonly GUIContent PathsToFilterAddFolderLabel = EditorGUIUtility.TrTextContent("Add Folder");
            public static readonly GUIContent PathsToFilterAddFileLabel = EditorGUIUtility.TrTextContent("Add File");
        }

        public PathsToAddHandler(CodeCoverageWindow parent, PathFilterType type)
        {
            m_Parent = parent;
            m_PathFilterType = type;
        }

        public void BrowseForDir(string pathsToFilter)
        {
            m_PathsToFilter = pathsToFilter;

            string candidate = CoverageUtils.BrowseForDir(Application.dataPath, m_PathFilterType == PathFilterType.Include ? kSelectIncludedDirectoryMessage : kSelectExcludedDirectoryMessage);
            if (CoverageUtils.IsValidFolder(candidate))
            {
                candidate = string.Concat(candidate, "/*");

                UpdatePathToFilter(candidate);
            }
        }

        public void BrowseForFile(string pathsToFilter)
        {
            m_PathsToFilter = pathsToFilter;

            string candidate = CoverageUtils.BrowseForFile(Application.dataPath, m_PathFilterType == PathFilterType.Include ? kSelectIncludedFileMessage : kSelectExcludedFileMessage);
            if (CoverageUtils.IsValidFile(candidate))
            {
                UpdatePathToFilter(candidate);
            }
        }

        private void UpdatePathToFilter(string candidate)
        {
            string[] pathFilters = m_PathsToFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            candidate = CoverageUtils.NormaliseFolderSeparators(candidate);

            if (!pathFilters.Contains(candidate))
            {
                if (m_PathsToFilter.Length > 0)
                    m_PathsToFilter += ",";
                m_PathsToFilter += candidate;

                if (m_PathFilterType == PathFilterType.Include)
                {
                    m_Parent.PathsToInclude = m_PathsToFilter;
                } 
                else
                {
                    m_Parent.PathsToExclude = m_PathsToFilter;
                }

                m_Parent.LoseFocus();
            }
        }
    }
}
