using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Bounds related utilities
    /// </summary>
    public static class BoundsUtils
    {
        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<Renderer> k_Renderers = new List<Renderer>();
        static readonly List<Transform> k_Transforms = new List<Transform>();

        /// <summary>
        /// Get the aggregated bounds of a list of GameObjects and their children
        /// </summary>
        /// <param name="gameObjects">The list of GameObjects</param>
        /// <returns>The aggregated bounds</returns>
        public static Bounds GetBounds(List<GameObject> gameObjects)
        {
            Bounds? bounds = null;
            foreach (var gameObject in gameObjects)
            {
                var goBounds = GetBounds(gameObject.transform);
                if (!bounds.HasValue)
                {
                    bounds = goBounds;
                }
                else
                {
                    goBounds.Encapsulate(bounds.Value);
                    bounds = goBounds;
                }
            }

            return bounds ?? new Bounds();
        }

        /// <summary>
        /// Get the aggregated bounds of an array of transforms and their children
        /// </summary>
        /// <param name="transforms">The array of transforms</param>
        /// <returns>The aggregated bounds</returns>
        public static Bounds GetBounds(Transform[] transforms)
        {
            Bounds? bounds = null;
            foreach (var t in transforms)
            {
                var goBounds = GetBounds(t);
                if (!bounds.HasValue)
                {
                    bounds = goBounds;
                }
                else
                {
                    goBounds.Encapsulate(bounds.Value);
                    bounds = goBounds;
                }
            }
            return bounds ?? new Bounds();
        }

        /// <summary>
        /// Get the aggregated bounds of a transform and its children
        /// </summary>
        /// <param name="transform">The transform</param>
        /// <returns>The aggregated bounds</returns>
        public static Bounds GetBounds(Transform transform)
        {
            // Static collections used below are cleared by the methods that use them
            transform.GetComponentsInChildren(k_Renderers);
            var b = GetBounds(k_Renderers);

            // As a fallback when there are no bounds, collect all transform positions
            if (b.size == Vector3.zero)
            {
                transform.GetComponentsInChildren(k_Transforms);

                if (k_Transforms.Count > 0)
                    b.center = k_Transforms[0].position;

                foreach (var t in k_Transforms)
                {
                    b.Encapsulate(t.position);
                }
            }

            return b;
        }

        /// <summary>
        /// Get the aggregated bounds of a list of renderers
        /// </summary>
        /// <param name="renderers">The list of renderers</param>
        /// <returns>The aggregated bounds</returns>
        public static Bounds GetBounds(List<Renderer> renderers)
        {
            if (renderers.Count > 0)
            {
                var first = renderers[0];
                var b = new Bounds(first.transform.position, Vector3.zero);
                foreach (var r in renderers)
                {
                    if (r.bounds.size != Vector3.zero)
                        b.Encapsulate(r.bounds);
                }

                return b;
            }

            return default;
        }

#if INCLUDE_PHYSICS_MODULE
        /// <summary>
        /// Get the aggregated bounds of a list of colliders
        /// </summary>
        /// <param name="colliders">The list of colliders</param>
        /// <typeparam name="T">The type of object in the list of colliders</typeparam>
        /// <returns>The aggregated bounds</returns>
        public static Bounds GetBounds<T>(List<T> colliders) where T : Collider
        {
            if (colliders.Count > 0)
            {
                var first = colliders[0];
                var b = new Bounds(first.transform.position, Vector3.zero);
                foreach (var c in colliders)
                {
                    if (c.bounds.size != Vector3.zero)
                        b.Encapsulate(c.bounds);
                }

                return b;
            }

            return default;
        }
#endif

        /// <summary>
        /// Gets the bounds that encapsulate a list of points
        /// </summary>
        /// <param name="points">The list of points to encapsulate</param>
        /// <returns>The aggregated bounds</returns>
        public static Bounds GetBounds(List<Vector3> points)
        {
            var bounds = default(Bounds);
            if (points.Count < 1)
                return bounds;

            var minPoint = points[0];
            var maxPoint = minPoint;
            for (var i = 1; i < points.Count; ++i)
            {
                var point = points[i];
                if (point.x < minPoint.x)
                    minPoint.x = point.x;
                if (point.y < minPoint.y)
                    minPoint.y = point.y;
                if (point.z < minPoint.z)
                    minPoint.z = point.z;
                if (point.x > maxPoint.x)
                    maxPoint.x = point.x;
                if (point.y > maxPoint.y)
                    maxPoint.y = point.y;
                if (point.z > maxPoint.z)
                    maxPoint.z = point.z;
            }

            bounds.SetMinMax(minPoint, maxPoint);
            return bounds;
        }
    }
}
