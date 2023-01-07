using System;

using UnityEngine;

namespace UnityEditor.XR.Management
{
    internal class XRCustomLoaderUIManager
    {
        public static IXRCustomLoaderUI GetCustomLoaderUI(string loaderTypeName, BuildTargetGroup buildTargetGroup)
        {
            IXRCustomLoaderUI ret = null;

            var customLoaderTypes = TypeCache.GetTypesDerivedFrom(typeof(IXRCustomLoaderUI));
            foreach (var customLoader in customLoaderTypes)
            {
                var attribs = customLoader.GetCustomAttributes(typeof(XRCustomLoaderUIAttribute), true);
                foreach (var attrib in attribs)
                {
                    if (attrib is XRCustomLoaderUIAttribute)
                    {
                        var customUiAttrib = attrib as XRCustomLoaderUIAttribute;
                        if (String.Compare(loaderTypeName, customUiAttrib.loaderTypeName, true) == 0 &&
                            buildTargetGroup == customUiAttrib.buildTargetGroup)
                        {
                            if (ret != null)
                            {
                                Debug.Log($"Multiple custom ui renderers found for ({loaderTypeName}, {buildTargetGroup}). Defaulting to built-in rendering instead.");
                                return null;
                            }
                            ret = Activator.CreateInstance(customLoader) as IXRCustomLoaderUI;
                        }
                    }
                }
            }

            return ret;
        }
    }
}
