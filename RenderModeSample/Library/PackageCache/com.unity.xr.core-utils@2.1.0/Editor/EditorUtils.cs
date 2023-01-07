using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Utility methods for use in the Editor
    /// </summary>
    public static class EditorUtils
    {
        /// <summary>
        /// Get attributes from the field which represents a property
        /// </summary>
        /// <param name="property">The property from which to get attributes</param>
        /// <returns>Array of attributes from property</returns>
        public static Attribute[] GetMemberAttributes(SerializedProperty property)
        {
            var fi = GetFieldInfoFromProperty(property);
            return fi.GetCustomAttributes(false).Cast<Attribute>().ToArray();
        }

        /// <summary>
        /// Get the FieldInfo which backs a property
        /// </summary>
        /// <param name="property">The property from which to get FieldInfo</param>
        /// <returns>The FieldInfo which backs the property</returns>
        public static FieldInfo GetFieldInfoFromProperty(SerializedProperty property)
        {
            var memberInfo = GetMemberInfoFromPropertyPath(property.serializedObject.targetObject.GetType(), property.propertyPath, out _);
            if (memberInfo.MemberType != MemberTypes.Field)
                return null;

            return memberInfo as FieldInfo;
        }

        /// <summary>
        /// Get MemberInfo for a property by path
        /// </summary>
        /// <param name="host">The type of the container object</param>
        /// <param name="path">The property path to search for</param>
        /// <param name="type">The type of the property at path</param>
        /// <returns>The MemberInfo found on the host type at the given path</returns>
        public static MemberInfo GetMemberInfoFromPropertyPath(Type host, string path, out Type type)
        {
            type = host;
            if (host == null)
                return null;
            MemberInfo memberInfo = null;

            var parts = path.Split ('.');
            for (var i = 0; i < parts.Length; i++)
            {
                var member = parts[i];

                // Special handling of array elements.
                // The "Array" and "data[x]" parts of the propertyPath don't correspond to any types,
                // so they should be skipped by the code that drills down into the types.
                // However, we want to change the type from the type of the array to the type of the array
                // element before we do the skipping.
                if (i < parts.Length - 1 && member == "Array" && parts[i + 1].StartsWith ("data["))
                {
                    Type listType = null;
                    // ReSharper disable once PossibleNullReferenceException would have returned if host was null
                    if (type.IsArray)
                    {
                        listType = type.GetElementType();
                    }
                    else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        listType = type.GetGenericArguments()[0];
                    }
                    if (listType != null)
                        type = listType;

                    // Skip rest of handling for this part ("Array") and the next part ("data[x]").
                    i++;
                    continue;
                }

                // GetField on class A will not find private fields in base classes to A,
                // so we have to iterate through the base classes and look there too.
                // Private fields are relevant because they can still be shown in the Inspector,
                // and that applies to private fields in base classes too.
                MemberInfo foundMember = null;
                for (var currentType = type; foundMember == null && currentType != null; currentType =
                    currentType.BaseType)
                {
                    var foundMembers = currentType.GetMember(member, BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic);
                    if (foundMembers.Length > 0)
                    {
                        foundMember = foundMembers[0];
                    }
                }

                if (foundMember == null)
                {
                    type = null;
                    return null;
                }

                memberInfo = foundMember;
                switch (memberInfo.MemberType) {
                    case MemberTypes.Field:
                        var info = memberInfo as FieldInfo;
                        if (info != null)
                            type = info.FieldType;
                        break;
                    case MemberTypes.Property:
                        var propertyInfo = memberInfo as PropertyInfo;
                        if (propertyInfo != null)
                            type = propertyInfo.PropertyType;
                        break;
                    default:
                        type = memberInfo.DeclaringType;
                        break;
                }
            }

            return memberInfo;
        }

        /// <summary>
        /// Guess the type of a <c>SerializedProperty</c> and return a <c>System.Type</c>, if one exists.
        /// The guess is done by checking the type of the target object and iterating through its fields looking for
        /// one that matches the property name. This may return null if you give it a <c>SerializedProperty</c> that
        /// represents a native type with no managed equivalent
        /// </summary>
        /// <param name="property">The <c>SerializedProperty</c> to examine</param>
        /// <returns>The best guess type</returns>
        public static Type SerializedPropertyToType(SerializedProperty property)
        {
            var field = SerializedPropertyToField(property);
            return field != null ? field.FieldType : null;
        }

        /// <summary>
        /// Get the FieldInfo for a given property
        /// </summary>
        /// <param name="property">The property for which to get the FieldInfo</param>
        /// <returns>The FieldInfo for the property</returns>
        public static FieldInfo SerializedPropertyToField(SerializedProperty property)
        {
            var parts = property.propertyPath.Split('.');
            if (parts.Length == 0)
                return null;

            var currentType = property.serializedObject.targetObject.GetType();
            FieldInfo field = null;
            foreach (var part in parts)
            {
                if (part == "Array")
                {
                    currentType = field.FieldType.GetElementType();
                    continue;
                }

                field = currentType.GetFieldInTypeOrBaseType(part);
                if (field == null)
                    continue;

                currentType = field.FieldType;
            }

            return field;
        }

        /// <summary>
        /// Special version of EditorGUI.MaskField which ensures that only the chosen bits are set. We need this version of the
        /// function to check explicitly whether only a single bit was set.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for this control.</param>
        /// <param name="label">Label for the field.</param>
        /// <param name="mask">The current mask to display.</param>
        /// <param name="displayedOptions">A string array containing the labels for each flag.</param>
        /// <param name="propertyType">The type of the property</param>
        /// <returns>The value modified by the user.</returns>
        public static int MaskField(Rect position, GUIContent label, int mask, string[] displayedOptions, Type propertyType)
        {
            mask = EditorGUI.MaskField(position, label, mask, displayedOptions);
            return ActualEnumFlags(mask, propertyType);
        }

        /// <summary>
        /// Return a value with only bits that can be set with values in the enum to prevent multiple representations of the same state
        /// </summary>
        /// <param name="value">The flags value</param>
        /// <param name="t">The type of enum to use</param>
        /// <returns>The transformed flags value</returns>
        static int ActualEnumFlags(int value, Type t)
        {
            if (value < 0)
            {
                var mask = 0;
                foreach (var enumValue in Enum.GetValues(t))
                {
                    mask |= (int)enumValue;
                }

                value &= mask;
            }

            return value;
        }

        /// <summary>
        /// Strip PPtr&lt;&gt; and $ from a string for getting a System.Type from SerializedProperty.type
        /// </summary>
        /// <param name="type">Type string</param>
        /// <returns>Nicified type string</returns>
        public static string NicifySerializedPropertyType(string type)
        {
            return type.Replace("PPtr<", "").Replace(">", "").Replace("$", "");
        }

        /// <summary>
        /// Search through all assemblies in the current AppDomain for a class that is assignable to UnityObject and matches the given weak name
        /// TODO: expose internal SerializedProperty.ValidateObjectReferenceValue to remove his hack
        /// </summary>
        /// <param name="name">Weak type name</param>
        /// <returns>Best guess System.Type</returns>
        public static Type TypeNameToType(string name)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.Name.Equals(name) && typeof(UnityObject).IsAssignableFrom(type))
                            return type;
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip any assemblies that don't load properly
                }
            }

            return typeof(UnityObject);
        }

        /// <summary>
        /// Tries to get an asset preview. If one is not available, waits until IsLoadingAssetPreview is false, and if
        /// preview is still not loaded, returns the result of AssetPreview.GetMiniThumbnail
        /// </summary>
        /// <param name="asset">The asset for which to get a preview</param>
        /// <param name="callback">Called with the preview texture as an argument, when it is available</param>
        /// <returns>An enumerator used to tick the coroutine</returns>
        public static IEnumerator GetAssetPreview(UnityObject asset, Action<Texture> callback)
        {
            // GetAssetPreview will start loading the preview, or return one if available
            var texture = AssetPreview.GetAssetPreview(asset);

            // If the preview is not available, IsLoadingAssetPreview will be true until loading has finished
            while (AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
            {
                texture = AssetPreview.GetAssetPreview(asset);
                yield return null;
            }

            // If loading a preview fails, fall back to the MiniThumbnail
            if (!texture)
                texture = AssetPreview.GetMiniThumbnail(asset);

            callback(texture);
        }
    }
}
