#if INCLUDE_UGUI
using UnityEngine;

using UnityEngine.UI;

namespace Unity.XR.CoreUtils.Tests
{
    /// <summary>
    /// This class exists to allow testing of the overload for MaterialUtils.GetMaterialClone that takes a Graphic-derived class
    /// </summary>
    [AddComponentMenu("")]
    class TestImage : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh) {}
    }
}
#endif
