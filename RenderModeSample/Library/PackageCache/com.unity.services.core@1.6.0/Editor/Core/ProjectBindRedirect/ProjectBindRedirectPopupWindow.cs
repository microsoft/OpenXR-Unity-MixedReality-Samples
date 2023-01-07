using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.Core.Editor.ProjectBindRedirect
{
    class ProjectBindRedirectPopupWindow : EditorWindow
    {
        internal static readonly Vector2 PopupSize = new Vector2(600, 400);
        const string k_WindowTitle = "Link your Unity project";

        ProjectBindRedirectPopupUI m_PopupUI;
        IEditorGameServiceAnalyticsSender m_EditorGameServiceAnalyticsSender;

        void Update()
        {
            if (RequiresInitialization())
            {
                Initialize(new List<string>());
            }
        }

        bool RequiresInitialization()
        {
            return m_PopupUI == null;
        }

        internal static ProjectBindRedirectPopupWindow CreateAndShowPopup([NotNull] List<string> installedPackages, [NotNull] IEditorGameServiceAnalyticsSender editorGameServiceAnalyticsSender)
        {
            var popupWindow = GetWindow<ProjectBindRedirectPopupWindow>(k_WindowTitle);
            popupWindow.m_EditorGameServiceAnalyticsSender = editorGameServiceAnalyticsSender;
            popupWindow.Initialize(installedPackages);

            return popupWindow;
        }

        void TrackDisplayedPopup([NotNull] List<string> packageList)
        {
            foreach (var package in packageList)
            {
                m_EditorGameServiceAnalyticsSender.SendProjectBindPopupDisplayedEvent(package);
            }
        }

        void Initialize(List<string> installedPackages)
        {
            rootVisualElement?.Clear();

            m_PopupUI = new ProjectBindRedirectPopupUI(rootVisualElement, Close, installedPackages, m_EditorGameServiceAnalyticsSender);
            TrackDisplayedPopup(installedPackages);

            maxSize = minSize = PopupSize;
        }
    }
}
