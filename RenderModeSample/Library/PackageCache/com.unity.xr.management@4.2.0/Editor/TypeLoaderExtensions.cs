using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;
using UnityEngine.XR.Management;

namespace UnityEditor.XR.Management
{
    internal static class TypeLoaderExtensions
    {
        public static TypeCache.TypeCollection GetTypesWithInterface<T>(this Assembly asm)
        {
            return TypeCache.GetTypesDerivedFrom(typeof(T));
        }

        public static TypeCache.TypeCollection GetAllTypesWithInterface<T>()
        {
            return TypeCache.GetTypesDerivedFrom(typeof(T));
        }

        public static TypeCache.TypeCollection GetTypesWithAttribute<T>(this Assembly asm)
        {
            return TypeCache.GetTypesWithAttribute(typeof(T));
        }

        public static TypeCache.TypeCollection GetAllTypesWithAttribute<T>()
        {
            return TypeCache.GetTypesWithAttribute(typeof(T));
        }
    }
}
