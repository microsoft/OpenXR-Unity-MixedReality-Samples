using System;
using UnityEngine;

namespace Unity.XR.CoreUtils.GUI
{
    /// <summary>
    /// Used with a special property drawer that can limit which enum options are displayed
    /// </summary>
    public sealed class EnumDisplayAttribute : PropertyAttribute
    {
        /// <summary>
        /// The names of the enum values used to initialize this attribute
        /// </summary>
        public string[] Names;

        /// <summary>
        /// The int values of the enum values used to initialize this attribute
        /// </summary>
        public int[] Values;

        /// <summary>
        /// Initialize a new EnumDisplayAttribute
        /// </summary>
        /// <param name="enumValues">The enum values which should be displayed</param>
        public EnumDisplayAttribute(params object[] enumValues)
        {
            Names = new string[enumValues.Length];
            Values = new int[enumValues.Length];

            var valueCounter = 0;
            while (valueCounter < Values.Length)
            {
                var asEnum = enumValues[valueCounter] as Enum;

                if (asEnum == null)
                {
                    Debug.LogError($"Non-enum passed into EnumDisplay Attribute: {enumValues[valueCounter]}");
                    continue;
                }

                Names[valueCounter] = asEnum.ToString();
                Values[valueCounter] = Convert.ToInt32(asEnum);
                valueCounter++;
            }
        }
    }
}
