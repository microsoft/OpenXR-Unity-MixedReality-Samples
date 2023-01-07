using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.XR.CoreUtils.EditorTests
{
    class GeometryUtilsTests
    {
        const float k_PolygonsWithinRangeDistance = 2.5f;
        const float k_PolygonsWithinRangeDelta = 0.001f;
        const float k_PointOnBoundsSmallEpsilon = 1E-6f;
        const float k_PointOnBoundsLargeEpsilon = 1E-3f;

        static readonly List<Vector3> k_ExampleTri = new List<Vector3>
        {
            new Vector3(0f, 0f, 1f),
            new Vector3(1f, 0f, 0f),
            new Vector3(-1f, 0f, 0f)
        };

        static readonly List<Vector3> k_ExampleQuad = new List<Vector3>
        {
            new Vector3(1f, 0f, 1f),
            new Vector3(-1f, 0f, 1f),
            new Vector3(-1f, 0f, -1f),
            new Vector3(1f, 0f, -1f)
        };

        static readonly List<Vector3> k_ExampleHexagon = new List<Vector3>
        {
            new Vector3(4f, 0f, 4f),
            new Vector3(3f, 0f, 4f),
            new Vector3(2f, 0f, 5f),
            new Vector3(3f, 0f, 6f),
            new Vector3(4f, 0f, 6f),
            new Vector3(5f, 0f, 5f)
        };

        static readonly List<Vector3> k_ExampleHeptagon = new List<Vector3>
        {
            new Vector3(1f, 0f, -1f),
            new Vector3(0f, 0f, -2f),
            new Vector3(-1f, 0f, -1f),
            new Vector3(-2f, 0f, 0f),
            new Vector3(-1f, 0f, 1f),
            new Vector3(0f, 0f, 2f),
            new Vector3(1f, 0f, 1f)
        };

        static readonly List<Vector3> k_ExampleOctagon = new List<Vector3>
        {
            new Vector3(1f, 0f, -1f),
            new Vector3(0f, 0f, -1.5f),
            new Vector3(-1f, 0f, -1f),
            new Vector3(-1.5f, 0f, 0f),
            new Vector3(-1f, 0f, 1f),
            new Vector3(0f, 0f, 1.5f),
            new Vector3(1f, 0f, 1f),
            new Vector3(1.5f, 0f, 0f),
        };

        static readonly List<Vector3> k_ExampleIrregularHexagon = new List<Vector3>
        {
            new Vector3(5f, 0f, 5f),
            new Vector3(4f, 0f, 6f),
            new Vector3(3.5f, 0f, 6f),
            new Vector3(3.5f, 0f, -2f),
            new Vector3(5f, 0f, -1f),
            new Vector3(7f, 0f, 1f),
            new Vector3(6f, 0f, 4f)
        };

        static readonly List<Vector3> k_IrregularPolygon = new List<Vector3>
        {
            new Vector3(2.02f, 0f, 1.9f),
            new Vector3(1.1f, 0f, 0.95f),
            new Vector3(0f, 0f, 0.9f),
            new Vector3(0.1f, 0f, -2.96f),
            new Vector3(1.02f, 0f, -1.97f),
            new Vector3(2.01f, 0f, -0.92f),
            new Vector3(2.55f, 0f, -0.47f)
        };

        static readonly List<Vector3> k_ExamplePointsInConvexHull = new List<Vector3>
        {
            new Vector3(-11.2f, 0f, 0f),
            new Vector3(-19f, 0f, 11.1f),
            new Vector3(-8.6f, 0f, 6.1f),
            new Vector3(-8.1f, 0f, 16.9f),
            new Vector3(5.9f, 0f, -4.9f),
            new Vector3(1.2f, 0f, 25.3f),
            new Vector3(-12.8f, 0f, -12.7f),
            new Vector3(7.4f, 0f, 11.2f),
            new Vector3(36.1f, 0f, -23f),
            new Vector3(-4.5f, 0f, -15.8f),
            new Vector3(1.2f, 0f, 6.1f)
        };

        static readonly List<Vector3> k_EdgeCasePointsInConvexHull = new List<Vector3>
        {
            new Vector3(0f, 0f, 0.5f),
            new Vector3(0f, 0f, 1f),
            new Vector3(1f, 0f, 1f),
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 0f, 0f)
        };

        static readonly List<Vector3> k_ConvexHullRevisitPointsTest = new List<Vector3>
        {
            new Vector3(-0.05f, 0f, 1.45f),
            new Vector3(-0.05f, 0f, 3.05f),
            new Vector3(0.05f, 0f, 3.15f),
            new Vector3(2.75f, 0f, 4.25f),
            new Vector3(3.25f, 0f, 4.25f),
            new Vector3(3.45f, 0f, 4.15f),
            new Vector3(3.85f, 0f, 3.65f),
            new Vector3(3.55f, 0f, 0.35f),
            new Vector3(3.15f, 0f, -0.05f),
            new Vector3(0.75f, 0f, -0.05f),
            new Vector3(0.15f, 0f, 0.25f),
            new Vector3(0.15f, 0f, 0.35f),
            new Vector3(0.25f, 0f, 0.35f),
            new Vector3(0.45f, 0f, 0.25f),
            new Vector3(0.55f, 0f, 0.05f),
            new Vector3(0.55f, 0f, -0.05f),
            new Vector3(0.45f, 0f, -0.05f),
            new Vector3(0.25f, 0f, 0.15f)
        };

        static readonly List<int> k_ExampleConvexHullIndices = new List<int>
        {
            1,
            5,
            8,
            9,
            6
        };

        static readonly List<Vector3> k_IrregularConvexPolygon = new List<Vector3>
        {
            new Vector3(-0.2f, 0f, -0.2f),
            new Vector3(-0.2f, 0f, -0.1f),
            new Vector3(-0.1f, 0f, 0.1f),
            new Vector3(0f, 0f, 0.3f),
            new Vector3(0.1f, 0f, 0.3f),
            new Vector3(0.2f, 0f, 0.1f),
            new Vector3(0.2f, 0f, -0.2f)
        };

        static readonly Color k_HalfRed = new Color(1f, 0f, 0f, 0.5f);

        Vector3 m_Start;
        Vector3 m_End;

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<Vector3> k_ConvexHull = new List<Vector3>();
        static readonly List<Vector3> k_ReversedPolygon = new List<Vector3>();

        [SetUp]
        public void BeforeEach()
        {
            k_ConvexHull.Clear();
        }

        [Test]
        // in this test the closest approach is vertex-to-edge
        public void ClosestApproach_TestOne()
        {
            DebugDraw.Polygon(k_IrregularPolygon, Color.yellow, 5f);
            DebugDraw.Polygon(k_ExampleIrregularHexagon, Color.red, 5f);
            GeometryUtils.ClosestPolygonApproach(k_IrregularPolygon, k_ExampleIrregularHexagon, out m_Start, out m_End);
            Debug.DrawLine(m_Start, m_End, Color.green, 5f);
        }

        [Test]
        public void ClosestApproach_TestTwo()
        {
            DebugDraw.Polygon(k_IrregularPolygon, Color.yellow, 5f);
            DebugDraw.Polygon(k_ExampleHexagon, Color.red, 5f);
            GeometryUtils.ClosestPolygonApproach(k_IrregularPolygon, k_ExampleHexagon, out m_Start, out m_End);
            Debug.DrawLine(m_Start, m_End, Color.green, 5f);

            // in this test the closest approach is vertex-to-vertex
            Assert.AreEqual(k_IrregularPolygon[0], m_Start);
            Assert.AreEqual(k_ExampleHexagon[1], m_End);
        }

        [Test]
        public void ConvexHull2D_InsufficientPoints()
        {
            // Need at least 3 points to compute convex hull
            Assert.False(GeometryUtils.ConvexHull2D(new List<Vector3> { Vector3.zero, Vector3.zero }, k_ConvexHull));
        }

        [Test]
        public void ConvexHull2D_SufficientPoints()
        {
            Assert.True(GeometryUtils.ConvexHull2D(k_ExamplePointsInConvexHull, k_ConvexHull));
            Assert.AreEqual(k_ConvexHull.Count, k_ExampleConvexHullIndices.Count);
            for (var i = 0; i < k_ConvexHull.Count; ++i)
            {
                var expectedIndex = k_ExampleConvexHullIndices[i];
                Assert.AreEqual(k_ExamplePointsInConvexHull[expectedIndex], k_ConvexHull[i]);
            }
        }

        [Test]
        public void ConvexHull2D_CollinearLeftmostPoints()
        {
            // This convex hull algorithm traverses the outermost points of the given list, starting at the leftmost point.
            // If the chosen starting point lies in the middle of a line of leftmost points, it will end up being
            // skipped over during traversal and the algorithm will enter an infinite loop since it never returns
            // to the starting point. This tests that we do not pick such a point as a starting point.
            Assert.True(GeometryUtils.ConvexHull2D(k_EdgeCasePointsInConvexHull, k_ConvexHull));
        }

        [Test]
        public void ConvexHull2D_RevisitPoints()
        {
            // This tests that we do not get into an infinite loop by revisiting points that we've already added to the hull,
            // which can happen if a collinearity check fails due to floating point error.
            Assert.True(GeometryUtils.ConvexHull2D(k_ConvexHullRevisitPointsTest, k_ConvexHull));
        }

        [Test]
        public void PolygonCentroid2D_WithConvexHull()
        {
            // first get the convex hull as we do above
            Assert.True(GeometryUtils.ConvexHull2D(k_ExamplePointsInConvexHull, k_ConvexHull));

            var centroidPoint = GeometryUtils.PolygonCentroid2D(k_ConvexHull);

            DebugDraw.Polygon(k_ConvexHull, Color.blue, 5f);
            foreach (var hullVertex in k_ConvexHull)
            {
                Debug.DrawLine(centroidPoint, hullVertex, Color.green, 5f);
            }

            Assert.AreEqual(0f, centroidPoint.y);
            Assert.True(GeometryUtils.PointInPolygon(centroidPoint, k_ConvexHull));
        }

        [TestCaseSource(typeof(Polygon2DData), nameof(Polygon2DData.SimpleConvex))]
        public void PolygonCentroid2D_WithSimpleRegularPolygons(List<Vector3> vertices)
        {
            var centroidPoint = GeometryUtils.PolygonCentroid2D(vertices);

            DebugDraw.Polygon(vertices, Color.blue, 5f);
            foreach (var hullVertex in vertices)
            {
                Debug.DrawLine(centroidPoint, hullVertex, Color.green, 5f);
            }

            Assert.AreEqual(0f, centroidPoint.y);
            Assert.True(GeometryUtils.PointInPolygon(centroidPoint, vertices));
        }

        [Test]
        public void OrientedMinimumBoundingBox_WithConvexHull2D()
        {
            // first, get our convex hull from the set of points
            Assert.True(GeometryUtils.ConvexHull2D(k_ExamplePointsInConvexHull, k_ConvexHull));

            var boxPoints = new Vector3[4];
            var bounds2D = GeometryUtils.OrientedMinimumBoundingBox2D(k_ConvexHull, boxPoints);

            DebugDraw.Polygon(k_ConvexHull, k_HalfRed);
            DebugDraw.Polygon(boxPoints, Color.black);

            // did we properly calculate the size of the bounding box?
            // x field should be width (left to right distance)
            Assert.AreEqual(bounds2D.x, Vector3.Distance(boxPoints[0], boxPoints[3]));
            // y field should be height / depth (top to bottom distance)
            Assert.AreEqual(bounds2D.y, Vector3.Distance(boxPoints[0], boxPoints[1]));

            var boxPointsList = boxPoints.ToList();
            var pointsOnBounds = new List<Vector3>();
            foreach (var vertex in k_ConvexHull)
            {
                // This test uses a larger epsilon than the other OMBB tests since this polygon encompasses a larger area
                var onBounds = GeometryUtils.PointOnPolygonBoundsXZ(vertex, boxPointsList, k_PointOnBoundsLargeEpsilon);
                if (onBounds)
                    pointsOnBounds.Add(vertex);

                var inPolygon = GeometryUtils.PointInPolygon(vertex, boxPointsList);

                // If this is bounding box then all points in the hull must either lie inside the box or lie on its bounds
                Assert.True(onBounds || inPolygon);
            }

            // Did at least two vertices test as being on the polygon bounds?
            // We test this because to truly be the minimum oriented bounding box,
            // we should have at least one edge aligned with the bounding box.
            Assert.GreaterOrEqual(pointsOnBounds.Count, 2);
        }

        [TestCaseSource(typeof(Polygon2DData), nameof(Polygon2DData.SimpleConvex))]
        public void OrientedMinimumBoundingBox_WithSimplePolygons(List<Vector3> vertices)
        {
            var boxPoints = new Vector3[4];
            GeometryUtils.OrientedMinimumBoundingBox2D(vertices, boxPoints);

            Random.InitState(0);
            DebugDraw.Polygon(vertices, Random.ColorHSV(), 2.5f);
            DebugDraw.Polygon(boxPoints, Color.black, 3f);

            var boxPointsList = boxPoints.ToList();
            var pointsOnBounds = new List<Vector3>();
            foreach (var vertex in vertices)
            {
                var onBounds = GeometryUtils.PointOnPolygonBoundsXZ(vertex, boxPointsList, k_PointOnBoundsSmallEpsilon);
                if (onBounds)
                    pointsOnBounds.Add(vertex);

                var inPolygon = GeometryUtils.PointInPolygon(vertex, boxPointsList);
                Assert.True(onBounds || inPolygon);
            }

            Assert.GreaterOrEqual(pointsOnBounds.Count, 2);
        }

        [Test]
        public void OrientedMinimumBoundingBox_WithIrregularPolygon()
        {
            var boxPoints = new Vector3[4];
            GeometryUtils.OrientedMinimumBoundingBox2D(k_IrregularConvexPolygon, boxPoints);

            Random.InitState(0);
            DebugDraw.Polygon(k_IrregularConvexPolygon, Random.ColorHSV(), 2.5f);
            DebugDraw.Polygon(boxPoints, Color.black, 3f);

            var boxPointsList = boxPoints.ToList();
            var pointsOnBounds = new List<Vector3>();
            foreach (var vertex in k_IrregularConvexPolygon)
            {
                var onBounds = GeometryUtils.PointOnPolygonBoundsXZ(vertex, boxPointsList, k_PointOnBoundsSmallEpsilon);
                if (onBounds)
                    pointsOnBounds.Add(vertex);

                var inPolygon = GeometryUtils.PointInPolygon(vertex, boxPointsList);
                Assert.True(onBounds || inPolygon);
            }

            Assert.GreaterOrEqual(pointsOnBounds.Count, 2);
        }

        [TestCaseSource(typeof(Polygon2DData), nameof(Polygon2DData.SimpleConvexAreas))]
        public void ConvexPolygonArea(List<Vector3> vertices, float expectedArea)
        {
            var area = GeometryUtils.ConvexPolygonArea(vertices);
            Assert.AreEqual(expectedArea, area, Mathf.Epsilon);

            // Area should be the same regardless of winding order
            k_ReversedPolygon.Clear();
            for (var i = vertices.Count - 1; i >= 0; i--)
            {
                k_ReversedPolygon.Add(vertices[i]);
            }

            var reversedArea = GeometryUtils.ConvexPolygonArea(k_ReversedPolygon);
            Assert.AreEqual(expectedArea, reversedArea, Mathf.Epsilon);
        }

        [Test]
        public void PolygonInPolygon()
        {
            Assert.True(GeometryUtils.PolygonInPolygon(k_ExampleTri, k_ExampleOctagon));
        }

        [Test]
        public void PolygonPartiallyInPolygon()
        {
            Assert.False(GeometryUtils.PolygonInPolygon(k_ExampleHexagon, k_ExampleIrregularHexagon));
        }

        [Test]
        public void PolygonOutsidePolygon()
        {
            Assert.False(GeometryUtils.PolygonInPolygon(k_ExampleHexagon, k_ExampleQuad));
        }

        [Test]
        public void PolygonsWithinRange_JustInRange()
        {
            const float distance = k_PolygonsWithinRangeDistance + k_PolygonsWithinRangeDelta;
            Assert.True(GeometryUtils.PolygonsWithinRange(k_ExampleQuad, k_ExampleIrregularHexagon, distance));
        }

        [Test]
        public void PolygonsWithinRange_JustOutOfRange()
        {
            const float distance = k_PolygonsWithinRangeDistance - k_PolygonsWithinRangeDelta;
            Assert.False(GeometryUtils.PolygonsWithinRange(k_ExampleQuad, k_ExampleIrregularHexagon, distance));
        }

        [Test]
        public void PolygonsWithinRange_InfiniteRange()
        {
            // Should be trivially within range if range is positive infinity
            Assert.True(GeometryUtils.PolygonsWithinRange(k_ExampleQuad, k_ExampleIrregularHexagon, float.PositiveInfinity));
        }

        [Test]
        public void PolygonsWithinRange_Overlapping()
        {
            // Distance should be zero if polygons overlap, so we can use a max distance of epsilon
            Assert.True(GeometryUtils.PolygonsWithinRange(k_ExampleHexagon, k_ExampleIrregularHexagon, Mathf.Epsilon));
        }

        [Test]
        public void PolygonsWithinRange_PolygonInsideOther()
        {
            Assert.True(GeometryUtils.PolygonsWithinRange(k_ExampleTri, k_ExampleOctagon, Mathf.Epsilon));
        }

        [Test]
        public void PointInPolygon_PointOutside()
        {
            Assert.False(GeometryUtils.PointInPolygon(Vector3.zero, k_ExampleIrregularHexagon));
        }

        [Test]
        public void PointInPolygon_PointInside()
        {
            Assert.True(GeometryUtils.PointInPolygon(new Vector3(4f, 0f, 1f), k_ExampleIrregularHexagon));
        }

        [Test]
        public void PointInPolygon_PointInsideAlignedWithLastVertex()
        {
            // Part of this algorithm involves checking when an edge crosses the given point's horizontal axis, so this
            // tests what happens when an inside point shares a horizontal axis with the algorithm's starting vertex
            // (which is the last vertex of the polygon).
            Assert.True(GeometryUtils.PointInPolygon(new Vector3(5f, 0f, 4f), k_ExampleIrregularHexagon));
        }

        [Test]
        public void PointInPolygon_PointOutsideToLeftOfTopVertex()
        {
            // Since the way this algorithm works is by testing how many "collisions" happen to the right of the point,
            // one thing we test for is what happens when a point is to the left of a top or bottom vertex,
            // since that "collision" should not count.
            Assert.False(GeometryUtils.PointInPolygon(Vector3.forward * 6f, k_ExampleIrregularHexagon));
        }

        [Test]
        public void PointInPolygon_PointOnVertex()
        {
            Assert.True(GeometryUtils.PointInPolygon(k_ExampleIrregularHexagon[1], k_ExampleIrregularHexagon));
        }

        [Test]
        public void PointInPolygon_PointOnEdge()
        {
            Assert.True(GeometryUtils.PointInPolygon(new Vector3(3.5f, 0f, 0f), k_ExampleIrregularHexagon));
        }

        static class Polygon2DData
        {
            static List<Vector3>[] SimplePolygons = { k_ExampleTri, k_ExampleHexagon, k_ExampleHeptagon };
            static readonly float[] k_SimplePolygonAreas = { 1f, 4f, 7f };

            public static IEnumerable SimpleConvex
            {
                get
                {
                    foreach (var polygon in SimplePolygons)
                    {
                        yield return new TestCaseData(polygon);
                    }
                }
            }

            public static IEnumerable SimpleConvexAreas
            {
                get
                {
                    var polygonsLength = SimplePolygons.Length;
                    for (var i = 0; i < polygonsLength; i++)
                    {
                        yield return new TestCaseData(SimplePolygons[i], k_SimplePolygonAreas[i]);
                    }
                }
            }
        }
    }
}
