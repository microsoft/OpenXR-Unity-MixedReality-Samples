using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for Type objects
    /// </summary>
    public static class TypeExtensions
    {
        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<FieldInfo> k_Fields = new List<FieldInfo>();
        static readonly List<string> k_TypeNames = new List<string>();

        /// <summary>
        /// Add all types assignable to this one to a list, using an optional predicate test
        /// </summary>
        /// <param name="type">The type to which assignable types will be matched</param>
        /// <param name="list">The list to which assignable types will be appended</param>
        /// <param name="predicate">Custom delegate to allow user filtering of type list.
        /// Return false to ignore given type</param>
        public static void GetAssignableTypes(this Type type, List<Type> list, Func<Type, bool> predicate = null)
        {
            ReflectionUtils.ForEachType(t =>
            {
                if (type.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && (predicate == null || predicate(t)))
                    list.Add(t);
            });
        }

        /// <summary>
        /// Find all types that implement the given interface type, and append them to a list
        /// If the input type is not an interface type, no action is taken.
        /// </summary>
        /// <param name="type">The interface type whose implementors will be found</param>
        /// <param name="list">The list to which implementors will be appended</param>
        public static void GetImplementationsOfInterface(this Type type, List<Type> list)
        {
            if (type.IsInterface)
                GetAssignableTypes(type, list);
        }

        /// <summary>
        /// Find all types that extend the given class type, and append them to a list
        /// If the input type is not an class type, no action is taken.
        /// </summary>
        /// <param name="type">The class type of whom list will be found</param>
        /// <param name="list">The list to which extension types will be appended</param>
        public static void GetExtensionsOfClass(this Type type, List<Type> list)
        {
            if (type.IsClass)
                GetAssignableTypes(type, list);
        }

        /// <summary>
        /// Search through all interfaces implemented by this type and, if any of them match the given generic interface
        /// append them to a list
        /// </summary>
        /// <param name="type">The type whose interfaces will be searched</param>
        /// <param name="genericInterface">The generic interface used to match implemented interfaces</param>
        /// <param name="interfaces">The list to which generic interfaces will be appended</param>
        public static void GetGenericInterfaces(this Type type, Type genericInterface, List<Type> interfaces)
        {
            foreach (var typeInterface in type.GetInterfaces())
            {
                if (typeInterface.IsGenericType)
                {
                    var genericType = typeInterface.GetGenericTypeDefinition();
                    if (genericType == genericInterface)
                        interfaces.Add(typeInterface);
                }
            }
        }

        /// <summary>
        /// Gets a specific property of the Type or any of its base Types
        /// </summary>
        /// <param name="type">The type which will be searched for fields</param>
        /// <param name="name">Name of the property to get</param>
        /// <param name="bindingAttr">A bitmask specifying how the search is conducted</param>
        /// <returns>An object representing the field that matches the specified requirements, if found; otherwise, null</returns>
        public static PropertyInfo GetPropertyRecursively(this Type type, string name, BindingFlags bindingAttr)
        {
            var property = type.GetProperty(name, bindingAttr);
            if (property != null)
                return property;

            var baseType = type.BaseType;
            if (baseType != null)
                property = type.BaseType.GetPropertyRecursively(name, bindingAttr);

            return property;
        }

        /// <summary>
        /// Gets a specific field of the Type or any of its base Types
        /// </summary>
        /// <param name="type">The type which will be searched for fields</param>
        /// <param name="name">Name of the field to get</param>
        /// <param name="bindingAttr">A bitmask specifying how the search is conducted</param>
        /// <returns>An object representing the field that matches the specified requirements, if found; otherwise, null</returns>
        public static FieldInfo GetFieldRecursively(this Type type, string name, BindingFlags bindingAttr)
        {
            var field = type.GetField(name, bindingAttr);
            if (field != null)
                return field;

            var baseType = type.BaseType;
            if (baseType != null)
                field = type.BaseType.GetFieldRecursively(name, bindingAttr);

            return field;
        }

        /// <summary>
        /// Gets all fields of the Type or any of its base Types
        /// </summary>
        /// <param name="type">Type we are going to get fields on</param>
        /// <param name="fields">A list to which all fields of this type will be added</param>
        /// <param name="bindingAttr">A bitmask specifying how the search is conducted</param>
        public static void GetFieldsRecursively(this Type type, List<FieldInfo> fields,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
        {
            while (true)
            {
                foreach (var field in type.GetFields(bindingAttr))
                {
                    fields.Add(field);
                }

                var baseType = type.BaseType;
                if (baseType != null)
                {
                    type = baseType;
                    continue;
                }

                break;
            }
        }

        /// <summary>
        /// Gets all properties of the Type or any of its base Types
        /// </summary>
        /// <param name="type">Type we are going to get properties on</param>
        /// <param name="fields">A list to which all properties of this type will be added</param>
        /// <param name="bindingAttr">A bitmask specifying how the search is conducted</param>
        public static void GetPropertiesRecursively(this Type type, List<PropertyInfo> fields,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
        {
            while (true)
            {
                foreach (var property in type.GetProperties(bindingAttr))
                {
                    fields.Add(property);
                }

                var baseType = type.BaseType;
                if (baseType != null)
                {
                    type = baseType;
                    continue;
                }

                break;
            }
        }

        /// <summary>
        /// Gets the field info on a collection of classes that are from a collection of interfaces.
        /// </summary>
        /// <param name="classes">Collection of classes to get fields from.</param>
        /// <param name="fields">A list to which matching fields will be added</param>
        /// <param name="interfaceTypes">Collection of interfaceTypes to check if field type implements any interface type.</param>
        /// <param name="bindingAttr">Binding flags of fields.</param>
        public static void GetInterfaceFieldsFromClasses(this IEnumerable<Type> classes, List<FieldInfo> fields,
            List<Type> interfaceTypes, BindingFlags bindingAttr)
        {
            foreach (var type in interfaceTypes)
            {
                if (!type.IsInterface)
                    throw new ArgumentException($"Type {type} in interfaceTypes is not an interface!");
            }

            foreach (var type in classes)
            {
                if (!type.IsClass)
                    throw new ArgumentException($"Type {type} in classes is not a class!");

                k_Fields.Clear();
                type.GetFieldsRecursively(k_Fields, bindingAttr);
                foreach (var field in k_Fields)
                {
                    var interfaces = field.FieldType.GetInterfaces();
                    foreach (var @interface in interfaces)
                    {
                        if (interfaceTypes.Contains(@interface))
                        {
                            fields.Add(field);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the first attribute of a given type.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type to return</typeparam>
        /// <param name="type">The type whose attribute will be returned</param>
        /// <param name="inherit">Whether to search this type's inheritance chain to find the attribute</param>
        /// <returns>The first <typeparamref name="TAttribute"/> found</returns>
        public static TAttribute GetAttribute<TAttribute>(this Type type, bool inherit = false) where TAttribute : Attribute
        {
            Assert.IsTrue(type.IsDefined(typeof(TAttribute), inherit), "Attribute not found");
            return (TAttribute)type.GetCustomAttributes(typeof(TAttribute), inherit)[0];
        }

        /// <summary>
        /// Returns an array of types from the current type back to the declaring type that includes an inherited attribute.
        /// </summary>
        /// <typeparam name="TAttribute">Type of attribute we are checking if is defined.</typeparam>
        /// <param name="type">Type that has the attribute or inherits the attribute.</param>
        /// <param name="types">A list to which matching types will be added</param>
        public static void IsDefinedGetInheritedTypes<TAttribute>(this Type type, List<Type> types) where TAttribute : Attribute
        {
            while (type != null)
            {
                if (type.IsDefined(typeof(TAttribute), true))
                {
                    types.Add(type);
                }
                type = type.BaseType;
            }
        }

        /// <summary>
        /// Search by name through a fields of a type and its base types and return the field if one is found
        /// </summary>
        /// <param name="type">The type to search</param>
        /// <param name="fieldName">The name of the field to search for</param>
        /// <returns>The field, if found</returns>
        public static FieldInfo GetFieldInTypeOrBaseType(this Type type, string fieldName)
        {
            while (true)
            {
                if (type == null)
                    return null;

                var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                if (field != null)
                    return field;

                type = type.BaseType;
            }
        }

        /// <summary>
        /// Returns a human-readable name for a class with its generic arguments filled in
        /// </summary>
        /// <param name="type">The type to get a name for</param>
        /// <returns>The human-readable name</returns>
        public static string GetNameWithGenericArguments(this Type type)
        {
            var name = type.Name;

            // Replace + with . for sub-classes
            name = name.Replace('+', '.');

            if (!type.IsGenericType)
                return name;

            // Trim off `1
            name = name.Split('`')[0];

            var arguments = type.GetGenericArguments();
            var length = arguments.Length;
            var stringArguments = new string[length];
            for (var i = 0; i < length; i++)
            {
                stringArguments[i] = arguments[i].GetNameWithGenericArguments();
            }

            return $"{name}<{string.Join(", ", stringArguments)}>";
        }

        /// <summary>
        /// Returns a human-readable name for a class with its assembly qualified generic arguments filled in
        /// </summary>
        /// <param name="type">The type to get a name for</param>
        /// <returns>The human-readable name</returns>
        public static string GetNameWithFullGenericArguments(this Type type)
        {
            var name = type.Name;

            // Replace + with . for sub-classes
            name = name.Replace('+', '.');

            if (!type.IsGenericType)
                return name;

            // Trim off `1
            name = name.Split('`')[0];

            var arguments = type.GetGenericArguments();
            var length = arguments.Length;
            var stringArguments = new string[length];
            for (var i = 0; i < length; i++)
            {
                stringArguments[i] = arguments[i].GetFullNameWithGenericArgumentsInternal();
            }

            return $"{name}<{string.Join(", ", stringArguments)}>";
        }

        /// <summary>
        /// Returns a human-readable, assembly qualified name for a class with its assembly qualified generic arguments filled in
        /// </summary>
        /// <param name="type">The type to get a name for</param>
        /// <returns>The human-readable name</returns>
        public static string GetFullNameWithGenericArguments(this Type type)
        {
            // Handle sub-classes
            var declaringType = type.DeclaringType;
            if (declaringType != null && !type.IsGenericParameter)
            {
                k_TypeNames.Clear();
                var name = type.GetNameWithFullGenericArguments();
                k_TypeNames.Add(name);
                while (true)
                {
                    var parentDeclaringType = declaringType.DeclaringType;
                    if (parentDeclaringType == null)
                    {
                        name = declaringType.GetFullNameWithGenericArguments();
                        k_TypeNames.Insert(0, name);
                        break;
                    }
                    name = declaringType.GetNameWithFullGenericArguments();
                    k_TypeNames.Insert(0, name);
                    declaringType = parentDeclaringType;
                }

                var result = string.Join(".", k_TypeNames.ToArray());
                return result;
            }

            return type.GetFullNameWithGenericArgumentsInternal();
        }

        static string GetFullNameWithGenericArgumentsInternal(this Type type)
        {
            var name = type.FullName;

            if (!type.IsGenericType)
                return name;

            // Trim off `1
            name = name.Split('`')[0];

            var arguments = type.GetGenericArguments();
            var length = arguments.Length;
            var stringArguments = new string[length];
            for (var i = 0; i < length; i++)
            {
                stringArguments[i] = arguments[i].GetFullNameWithGenericArguments();
            }

            return $"{name}<{string.Join(", ", stringArguments)}>";
        }

        /// <summary>
        /// Tests if class type IsAssignableFrom or IsSubclassOf another type
        /// </summary>
        /// <param name="checkType">type wanting to check</param>
        /// <param name="baseType">type wanting to check against</param>
        /// <returns>True if IsAssignableFrom or IsSubclassOf</returns>
        public static bool IsAssignableFromOrSubclassOf(this Type checkType, Type baseType)
        {
            return checkType.IsAssignableFrom(baseType) || checkType.IsSubclassOf(baseType);
        }

        /// <summary>
        /// Searches this type and all base types for a method by name
        /// </summary>
        /// <param name="type">The type being searched</param>
        /// <param name="name">The name of the method for which to search</param>
        /// <param name="bindingAttr">BindingFlags passed to Type.GetMethod</param>
        /// <returns>MethodInfo for the first matching method found. Null if no method is found</returns>
        public static MethodInfo GetMethodRecursively(this Type type, string name, BindingFlags bindingAttr)
        {
            var method = type.GetMethod(name, bindingAttr);
            if (method != null)
                return method;

            var baseType = type.BaseType;
            if (baseType != null)
                method = type.BaseType.GetMethodRecursively(name, bindingAttr);

            return method;
        }
    }
}
