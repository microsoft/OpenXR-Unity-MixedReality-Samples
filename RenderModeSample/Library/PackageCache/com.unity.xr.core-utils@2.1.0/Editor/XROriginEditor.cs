// ENABLE_VR is not defined on Game Core but the assembly is available with limited features when the XR module is enabled.
#if ENABLE_VR || UNITY_GAMECORE
#define XR_MODULE_AVAILABLE
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;
using UnityEditor;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Custom editor for an <see cref="XROrigin"/>.
    /// </summary>
    [CustomEditor(typeof(XROrigin), true), CanEditMultipleObjects]
    public class XROriginEditor : UnityEditor.Editor
    {
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XROrigin.Origin"/>.</summary>
        protected SerializedProperty m_OriginBaseGameObject;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XROrigin.CameraFloorOffsetObject"/>.</summary>
        protected SerializedProperty m_CameraFloorOffsetObject;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XROrigin.Camera"/>.</summary>
        protected SerializedProperty m_Camera;
        /// <summary>m_TrackingOriginMode has been deprecated. Use m_RequestedTrackingOriginMode instead.</summary>
        [Obsolete("m_TrackingOriginMode has been deprecated. Use m_RequestedTrackingOriginMode instead.")]
        protected SerializedProperty m_TrackingOriginMode;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XROrigin.RequestedTrackingOriginMode"/>.</summary>
        protected SerializedProperty m_RequestedTrackingOriginMode;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XROrigin.CameraYOffset"/>.</summary>
        protected SerializedProperty m_CameraYOffset;

        List<XROrigin> m_Origins;

        readonly GUIContent[] m_MixedValuesOptions = { Contents.MixedValues };

        bool m_InitializedKnownSerializedPropertyNames;

        /// <summary>
        /// The <see cref="SerializeField"/> names of all <see cref="SerializedProperty"/> fields
        /// defined in the <see cref="Editor"/> (including derived types).
        /// </summary>
        /// <seealso cref="InitializeKnownSerializedPropertyNames"/>
        protected string[] knownSerializedPropertyNames { get; set; }

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="XROrigin.Origin"/>.</summary>
            public static readonly GUIContent Origin = EditorGUIUtility.TrTextContent("Origin Base GameObject", "The \"Origin\" GameObject is used to refer to the base of the XR Origin, by default it is this GameObject. This is the GameObject that will be manipulated via locomotion.");
            /// <summary><see cref="GUIContent"/> for <see cref="XROrigin.CameraFloorOffsetObject"/>.</summary>
            public static readonly GUIContent CameraFloorOffsetObject = EditorGUIUtility.TrTextContent("Camera Floor Offset Object", "The GameObject to move to desired height off the floor (defaults to this object if none provided).");
            /// <summary><see cref="GUIContent"/> for <see cref="XROrigin.Camera"/>.</summary>
            public static readonly GUIContent Camera = EditorGUIUtility.TrTextContent("Camera GameObject", "The GameObject that contains the camera, this is usually the \"Head\" of XR Origins.");
            /// <summary><see cref="GUIContent"/> for <see cref="XROrigin.RequestedTrackingOriginMode"/>.</summary>
            public static readonly GUIContent TrackingOriginMode = EditorGUIUtility.TrTextContent("Tracking Origin Mode", "The type of tracking origin to use for this Origin. Tracking origins identify where (0, 0, 0) is in the world of tracking.");
            /// <summary><see cref="GUIContent"/> for <see cref="XROrigin.CurrentTrackingOriginMode"/>.</summary>
            public static readonly GUIContent CurrentTrackingOriginMode = EditorGUIUtility.TrTextContent("Current Tracking Origin Mode", "The Tracking Origin Mode that this Origin is in.");
            /// <summary><see cref="GUIContent"/> for <see cref="XROrigin.CameraYOffset"/>.</summary>
            public static readonly GUIContent CameraYOffset = EditorGUIUtility.TrTextContent("Camera Y Offset", "Camera height to be used when in \"Device\" Tracking Origin Mode to define the height of the user from the floor.");
            /// <summary><see cref="GUIContent"/> to indicate mixed values when multi-object editing.</summary>
            public static readonly GUIContent  MixedValues = EditorGUIUtility.TrTextContent("\u2014", "Mixed Values");
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        /// <seealso cref="MonoBehaviour"/>
        protected virtual void OnEnable()
        {
            m_OriginBaseGameObject = serializedObject.FindProperty("m_OriginBaseGameObject");
            m_CameraFloorOffsetObject = serializedObject.FindProperty("m_CameraFloorOffsetObject");
            m_Camera = serializedObject.FindProperty("m_Camera");
#pragma warning disable 618 // Setting deprecated field to help with backwards compatibility with existing user code.
            m_TrackingOriginMode = serializedObject.FindProperty("m_TrackingOriginMode");
#pragma warning restore 618
            m_RequestedTrackingOriginMode = serializedObject.FindProperty("m_RequestedTrackingOriginMode");
            m_CameraYOffset = serializedObject.FindProperty("m_CameraYOffset");

            m_Origins = targets.Cast<XROrigin>().ToList();
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            InitializeKnownSerializedPropertyNames();

            serializedObject.Update();

            DrawInspector();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// This method is automatically called by <see cref="OnInspectorGUI"/> to
        /// draw the custom inspector. Override this method to customize the
        /// inspector as a whole.
        /// </summary>
        /// <seealso cref="DrawBeforeProperties"/>
        /// <seealso cref="DrawProperties"/>
        /// <seealso cref="DrawDerivedProperties"/>
        protected virtual void DrawInspector()
        {
            DrawBeforeProperties();
            DrawProperties();
            DrawDerivedProperties();
        }

        /// <summary>
        /// This method is automatically called by <see cref="OnInspectorGUI"/> to
        /// initialize <see cref="knownSerializedPropertyNames"/> if necessary.
        /// This is used together with <see cref="DrawDerivedProperties"/> to draw all unknown
        /// serialized fields from derived classes.
        /// </summary>
        /// <seealso cref="DrawDerivedProperties"/>
        protected virtual void InitializeKnownSerializedPropertyNames()
        {
            if (m_InitializedKnownSerializedPropertyNames)
                return;

            knownSerializedPropertyNames = GetDerivedSerializedPropertyNames().ToArray();
            m_InitializedKnownSerializedPropertyNames = true;
        }

        /// <summary>
        /// Returns a list containing the <see cref="SerializeField"/> names of all <see cref="SerializedProperty"/> fields
        /// defined in the <see cref="Editor"/> (including derived types).
        /// </summary>
        /// <returns>Returns a list of strings with property names.</returns>
        protected virtual List<string> GetDerivedSerializedPropertyNames()
        {
            return GetDerivedSerializedPropertyNames(this);
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the property fields of derived classes that are not explicitly defined in the <see cref="Editor"/>.
        /// </summary>
        /// <remarks>
        /// This method is used to allow users to add a <see cref="SerializeField"/> to a derived behavior
        /// and have it automatically appear in the Inspector while still having the custom Editor
        /// apply to that derived class.
        /// <br />
        /// When a derived <see cref="Editor"/> class adds a <see cref="SerializedProperty"/>,
        /// it will no longer automatically be drawn by this method. This is to allow users to customize
        /// where the property is drawn in the Inspector window. The derived <see cref="Editor"/> class
        /// does not need to explicitly add the <see cref="SerializedProperty"/> if the user is fine with
        /// the default location of where it will be drawn.
        /// </remarks>
        /// <seealso cref="InitializeKnownSerializedPropertyNames"/>
        protected virtual void DrawDerivedProperties()
        {
            DrawPropertiesExcluding(serializedObject, knownSerializedPropertyNames);
        }

        /// <summary>
        /// Draw the standard read-only Script property.
        /// </summary>
        protected virtual void DrawScript()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                if (target is MonoBehaviour behaviour)
                    EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent("Script"), MonoScript.FromMonoBehaviour(behaviour), typeof(MonoBehaviour), false);
                else if (target is ScriptableObject scriptableObject)
                    EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent("Script"), MonoScript.FromScriptableObject(scriptableObject), typeof(ScriptableObject), false);
            }
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the section of the custom inspector before <see cref="DrawProperties"/>.
        /// By default, this draws the read-only Script property.
        /// </summary>
        protected virtual void DrawBeforeProperties()
        {
            DrawScript();
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the property fields. Override this method to customize the
        /// properties shown in the Inspector. This is typically the method overridden
        /// when a derived behavior adds additional serialized properties
        /// that should be displayed in the Inspector.
        /// </summary>
        protected virtual void DrawProperties()
        {
            EditorGUILayout.PropertyField(m_OriginBaseGameObject, Contents.Origin);
            EditorGUILayout.PropertyField(m_CameraFloorOffsetObject, Contents.CameraFloorOffsetObject);
            EditorGUILayout.PropertyField(m_Camera, Contents.Camera);

            EditorGUILayout.PropertyField(m_RequestedTrackingOriginMode, Contents.TrackingOriginMode);

            var showCameraYOffset =
                m_RequestedTrackingOriginMode.enumValueIndex == (int)XROrigin.TrackingOriginMode.NotSpecified ||
                m_RequestedTrackingOriginMode.enumValueIndex == (int)XROrigin.TrackingOriginMode.Device ||
                m_RequestedTrackingOriginMode.hasMultipleDifferentValues;
            if (showCameraYOffset)
            {
#if XR_MODULE_AVAILABLE
                // The property should be enabled when not playing since the default for the XR device
                // may be Device, so the property should be editable to define the offset.
                // When playing, disable the property to convey that it isn't having an effect,
                // which is when the current mode is Floor.
                var currentTrackingOriginMode = ((XROrigin)target).CurrentTrackingOriginMode;
                var allCurrentlyFloor = (m_Origins.Count == 1 && currentTrackingOriginMode == TrackingOriginModeFlags.Floor) ||
                    m_Origins.All(origin => origin.CurrentTrackingOriginMode == TrackingOriginModeFlags.Floor);
                var disabled = Application.isPlaying &&
                    !m_RequestedTrackingOriginMode.hasMultipleDifferentValues &&
                    m_RequestedTrackingOriginMode.enumValueIndex == (int)XROrigin.TrackingOriginMode.NotSpecified &&
                    allCurrentlyFloor;
#else
                const bool disabled = false;
#endif
                using (new EditorGUI.IndentLevelScope())
                using (new EditorGUI.DisabledScope(disabled))
                {
                    EditorGUILayout.PropertyField(m_CameraYOffset, Contents.CameraYOffset);
                }
            }

            DrawCurrentTrackingOriginMode();
        }

        /// <summary>
        /// Draw the current Tracking Origin Mode while the application is playing.
        /// </summary>
        /// <seealso cref="XROrigin.CurrentTrackingOriginMode"/>
        protected void DrawCurrentTrackingOriginMode()
        {
#if XR_MODULE_AVAILABLE
            if (!Application.isPlaying)
                return;

            using (new EditorGUI.DisabledScope(true))
            {
                var currentTrackingOriginMode = ((XROrigin)target).CurrentTrackingOriginMode;
                if (m_Origins.Count == 1 || m_Origins.All(origin => origin.CurrentTrackingOriginMode == currentTrackingOriginMode))
                    EditorGUILayout.EnumPopup(Contents.CurrentTrackingOriginMode, currentTrackingOriginMode);
                else
                    EditorGUILayout.Popup(Contents.CurrentTrackingOriginMode, 0, m_MixedValuesOptions);
            }
#endif
        }

        /// <summary>
        /// Returns a list containing the <see cref="SerializeField"/> names of all <see cref="SerializedProperty"/> fields
        /// defined in the Editor (including derived types).
        /// </summary>
        /// <param name="editor">The <see cref="Editor"/> instance to reflect.</param>
        /// <returns>Returns a list of strings with property names.</returns>
        static List<string> GetDerivedSerializedPropertyNames(UnityEditor.Editor editor)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            var fields = editor.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var propertyNames = new List<string> { "m_Script" };
            foreach (var field in fields)
            {
                var value = field.GetValue(editor);
                if (value is SerializedProperty serializedProperty)
                {
                    propertyNames.Add(serializedProperty.name);
                }
            }

            return propertyNames;
        }
    }
}
