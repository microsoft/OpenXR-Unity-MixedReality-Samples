#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Helpers for drawing shapes for debugging purposes.
    /// </summary>
    public static class DebugDraw
    {
        /// <summary>
        /// Draws a line around a polygonal shape
        /// </summary>
        /// <param name="vertices">Polygon made of a series of adjacent points in world space</param>
        /// <param name="color">Color of the line</param>
        /// <param name="duration">How long the line should be visible for.</param>
        public static void Polygon(List<Vector3> vertices, Color color, float duration = 10f)
        {
            var vertexCount = vertices.Count;
            if (vertexCount < 2)
                return;

            var lengthMinusOne = vertexCount - 1;
            for (var i = 0; i < lengthMinusOne; i++)
            {
                var a = vertices[i];
                var b = vertices[i + 1];
                Debug.DrawLine(a, b, color, duration);
            }

            var last = vertices[lengthMinusOne];
            var first = vertices[0];
            Debug.DrawLine(last, first, color, duration);
        }

        /// <summary>
        /// Draws a line around a polygonal shape
        /// </summary>
        /// <param name="vertices">Polygon made of a series of adjacent points in world space</param>
        /// <param name="color">Color of the line</param>
        /// <param name="duration">How long the line should be visible for.</param>
        public static void Polygon(Vector3[] vertices, Color color, float duration = 10f)
        {
            var vertexCount = vertices.Length;
            if (vertexCount < 2)
                return;

            var lengthMinusOne = vertexCount - 1;
            for (var i = 0; i < lengthMinusOne; i++)
            {
                var a = vertices[i];
                var b = vertices[i + 1];
                Debug.DrawLine(a, b, color, duration);
            }

            var last = vertices[lengthMinusOne];
            var first = vertices[0];
            Debug.DrawLine(last, first, color, duration);
        }
    }
}
#endif
