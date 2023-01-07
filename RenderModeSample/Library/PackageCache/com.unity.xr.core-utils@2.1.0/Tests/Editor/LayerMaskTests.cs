using NUnit.Framework;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    class LayerMaskTests
    {
        [Test]
        public void Contains()
        {
            for (var i = 0; i < 32; i++)
            {
                var mask = new LayerMask();

                Assert.IsFalse(mask.Contains(i), "New LayerMask contains layer {0}.", i);

                mask |= 1 << i;
                Assert.IsTrue(mask.Contains(i), "LayerMask with layer {0} set true does not contain layer {0}.", i);

                mask = ~0;
                mask &= ~(1 << i);
                Assert.IsFalse(mask.Contains(i), "LayerMask with layer {0} set false contains layer {0}.", i);
            }
        }

        [Test]
        public void ContainsEverything()
        {
            var mask = (LayerMask)~0;
            for (var i = 0; i < 32; i++)
            {
                Assert.IsTrue(mask.Contains(i), "Everything LayerMask does not contain layer {0}.", i);
            }
        }

        [Test]
        public void ContainsNothing()
        {
            var mask = (LayerMask)0;
            for (var i = 0; i < 32; i++)
            {
                Assert.IsFalse(mask.Contains(i), "Nothing LayerMask contains layer {0}.", i);
            }
        }
    }
}
