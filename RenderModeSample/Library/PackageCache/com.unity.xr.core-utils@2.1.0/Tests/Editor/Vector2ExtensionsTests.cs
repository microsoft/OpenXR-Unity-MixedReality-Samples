using NUnit.Framework;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    class Vec2ExtensionsTests
    {
        [Test]
        public void Abs_NegativeValues_AreInverted()
        {
            Assert.AreEqual(new Vector2(2f, 1f), new Vector2(-2f, -1f).Abs());
        }

        [Test]
        public void Abs_PositiveValues_AreUnchanged()
        {
            Assert.AreEqual(new Vector2(2f, 1f), new Vector2(2f, 1f).Abs());
        }
    }
}
