using UnityEngine;

namespace Unity.XR.CoreUtils.Tests
{
    static class TestData
    {
        public static Vector2[] RandomVector2Array(int length, float range = 0.0001f)
        {
            var array = new Vector2[length];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new Vector2(Random.Range(-range, range), Random.Range(-range, range));
            }

            return array;
        }
        
        public static Vector3[] RandomXZVector3Array(int length, float range = 0.0001f)
        {
            var array = new Vector3[length];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
            }

            return array;
        }
    }
}
