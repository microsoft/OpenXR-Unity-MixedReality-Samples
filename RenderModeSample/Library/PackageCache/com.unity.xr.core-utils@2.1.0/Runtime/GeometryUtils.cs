using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utility methods for common geometric operations
    /// </summary>
    public static class GeometryUtils
    {
        // Used in approximate equality checks
        const float k_TwoPi = Mathf.PI * 2f;

        // constants/cached constructions for Vector/UV operations
        static readonly Vector3 k_Up = Vector3.up;
        static readonly Vector3 k_Forward = Vector3.forward;
        static readonly Vector3 k_Zero = Vector3.zero;
        static readonly Quaternion k_VerticalCorrection = Quaternion.AngleAxis(180.0f, k_Up);
        const float k_MostlyVertical = 0.95f;

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<Vector3> k_HullEdgeDirections = new List<Vector3>();
        static readonly HashSet<int> k_HullIndices = new HashSet<int>();

        /// <summary>
        /// Finds the two closest adjacent vertices in a polygon, to a separate world space position
        /// </summary>
        /// <param name="vertices">An outline of a polygon defined by vertices, each one connected to the next</param>
        /// <param name="point">The position in space to find the two closest outline vertices to</param>
        /// <param name="vertexA">One vertex of the nearest edge</param>
        /// <param name="vertexB">The other vertex of the nearest edge</param>
        /// <returns>True if a nearest edge could be found</returns>
        public static bool FindClosestEdge(List<Vector3> vertices, Vector3 point,
            out Vector3 vertexA, out Vector3 vertexB)
        {
            var vertexCount = vertices.Count;
            if (vertexCount < 1)
            {
                vertexA = Vector3.zero;
                vertexB = Vector3.zero;
                return false;
            }

            var shortestSqrDistance = float.MaxValue;
            var closestVertA = Vector3.zero;
            var closestVertB = Vector3.zero;
            for (var i = 0; i < vertexCount; i++)
            {
                var vert = vertices[i];
                var nextVert = vertices[(i + 1) % vertices.Count];

                var closestPointOnEdge = ClosestPointOnLineSegment(point, vert, nextVert);
                var sqrDistanceToEdge = Vector3.SqrMagnitude(point - closestPointOnEdge);
                if (sqrDistanceToEdge < shortestSqrDistance)
                {
                    shortestSqrDistance = sqrDistanceToEdge;
                    closestVertA = vert;
                    closestVertB = nextVert;
                }
            }

            vertexA = closestVertA;
            vertexB = closestVertB;
            return true;
        }

        /// <summary>
        /// Finds the furthest intersection point on a polygon from a point in space
        /// </summary>
        /// <param name="vertices">An outline of a polygon defined by vertices, each one connected to the next</param>
        /// <param name="point">The position in world space to find the furthest intersection point </param>
        /// <returns>A world space position of a point on the polygon that is as far from the input point as possible</returns>
        public static Vector3 PointOnOppositeSideOfPolygon(List<Vector3> vertices, Vector3 point)
        {
            const float oppositeSideBufferScale = 100.0f;

            var vertexCount = vertices.Count;
            if (vertexCount < 3)
                return Vector3.zero;

            var a = vertices[0];
            var b = vertices[1];
            var c = vertices[2];
            var normal = Vector3.Cross(b - a, c - a).normalized;
            var center = Vector3.zero;
            foreach (var vertex in vertices)
            {
                center += vertex;
            }

            center *= 1f / vertexCount;
            var toPoint = Vector3.ProjectOnPlane(point - center, normal);

            var lengthMinusOne = vertexCount - 1;
            for (var i = 0; i < vertexCount; i++)
            {
                var vertexA = vertices[i];
                var aNeighbor = i == lengthMinusOne ? a : vertices[i + 1];
                var aLineVector = aNeighbor - vertexA;

                ClosestTimesOnTwoLines(vertexA, aLineVector, center, -toPoint * oppositeSideBufferScale, out var s, out var t);
                if (t >= 0 && s >= 0 && s <= 1)
                {
                    return vertexA + aLineVector * s;
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Given a number of perimeter vertices, generate a triangle buffer and add it to the given list
        /// The winding order is reversible. Example winding orders:
        /// Normal:   Reverse:
        /// 0, 1, 2,  0, 2, 1,
        /// 0, 2, 3,  0, 3, 2,
        /// 0, 3, 4,  0, 4, 3,
        /// --etc--
        /// </summary>
        /// <param name="indices">The list to which the triangle buffer will be added</param>
        /// <param name="vertCount">The number of perimeter vertices</param>
        /// <param name="reverse">(Optional) Whether to reverse the winding order of the vertices</param>
        public static void TriangulatePolygon(List<int> indices, int vertCount, bool reverse = false)
        {
            vertCount-= 2;
            indices.EnsureCapacity(vertCount * 3);
            if (reverse)
            {
                for (var i = 0; i < vertCount; i++)
                {
                    indices.Add(0);
                    indices.Add(i + 2);
                    indices.Add(i + 1);
                }
            }
            else
            {
                for (var i = 0; i < vertCount; i++)
                {
                    indices.Add(0);
                    indices.Add(i + 1);
                    indices.Add(i + 2);
                }
            }
        }

        /// <summary>
        /// Two trajectories which may or may not intersect have a time along each path which minimizes the distance
        /// between trajectories. This function finds those two times. The same logic applies to line segments, where
        /// the one point is the starting position, and the second point is the position at t = 1.
        /// </summary>
        /// <param name="positionA">Starting point of object a</param>
        /// <param name="velocityA">Velocity (direction and magnitude) of object a</param>
        /// <param name="positionB">Starting point of object b</param>
        /// <param name="velocityB">Velocity (direction and magnitude) of object b</param>
        /// <param name="s">The time along trajectory a</param>
        /// <param name="t">The time along trajectory b</param>
        /// <param name="parallelTest">(Optional) epsilon value for parallel lines test</param>
        /// <returns>False if the lines are parallel, otherwise true</returns>
        public static bool ClosestTimesOnTwoLines(Vector3 positionA, Vector3 velocityA, Vector3 positionB, Vector3 velocityB,
            out float s, out float t, double parallelTest = double.Epsilon)
        {
            // Cast dot products to doubles because parallel test can fail on some hardware (iOS)
            var a = (double)Vector3.Dot(velocityA, velocityA);
            var b = (double)Vector3.Dot(velocityA, velocityB);
            var e = (double)Vector3.Dot(velocityB, velocityB);

            var d = a * e - b * b;

            //lines are parallel
            if (Math.Abs(d) < parallelTest)
            {
                s = 0;
                t = 0;
                return false;
            }

            var r = positionA - positionB;
            var c = Vector3.Dot(velocityA, r);
            var f = Vector3.Dot(velocityB, r);

            s = (float)((b * f - c * e) / d);
            t = (float)((a * f - c * b) / d);

            return true;
        }

        /// <summary>
        /// Two trajectories which may or may not intersect have a time along each path which minimizes the distance
        /// between trajectories. This function finds those two times. The same logic applies to line segments, where
        /// the one point is the starting position, and the second point is the position at t = 1.
        /// This function ignores the y components.
        /// </summary>
        /// <param name="positionA">Starting point of object a</param>
        /// <param name="velocityA">Velocity (direction and magnitude) of object a</param>
        /// <param name="positionB">Starting point of object b</param>
        /// <param name="velocityB">Velocity (direction and magnitude) of object b</param>
        /// <param name="s">The time along trajectory a</param>
        /// <param name="t">The time along trajectory b</param>
        /// <param name="parallelTest">(Optional) epsilon value for parallel lines test</param>
        /// <returns>False if the lines are parallel, otherwise true</returns>
        public static bool ClosestTimesOnTwoLinesXZ(Vector3 positionA, Vector3 velocityA, Vector3 positionB, Vector3 velocityB,
            out float s, out float t, double parallelTest = double.Epsilon)
        {
            // Cast dot products to doubles because parallel test can fail on some hardware (iOS)
            var a = (double) (velocityA.x * velocityA.x + velocityA.z * velocityA.z);
            var b = (double) (velocityA.x * velocityB.x + velocityA.z * velocityB.z);
            var e = (double) (velocityB.x * velocityB.x + velocityB.z * velocityB.z);

            var d = a * e - b * b;

            //lines are parallel
            if (Math.Abs(d) < parallelTest)
            {
                s = 0;
                t = 0;
                return false;
            }

            var r = positionA - positionB;
            var c = velocityA.x * r.x + velocityA.z * r.z;
            var f = velocityB.x * r.x + velocityB.z * r.z;

            s = (float) ((b * f - c * e) / d);
            t = (float) ((a * f - c * b) / d);

            return true;
        }

        /// <summary>
        /// Finds the points along two line segments which are closest together
        /// </summary>
        /// <param name="a">Starting point of segment A</param>
        /// <param name="aLineVector">Vector from point a to the end point of segment A</param>
        /// <param name="b">Starting point of segment B</param>
        /// <param name="bLineVector">Vector from point b to the end point of segment B</param>
        /// <param name="resultA">The resulting point along segment A</param>
        /// <param name="resultB">The resulting point along segment B</param>
        /// <param name="parallelTest">(Optional) epsilon value for parallel lines test</param>
        /// <returns>True if the line segments are parallel, false otherwise</returns>
        public static bool ClosestPointsOnTwoLineSegments(Vector3 a, Vector3 aLineVector, Vector3 b, Vector3 bLineVector,
            out Vector3 resultA, out Vector3 resultB, double parallelTest = double.Epsilon)
        {
            var parallel = !ClosestTimesOnTwoLines(a, aLineVector, b, bLineVector,
                out var s, out var t, parallelTest);

            if (s > 0 && s <= 1 && t > 0 && t <= 1)
            {
                resultA = a + aLineVector * s;
                resultB = b + bLineVector * t;
            }
            else
            {
                // Edge cases (literally--we are checking each of the four endpoints against the opposite segment)
                var bNeighbor = b + bLineVector;
                var aNeighbor = a + aLineVector;
                var aOnB = ClosestPointOnLineSegment(a, b, bNeighbor);
                var aNeighborOnB = ClosestPointOnLineSegment(aNeighbor, b, bNeighbor);
                var minDist = Vector3.Distance(a, aOnB);
                resultA = a;
                resultB = aOnB;

                var nextDist = Vector3.Distance(aNeighbor, aNeighborOnB);
                if (nextDist < minDist)
                {
                    resultA = aNeighbor;
                    resultB = aNeighborOnB;
                    minDist = nextDist;
                }

                var bOnA = ClosestPointOnLineSegment(b, a, aNeighbor);
                nextDist = Vector3.Distance(b, bOnA);
                if (nextDist < minDist)
                {
                    resultA = bOnA;
                    resultB = b;
                    minDist = nextDist;
                }

                var bNeighborOnA = ClosestPointOnLineSegment(bNeighbor, a, aNeighbor);
                nextDist = Vector3.Distance(bNeighbor, bNeighborOnA);
                if (nextDist < minDist)
                {
                    resultA = bNeighborOnA;
                    resultB = bNeighbor;
                }

                if (parallel)
                {
                    if (Vector3.Dot(aLineVector, bLineVector) > 0)
                    {
                        t = Vector3.Dot(bNeighbor - a, aLineVector.normalized) * 0.5f;
                        var midA = a + aLineVector.normalized * t;
                        var midB = bNeighbor + bLineVector.normalized * -t;
                        if (t > 0 && t < aLineVector.magnitude)
                        {
                            resultA = midA;
                            resultB = midB;
                        }
                    }
                    else
                    {
                        t = Vector3.Dot(aNeighbor - bNeighbor, aLineVector.normalized) * 0.5f;
                        var midA = aNeighbor + aLineVector.normalized * -t;
                        var midB = bNeighbor + bLineVector.normalized * -t;
                        if (t > 0 && t < aLineVector.magnitude)
                        {
                            resultA = midA;
                            resultB = midB;
                        }
                    }
                }
            }

            return parallel;
        }

        /// <summary>
        /// Returns the closest point along a line segment to a given point
        /// </summary>
        /// <param name="point">The point to test against the line segment</param>
        /// <param name="a">The first point of the line segment</param>
        /// <param name="b">The second point of the line segment</param>
        /// <returns>The closest point along the line segment to <paramref name="point"/></returns>
        public static Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            var segment = b - a;
            var direction = segment.normalized;
            var projection = Vector3.Dot(point - a, direction);
            if (projection < 0)
                return a;

            if (projection*projection > segment.sqrMagnitude)
                return b;

            return a + projection * direction;
        }

        /// <summary>
        /// Find the closest points on the perimeter of a pair of polygons
        /// </summary>
        /// <param name="verticesA">The vertex list of polygon A</param>
        /// <param name="verticesB">The vertex list of polygon B</param>
        /// <param name="pointA">The point on polygon A's closest to an edge of polygon B</param>
        /// <param name="pointB">The point on polygon B's closest to an edge of polygon A</param>
        /// <param name="parallelTest">The minimum distance between closest approaches used to detect parallel line segments</param>
        public static void ClosestPolygonApproach(List<Vector3> verticesA, List<Vector3> verticesB,
            out Vector3 pointA, out Vector3 pointB, float parallelTest = 0f)
        {
            pointA = default;
            pointB = default;
            var closest = float.MaxValue;
            var aCount = verticesA.Count;
            var bCount = verticesB.Count;
            var aCountMinusOne = aCount - 1;
            var bCountMinusOne = bCount - 1;
            var firstVertexA = verticesA[0];
            var firstVertexB = verticesB[0];
            for (var i = 0; i < aCount; i++)
            {
                var vertexA = verticesA[i];
                var aNeighbor = i == aCountMinusOne ? firstVertexA : verticesA[i + 1];
                var aLineVector = aNeighbor - vertexA;

                for (var j = 0; j < bCount; j++)
                {
                    var vertexB = verticesB[j];
                    var bNeighbor = j == bCountMinusOne ? firstVertexB : verticesB[j + 1];
                    var bLineVector = bNeighbor - vertexB;

                    var parallel = ClosestPointsOnTwoLineSegments(vertexA, aLineVector, vertexB, bLineVector,
                        out var a, out var b, parallelTest);

                    var dist = Vector3.Distance(a, b);

                    if (parallel)
                    {
                        var delta = dist - closest;
                        if (delta < parallelTest)
                        {
                            closest = dist - parallelTest;
                            pointA = a;
                            pointB = b;
                        }
                    }
                    else if (dist < closest)
                    {
                        closest = dist;
                        pointA = a;
                        pointB = b;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a point is inside of a polygon on the XZ plane, the y value is not used
        /// </summary>
        /// <param name="testPoint">The point to test</param>
        /// <param name="vertices">The vertices that make up the bounds of the polygon</param>
        /// <returns>True if the point is inside the polygon, false otherwise</returns>
        public static bool PointInPolygon(Vector3 testPoint, List<Vector3> vertices)
        {
            // Sanity check - not enough bounds vertices = nothing to be inside of
            if (vertices.Count < 3)
                return false;

            // Check how many lines this test point collides with going in one direction
            // Odd = Inside, Even = Outside
            var collisions = 0;
            var vertexCounter = 0;
            var startPoint = vertices[vertices.Count - 1];

            // We recenter the test point around the origin to simplify the math a bit
            startPoint.x -= testPoint.x;
            startPoint.z -= testPoint.z;

            var currentSide = false;
            if (!MathUtility.ApproximatelyZero(startPoint.z))
            {
                currentSide = startPoint.z < 0f;
            }
            else
            {
                // We need a definitive side of the horizontal axis to start with (since we need to know when we
                // cross it), so we go backwards through the vertices until we find one that does not lie on the horizontal
                for (var i = vertices.Count - 2; i >= 0; --i)
                {
                    var vertZ = vertices[i].z;
                    vertZ -= testPoint.z;
                    if (!MathUtility.ApproximatelyZero(vertZ))
                    {
                        currentSide = vertZ < 0f;
                        break;
                    }
                }
            }

            while (vertexCounter < vertices.Count)
            {
                var endPoint = vertices[vertexCounter];
                endPoint.x -= testPoint.x;
                endPoint.z -= testPoint.z;

                var startToEnd = endPoint - startPoint;
                var edgeSqrMagnitude = startToEnd.sqrMagnitude;
                if (MathUtility.ApproximatelyZero(startToEnd.x * endPoint.z - startToEnd.z * endPoint.x) &&
                    startPoint.sqrMagnitude <= edgeSqrMagnitude && endPoint.sqrMagnitude <= edgeSqrMagnitude)
                {
                    // This line goes through the start point, which means the point is on an edge of the polygon
                    return true;
                }

                // Ignore lines that end at the horizontal axis
                if (!MathUtility.ApproximatelyZero(endPoint.z))
                {
                    var nextSide = endPoint.z < 0f;
                    if (nextSide != currentSide)
                    {
                        currentSide = nextSide;

                        // If we've crossed the horizontal, check if the origin is to the left of the line
                        if ((startPoint.x * endPoint.z - startPoint.z * endPoint.x) / -(startPoint.z - endPoint.z) > 0)
                            collisions++;
                    }
                }

                startPoint = endPoint;
                vertexCounter++;
            }

            return collisions % 2 > 0;
        }

        /// <summary>
        /// Determines if a point is inside of a convex polygon and lies on the surface
        /// </summary>
        /// <param name="testPoint">The point to test</param>
        /// <param name="vertices">The vertices that make up the bounds of the polygon, these should be convex and coplanar but can have any normal</param>
        /// <returns>True if the point is inside the polygon and coplanar, false otherwise</returns>
        public static bool PointInPolygon3D(Vector3 testPoint, List<Vector3> vertices)
        {
            // Not enough bounds vertices = nothing to be inside of
            if (vertices.Count < 3)
                return false;

            // Compute the sum of the angles between the test point and each pair of edge points
            double angleSum = 0;
            for (var vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
            {
                var toA = vertices[vertIndex] - testPoint;
                var toB = vertices[(vertIndex + 1) % vertices.Count] - testPoint;
                var sqrDistances = toA.sqrMagnitude * toB.sqrMagnitude; // Use sqrMagnitude, take sqrt of result later
                if (sqrDistances <= MathUtility.EpsilonScaled) // On a vertex
                {
                    return true;
                }

                double cosTheta = Vector3.Dot(toA, toB) / Mathf.Sqrt(sqrDistances);
                var angle = Math.Acos(cosTheta);
                angleSum += angle;
            }
            // The sum will only be 2*PI if the point is on the plane of the polygon and on the interior
            const float radiansCompareThreshold = 0.01f;
            return Mathf.Abs((float)angleSum - k_TwoPi) < radiansCompareThreshold;
        }


        /// <summary>
        /// Returns the closest point on a plane to another point
        /// </summary>
        /// <param name="planeNormal">The plane normal</param>
        /// <param name="planePoint">A point on the plane</param>
        /// <param name="point">The other point</param>
        /// <returns>The closest point on the plane to the other point</returns>
        public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
        {
            var distance = -Vector3.Dot(planeNormal.normalized, point - planePoint);
            return point + planeNormal.normalized * distance;
        }

        /// <summary>
        /// Finds the smallest convex polygon in the xz plane that contains <paramref name="points"/>
        /// Based on algorithm outlined in https://www.bitshiftprogrammer.com/2018/01/gift-wrapping-convex-hull-algorithm.html
        /// </summary>
        /// <param name="points">Points used to find the convex hull. The y values of these points are ignored.</param>
        /// <param name="hull">List that will be filled out with vertices that define a convex polygon</param>
        /// <returns>True if <paramref name="points"/> has at least 3 entries, false otherwise</returns>
        public static bool ConvexHull2D(List<Vector3> points, List<Vector3> hull)
        {
            if (points.Count < 3)
                return false;

            k_HullIndices.Clear();
            var pointsCount = points.Count;
            var leftmostPointIndex = 0;
            for (var i = 1; i < pointsCount; ++i)
            {
                var point = points[i];
                var pointX = point.x;
                var pointZ = point.z;
                var leftMost = points[leftmostPointIndex];
                var leftmostX = leftMost.x;
                var leftmostZ = leftMost.z;

                // As we traverse the outermost points, if we find 3 or more collinear points then we skip points that
                // fall in the middle. So if our starting point falls in the middle of a line, it will always be skipped
                // and our loop's end condition will never be met. So if there are multiple leftmost points, we want to
                // use the point that has the minimum Z.
                if (pointX < leftmostX || MathUtility.Approximately(pointX, leftmostX) && pointZ < leftmostZ)
                    leftmostPointIndex = i;
            }

            // Starting from the leftmost point, move clockwise along outermost points until we are back at the starting point.
            var currentIndex = leftmostPointIndex;
            do
            {
                var currentPoint = points[currentIndex];
                hull.Add(currentPoint);
                k_HullIndices.Add(currentIndex);

                // This loop is where we find the next outermost point (next point on the hull clockwise).
                // To do this we start with a point "p" which is an arbitrary entry in "points".
                // We iterate through each point "q" in "points". If "q" is to the left of the line from
                // the current point to "p", then "p" takes on the value of "q" and we continue iterating.
                // By the end of iteration, "p" will be the next point on the hull because no point is more to the left.
                var pIndex = 0;
                var p = points[pIndex];
                for (var qIndex = 1; qIndex < pointsCount; ++qIndex)
                {
                    if (qIndex == currentIndex)
                        continue;

                    // By explicitly ignoring points that are already on the hull, we prevent the possibility of an infinite loop.
                    // Without this check, a point could potentially be chosen again if a collinearity check results in a
                    // false negative due to floating point error.
                    if (k_HullIndices.Contains(qIndex) && qIndex != leftmostPointIndex)
                        continue;

                    var q = points[qIndex];
                    var currentToP = p - currentPoint;
                    var currentToQ = q - currentPoint;

                    // The y value of the cross product of (current -> p) and (current -> q) tells us where q is
                    // in relation to the line (current -> p).
                    // If y is zero, q is on the line.
                    var crossY = currentToP.z * currentToQ.x - currentToP.x * currentToQ.z;

                    // next few lines are an inlined ` MathUtility.ApproximatelyZero(crossY) `,
                    // because we sometimes call this equality check many thousands of times in a frame
                    var yIsNegative = crossY < 0f;
                    var absY = yIsNegative ? -crossY : crossY;
                    var approximatelyEqual = absY < MathUtility.EpsilonScaled;

                    if (approximatelyEqual)
                    {
                        // If current, p, and q are collinear, then we want p to be the point that is furthest from current.
                        if (Vector3.SqrMagnitude(currentPoint - p) < Vector3.SqrMagnitude(currentPoint - q))
                        {
                            pIndex = qIndex;
                            p = points[pIndex];
                        }
                    }
                    // If y is negative, q is to the left.
                    else if (yIsNegative)
                    {
                        pIndex = qIndex;
                        p = points[pIndex];
                    }
                }

                currentIndex = pIndex;
            } while (currentIndex != leftmostPointIndex);

            return true;
        }

        /// <summary>
        /// Given a list of vertices of a 2d convex polygon, find the centroid of the polygon.
        /// This implementation operates only on the X and Z axes
        /// </summary>
        /// <param name="vertices">The vertices of the 2D polygon</param>
        /// <returns>The centroid point for the polygon</returns>
        public static Vector3 PolygonCentroid2D(List<Vector3> vertices)
        {
            var vertexCount = vertices.Count;
            double partialSignedArea, signedArea = 0;
            double centroidX = 0, centroidZ = 0;
            double currentX, currentZ;
            double nextX, nextZ;

            int i;
            for (i = 0; i < vertexCount - 1; i++)
            {
                var vertex = vertices[i];
                currentX = vertex.x;
                currentZ = vertex.z;
                var nextVertex = vertices[i+1];
                nextX = nextVertex.x;
                nextZ = nextVertex.z;

                partialSignedArea = currentX * nextZ - nextX * currentZ;
                signedArea += partialSignedArea;

                centroidX += (currentX + nextX) * partialSignedArea;
                centroidZ += (currentZ + nextZ) * partialSignedArea;
            }

            // Do last vertex separately so we don't check indexes via modulo every iteration
            var vertexI = vertices[i];
            currentX = vertexI.x;
            currentZ = vertexI.z;
            var vertex0 = vertices[0];
            nextX = vertex0.x;
            nextZ = vertex0.z;

            partialSignedArea = currentX * nextZ - nextX * currentZ;
            signedArea += partialSignedArea;

            centroidX += (currentX + nextX) * partialSignedArea;
            centroidZ += (currentZ + nextZ) * partialSignedArea;

            signedArea *= 0.5;
            var signedAreaMultiple = 6.0 * signedArea;
            centroidX /= signedAreaMultiple;
            centroidZ /= signedAreaMultiple;

            return new Vector3((float)centroidX, 0f, (float)centroidZ);
        }

        /// <summary>
        /// Find the oriented minimum bounding box for a 2D convex hull.
        /// This implements the 'rotating calipers' algorithm and operates in linear time.
        /// Operates only on the X and Z axes of the input.
        /// </summary>
        /// <param name="convexHull">The list of all points in a 2D convex hull on the x and z axes, in a clockwise winding order</param>
        /// <param name="boundingBox">An array of length 4 to fill with the vertex positions of the bounding box,
        /// in the order { top left, bottom left, bottom right, top right }</param>
        /// <returns>The size of the bounding box on each axis. Y here maps to the Z axis</returns>
        public static Vector2 OrientedMinimumBoundingBox2D(List<Vector3> convexHull, Vector3[] boundingBox)
        {
            // Caliper lines start axis-aligned as shown before we orient
            //        top
            //      ^------>
            //      |      | right
            // left |      |
            //      <------V
            //       bottom

            var caliperLeft = new Vector3(0f, 0f, 1f);
            var caliperRight = new Vector3(0f, 0f, -1f);
            var caliperTop = new Vector3(1f, 0f, 0f);
            var caliperBottom = new Vector3(-1f, 0f, 0f);

            float xMin = float.MaxValue, yMin = float.MaxValue;
            float xMax = float.MinValue, yMax = float.MinValue;
            int leftIndex = 0, rightIndex = 0, topIndex = 0, bottomIndex = 0;

            // find the indices of the 'extreme points' in the hull to use as starting edge indices
            var vertexCount = convexHull.Count;
            for (var i = 0; i < vertexCount; i++)
            {
                var vertex = convexHull[i];
                var x = vertex.x;
                if (x < xMin)
                {
                    xMin = x;
                    leftIndex = i;
                }

                if (x > xMax)
                {
                    xMax = x;
                    rightIndex = i;
                }

                var z = vertex.z;
                if (z < yMin)
                {
                    yMin = z;
                    bottomIndex = i;
                }

                if (z > yMax)
                {
                    yMax = z;
                    topIndex = i;
                }
            }

            // compute & store the direction of every edge in the hull
            k_HullEdgeDirections.Clear();
            var lastVertexIndex = vertexCount - 1;
            for (var i = 0; i < lastVertexIndex; i++)
            {
                var edgeDirection = convexHull[i + 1] - convexHull[i];
                edgeDirection.Normalize();
                k_HullEdgeDirections.Add(edgeDirection);
            }

            // by doing the last vertex on its own, we can skip checking indices while iterating above
            var lastEdgeDirection = convexHull[0] - convexHull[lastVertexIndex];
            lastEdgeDirection.Normalize();
            k_HullEdgeDirections.Add(lastEdgeDirection);

            var bestOrientedBoundingBoxArea = double.MaxValue;
            // for every vertex in the hull, try aligning a caliper edge with an edge the vertex lies on
            for (var i = 0; i < vertexCount; i++)
            {
                var leftEdge = k_HullEdgeDirections[leftIndex];
                var rightEdge = k_HullEdgeDirections[rightIndex];
                var topEdge = k_HullEdgeDirections[topIndex];
                var bottomEdge = k_HullEdgeDirections[bottomIndex];

                // find the angles between our caliper lines and the polygon edges, by doing
                // ` arccosine(caliperEdge · hullEdge) ` for each pair of caliper edge & polygon edge
                var leftAngle = Math.Acos(caliperLeft.x * leftEdge.x + caliperLeft.z * leftEdge.z);
                var rightAngle = Math.Acos(caliperRight.x * rightEdge.x + caliperRight.z * rightEdge.z);
                var topAngle = Math.Acos(caliperTop.x * topEdge.x + caliperTop.z * topEdge.z);
                var bottomAngle = Math.Acos(caliperBottom.x * bottomEdge.x + caliperBottom.z * bottomEdge.z);

                // find smallest angle among the lines
                var smallestAngleIndex = 0;
                var smallestAngle = leftAngle;
                if (rightAngle < smallestAngle)
                {
                    smallestAngle = rightAngle;
                    smallestAngleIndex = 1;
                }

                if (topAngle < smallestAngle)
                {
                    smallestAngle = topAngle;
                    smallestAngleIndex = 2;
                }

                if (bottomAngle < smallestAngle)
                    smallestAngleIndex = 3;

                // based on which caliper edge had the smallest angle between it & the polygon, rotate our calipers
                // and recalculate corners
                Vector3 upperLeft, upperRight, bottomLeft, bottomRight;
                switch (smallestAngleIndex)
                {
                    // left
                    case 0:
                        RotateCalipers(leftEdge, convexHull, ref leftIndex, out topIndex, out rightIndex, out bottomIndex,
                            out caliperLeft, out caliperTop, out caliperRight, out caliperBottom,
                            out upperLeft, out upperRight, out bottomRight, out bottomLeft);
                        break;
                    // right
                    case 1:
                        RotateCalipers(rightEdge, convexHull, ref rightIndex, out bottomIndex, out leftIndex, out topIndex,
                            out caliperRight, out caliperBottom, out caliperLeft, out caliperTop,
                            out bottomRight, out bottomLeft, out upperLeft, out upperRight);
                        break;
                    // top
                    case 2:
                        RotateCalipers(topEdge, convexHull, ref topIndex, out rightIndex, out bottomIndex, out leftIndex,
                            out caliperTop, out caliperRight, out caliperBottom, out caliperLeft,
                            out upperRight, out bottomRight, out bottomLeft, out upperLeft);
                        break;
                    // bottom
                    default:
                        RotateCalipers(bottomEdge, convexHull, ref bottomIndex, out leftIndex, out topIndex, out rightIndex,
                            out caliperBottom, out caliperLeft, out caliperTop, out caliperRight,
                            out bottomLeft, out upperLeft, out upperRight, out bottomRight);
                        break;
                }

                // usually with rotating calipers, this comparison is talked about in terms of distance,
                // but since we just want to know which is bigger it works to use square magnitudes
                var sqrDistanceX = (upperLeft - upperRight).sqrMagnitude;
                var sqrDistanceZ = (upperLeft - bottomLeft).sqrMagnitude;
                var sqrDistanceProduct = sqrDistanceX * sqrDistanceZ;

                // if this is a smaller box than any we've found before, it's our new candidate
                if (sqrDistanceProduct < bestOrientedBoundingBoxArea)
                {
                    bestOrientedBoundingBoxArea = sqrDistanceProduct;
                    boundingBox[0] = bottomLeft;
                    boundingBox[1] = bottomRight;
                    boundingBox[2] = upperRight;
                    boundingBox[3] = upperLeft;
                }
            }

            // compute the size of the 2d bounds
            var topLeft = boundingBox[0];
            var leftRightDistance = Vector3.Distance(topLeft, boundingBox[3]);
            var topBottomDistance = Vector3.Distance(topLeft, boundingBox[1]);
            return new Vector2(leftRightDistance, topBottomDistance);
        }

        static void RotateCalipers(Vector3 alignEdge, List<Vector3> vertices,
            ref int indexA, out int indexB, out int indexC, out int indexD,
            out Vector3 caliperA, out Vector3 caliperB, out Vector3 caliperC, out Vector3 caliperD,
            out Vector3 caliperAEndCorner, out Vector3 caliperBEndCorner, out Vector3 caliperCEndCorner, out Vector3 caliperDEndCorner)
        {
            var vertexCount = vertices.Count;
            caliperA = alignEdge;
            caliperB = new Vector3(caliperA.z, 0f, -caliperA.x); // orthogonal
            caliperC = -caliperA; // opposite
            caliperD = -caliperB; // opposite orthogonal
            indexA = (indexA + 1) % vertexCount;

            // For each caliper, determine the polygon edge for the next caliper by testing intersection between the current caliper
            // and the opposite orthogonal from subsequent polygon vertices until we've found the maximum intersection point.
            var startA = vertices[indexA];
            indexB = indexA;
            var maxS = 0f;
            while (true)
            {
                var nextIndex = (indexB + 1) % vertexCount;
                ClosestTimesOnTwoLinesXZ(startA, caliperA, vertices[nextIndex], caliperD, out var s, out _);
                if (s <= maxS)
                    break;

                maxS = s;
                indexB = nextIndex;
            }

            caliperAEndCorner = startA + caliperA * maxS;
            var startB = vertices[indexB];
            indexC = indexB;
            maxS = 0f;
            while (true)
            {
                var nextIndex = (indexC + 1) % vertexCount;
                ClosestTimesOnTwoLinesXZ(startB, caliperB, vertices[nextIndex], caliperA, out var s, out _);
                if (s <= maxS)
                    break;

                maxS = s;
                indexC = nextIndex;
            }

            caliperBEndCorner = startB + caliperB * maxS;
            var startC = vertices[indexC];
            indexD = indexC;
            maxS = 0f;
            while (true)
            {
                var nextIndex = (indexD + 1) % vertexCount;
                ClosestTimesOnTwoLinesXZ(startC, caliperC, vertices[nextIndex], caliperB, out var s, out _);
                if (s <= maxS)
                    break;

                maxS = s;
                indexD = nextIndex;
            }

            caliperCEndCorner = startC + caliperC * maxS;

            // No need for any intersection tests for the last corner since we have all the other corners
            caliperDEndCorner = caliperCEndCorner + caliperAEndCorner - caliperBEndCorner;
        }

        /// <summary>
        /// Given a 2D bounding box's vertices, find the rotation of the box
        /// </summary>
        /// <param name="vertices">The 4 vertices of the bounding box, in the order
        /// { top left, bottom left, bottom right, top right }</param>
        /// <returns>The rotation of the box, with the horizontal side aligned to the x axis and the
        /// vertical side aligned to the z axis</returns>
        public static Quaternion RotationForBox(Vector3[] vertices)
        {
            var topLeft = vertices[0];
            var topRight = vertices[3];
            var leftToRight = topRight - topLeft;
            return Quaternion.FromToRotation(Vector3.right, leftToRight);
        }

        /// <summary>
        /// Finds the area of a convex polygon
        /// </summary>
        /// <param name="vertices">The vertices that make up the bounds of the polygon.
        /// These must be convex but can be in either winding order.</param>
        /// <returns>The area of the polygon</returns>
        public static float ConvexPolygonArea(List<Vector3> vertices)
        {
            var count = vertices.Count;
            if (count < 3)
                return 0f;

            var firstVertex = vertices[0];
            var lastIndex = count - 1;
            var lastVertex = vertices[lastIndex];
            var area = lastVertex.x * firstVertex.z - firstVertex.x * lastVertex.z;
            for (var i = 0; i < lastIndex; i++)
            {
                var currentVertex = vertices[i];
                var nextVertex = vertices[i + 1];
                area += currentVertex.x * nextVertex.z - nextVertex.x * currentVertex.z;
            }

            // Take absolute value because area is negative if vertices are clockwise
            return Math.Abs(area * 0.5f);
        }

        /// <summary>
        /// Determines if one polygon lies completely inside a coplanar polygon
        /// </summary>
        /// <param name="polygonA">The polygon to test for lying inside <paramref name="polygonB"/></param>
        /// <param name="polygonB">The polygon to test for containing <paramref name="polygonA"/>.
        /// Must be convex and coplanar with <paramref name="polygonA"/></param>
        /// <returns>True if <paramref name="polygonA"/> lies completely inside <paramref name="polygonB"/>, false otherwise</returns>
        public static bool PolygonInPolygon(List<Vector3> polygonA, List<Vector3> polygonB)
        {
            if (polygonA.Count < 1)
                return false;

            foreach (var vertex in polygonA)
            {
                if (!PointInPolygon3D(vertex, polygonB))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if two convex coplanar polygons are within a certain distance from each other.
        /// This includes the polygon perimeters as well as their interiors.
        /// </summary>
        /// <param name="polygonA">The first polygon to test. Must be convex and coplanar with <paramref name="polygonB"/></param>
        /// <param name="polygonB">The second polygon to test. Must be convex and coplanar with <paramref name="polygonA"/></param>
        /// <param name="maxDistance">The maximum distance allowed between the two polygons</param>
        /// <returns>True if the polygons are within the specified distance from each other, false otherwise</returns>
        public static bool PolygonsWithinRange(List<Vector3> polygonA, List<Vector3> polygonB, float maxDistance)
        {
            return PolygonsWithinSqRange(polygonA, polygonB, maxDistance * maxDistance);
        }

        /// <summary>
        /// Determines if two convex coplanar polygons are within a certain distance from each other.
        /// This includes the polygon perimeters as well as their interiors.
        /// </summary>
        /// <param name="polygonA">The first polygon to test. Must be convex and coplanar with <paramref name="polygonB"/></param>
        /// <param name="polygonB">The second polygon to test. Must be convex and coplanar with <paramref name="polygonA"/></param>
        /// <param name="maxSqDistance">The square of the maximum distance allowed between the two polygons</param>
        /// <returns>True if the polygons are within the specified distance from each other, false otherwise</returns>
        public static bool PolygonsWithinSqRange(List<Vector3> polygonA, List<Vector3> polygonB, float maxSqDistance)
        {
            ClosestPolygonApproach(polygonA, polygonB, out var pointA, out var pointB);
            return Vector3.SqrMagnitude(pointB - pointA) <= maxSqDistance ||
                PolygonInPolygon(polygonA, polygonB) || PolygonInPolygon(polygonB, polygonA);
        }

        /// <summary>
        /// Determines if a point lies on the bounds of a polygon, ignoring the y components
        /// </summary>
        /// <param name="testPoint">The point to test</param>
        /// <param name="vertices">The vertices that make up the bounds of the polygon</param>
        /// <param name="epsilon">Custom epsilon value used when testing if the point lies on an edge</param>
        /// <returns>True if the point lies on any edge of the polygon, false otherwise</returns>
        public static bool PointOnPolygonBoundsXZ(Vector3 testPoint, List<Vector3> vertices, float epsilon = float.Epsilon)
        {
            var verticesCount = vertices.Count;

            // No edge for the point to lie on
            if (verticesCount < 2)
                return false;

            var lastVertex = vertices[verticesCount - 1];
            foreach (var vertex in vertices)
            {
                if (PointOnLineSegmentXZ(testPoint, lastVertex, vertex, epsilon))
                    return true;

                lastVertex = vertex;
            }

            return false;
        }

        /// <summary>
        /// Determines if a point lies on a line segment, ignoring the y components
        /// </summary>
        /// <param name="testPoint">The point to test</param>
        /// <param name="lineStart">Starting point of the line segment</param>
        /// <param name="lineEnd">Ending point of the line segment</param>
        /// <param name="epsilon">Custom epsilon value used for comparison checks</param>
        /// <returns>True if the point lies on the line segment, false otherwise</returns>
        public static bool PointOnLineSegmentXZ(Vector3 testPoint, Vector3 lineStart, Vector3 lineEnd, float epsilon = float.Epsilon)
        {
            var startToEnd = lineEnd - lineStart;
            var startToTestPoint = testPoint - lineStart;
            var cross = startToEnd.z * startToTestPoint.x - startToEnd.x * startToTestPoint.z;
            var absCross = cross >= 0f ? cross : -cross;
            if (absCross >= epsilon)
                return false;

            var dot = startToEnd.x * startToTestPoint.x + startToEnd.z * startToTestPoint.z;
            var lineSqrMagnitude = startToEnd.x * startToEnd.x + startToEnd.z * startToEnd.z;
            return dot >= -epsilon && dot <= lineSqrMagnitude + epsilon;
        }

        static Quaternion NormalizeRotationKeepingUp(Quaternion rot)
        {
            var srcUp = (rot * k_Up).normalized;
            var isMostlyVertical = Mathf.Abs(srcUp.y) > k_MostlyVertical;

            Vector3 modFwd;

            if (isMostlyVertical)
            {
                modFwd = Vector3.Cross(k_Forward, srcUp);
            }
            else
            {
                var side = Vector3.Cross(srcUp, k_Up);
                modFwd = Vector3.Cross(srcUp, side);
            }

            return Quaternion.LookRotation(modFwd, srcUp);
        }

        /// <summary>
        /// Gets a corrected polygon uv pose from a given plane pose.
        /// </summary>
        /// <param name="pose">The source plane pose.</param>
        /// <returns>The rotation-corrected pose for calculating UVs</returns>
        public static Pose PolygonUVPoseFromPlanePose(Pose pose)
        {
            return new Pose(k_Zero, NormalizeRotationKeepingUp(pose.rotation));
        }

        /// <summary>
        /// Takes a Polygon UV coordinate, and produces a pose-corrected UV coordinate.
        /// </summary>
        /// <param name="vertexPos">Vertex to transform</param>
        /// <param name="planePose">Polygon pose</param>
        /// <param name="uvPose">UV-correction Pose</param>
        /// <returns>The corrected UV coordinate.</returns>
        public static Vector2 PolygonVertexToUV(Vector3 vertexPos, Pose planePose, Pose uvPose)
        {
            var worldPos = planePose.position + planePose.rotation * vertexPos;
            var localUv = Quaternion.Inverse(uvPose.rotation) * (worldPos - uvPose.position);

            localUv = k_VerticalCorrection * localUv;

            return new Vector2(localUv.x, localUv.z);
        }
    }
}
