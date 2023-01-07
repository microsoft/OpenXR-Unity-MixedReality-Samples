using UnityEngine;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class ReportGeneratorStyles
    {
        public static GUIContent ProgressTitle = EditorGUIUtility.TrTextContent("Code Coverage");
        public static GUIContent ProgressInfo = EditorGUIUtility.TrTextContent("Preparing the Report..");
        public static GUIContent ProgressInfoPreparing = EditorGUIUtility.TrTextContent("Preparing the Report..");
        public static GUIContent ProgressInfoCreating = EditorGUIUtility.TrTextContent("Creating the Report..");
    }
}