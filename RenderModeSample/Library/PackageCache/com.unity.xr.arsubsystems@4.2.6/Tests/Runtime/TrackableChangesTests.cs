using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.XR.ARSubsystems.Tests
{
    [TestFixture]
    class TrackableChangesTestFixture
    {
        static readonly Allocator[] k_Allocators = new[]
        {
            Allocator.Temp,
            Allocator.TempJob,
            Allocator.Persistent
        };

        static void ForEachAllocator(Action<Allocator> @delegate)
        {
            foreach (var allocator in k_Allocators)
            {
                @delegate(allocator);
            }
        }

        /// <summary>
        /// This is a simulation of Trackable data that was added in a later minor revision of the package.
        /// </summary>
        struct DataAddedInALaterVersion : IEquatable<DataAddedInALaterVersion>
        {
            public float aFloat;
            public double aDouble;
            public char aChar;
            public long aLong64;
            public byte aByte;

            const double feigenbaumConstant = 4.669201609102990671853203820466;
            const byte largestPrimeNumberLessThan255 = 251;
            const long carolPrime = 18014398241046527;

            public static DataAddedInALaterVersion defaultValue => new DataAddedInALaterVersion
            {
                aFloat = Mathf.PI,
                aDouble = feigenbaumConstant,
                aChar = 'A',
                aLong64 = carolPrime,
                aByte = largestPrimeNumberLessThan255
            };

            public bool Equals(DataAddedInALaterVersion other) =>
                aFloat.Equals(other.aFloat) &&
                aDouble.Equals(other.aDouble) &&
                aChar == other.aChar &&
                aLong64 == other.aLong64 &&
                aByte == other.aByte;

            public override bool Equals(object obj) => obj is DataAddedInALaterVersion other && Equals(other);

            // Required but not used
            public override int GetHashCode() => base.GetHashCode();
        }

        /// <summary>
        /// Simulates some data from an <see cref="ITrackable"/>, e.g., <see cref="XRAnchor"/> or <see cref="XRRaycast"/>
        /// </summary>
        struct SubsystemTrackableData : ITrackable
        {
            // Disable "Field 'field' is never assigned to, and will always have its default value 'value'"
            // In this test, SubsystemTrackableData is only created via a MemCpy.
            #pragma warning disable CS0649
            TrackableId m_TrackableId;
            public TrackableId trackableId => m_TrackableId;

            Pose m_Pose;
            public Pose pose => m_Pose;

            TrackingState m_TrackingState;
            public TrackingState trackingState => m_TrackingState;

            IntPtr m_NativePtr;
            public IntPtr nativePtr => m_NativePtr;

            public int anInt32;
            public bool aBool;
            public DataAddedInALaterVersion dataAddedInALaterVersion;
            #pragma warning restore CS0649

            public static SubsystemTrackableData defaultValue => new SubsystemTrackableData
            {
                m_Pose = Pose.identity,
                dataAddedInALaterVersion = DataAddedInALaterVersion.defaultValue
            };

            public unsafe ProviderTrackableData ToProviderTrackableData()
            {
                var result = default(ProviderTrackableData);
                UnsafeUtility.MemCpy(&result, UnsafeUtility.AddressOf(ref this), sizeof(ProviderTrackableData));
                return result;
            }
        }

        static unsafe TrackableId GenerateRandomId()
        {
            var guid = Guid.NewGuid();
            return *(TrackableId*)&guid;
        }

        static Pose GenerateRandomPose() => new Pose(Random.insideUnitSphere * 100, Random.rotationUniform);

        /// <summary>
        /// This simulates the data a particular provider (i.e., implementation) knows about. It matches the first
        /// part of <see cref="SubsystemTrackableData"/>, but is from an "earlier" version of the API, so doesn't
        /// know about the <see cref="DataAddedInALaterVersion"/>.
        /// </summary>
        struct ProviderTrackableData : IEquatable<ProviderTrackableData>
        {
            public TrackableId trackableId;
            public Pose pose;
            public TrackingState trackingState;
            public IntPtr nativePtr;
            public int anInt32;
            public bool aBool;

            public static ProviderTrackableData random => new ProviderTrackableData
            {
                trackableId = GenerateRandomId(),
                pose = GenerateRandomPose(),
                trackingState = (TrackingState)Random.Range(0, 2),
                nativePtr = new IntPtr(Random.Range(int.MinValue, int.MaxValue)),
                anInt32 = Random.Range(int.MinValue, int.MaxValue),
                aBool = Random.Range(0, 1) == 0,
            };

            public bool Equals(ProviderTrackableData other) =>
                trackableId.Equals(other.trackableId) &&
                pose.Equals(other.pose) &&
                trackingState == other.trackingState &&
                nativePtr == other.nativePtr &&
                anInt32 == other.anInt32 &&
                aBool == other.aBool;

            public override bool Equals(object obj) => obj is ProviderTrackableData other && Equals(other);

            // Required but not used
            public override int GetHashCode() => base.GetHashCode();
        }

        [Test]
        public unsafe void TestTrackableChangesCopiesPointers()
        {
            const int addedCount = 7;
            const int updatedCount = 11;
            const int removedCount = 13;
            var addedPtr = stackalloc ProviderTrackableData[addedCount];
            var updatedPtr = stackalloc ProviderTrackableData[updatedCount];
            var removedPtr = stackalloc TrackableId[removedCount];

            for (var i = 0; i < addedCount; i++)
            {
                addedPtr[i] = ProviderTrackableData.random;
            }

            for (var i = 0; i < updatedCount; i++)
            {
                updatedPtr[i] = ProviderTrackableData.random;
            }

            for (var i = 0; i < removedCount; i++)
            {
                removedPtr[i] = GenerateRandomId();
            }

            ForEachAllocator(allocator =>
            {
                using (var trackableChanges = new TrackableChanges<SubsystemTrackableData>(
                    addedPtr, addedCount,
                    updatedPtr, updatedCount,
                    removedPtr, removedCount,
                    SubsystemTrackableData.defaultValue,
                    UnsafeUtility.SizeOf<ProviderTrackableData>(), allocator))
                {
                    Assert.IsTrue(trackableChanges.isCreated);
                    Assert.AreEqual(addedCount, trackableChanges.added.Length);
                    Assert.AreEqual(updatedCount, trackableChanges.updated.Length);
                    Assert.AreEqual(removedCount, trackableChanges.removed.Length);

                    for (var i = 0; i < addedCount; i++)
                    {
                        var added = trackableChanges.added[i];
                        Assert.AreEqual(addedPtr[i], added.ToProviderTrackableData());
                        Assert.AreEqual(DataAddedInALaterVersion.defaultValue, added.dataAddedInALaterVersion);
                        Assert.AreEqual(0, UnsafeUtility.MemCmp(addedPtr + i, &added, sizeof(ProviderTrackableData)));
                    }

                    for (var i = 0; i < updatedCount; i++)
                    {
                        var updated = trackableChanges.updated[i];
                        Assert.AreEqual(updatedPtr[i], updated.ToProviderTrackableData());
                        Assert.AreEqual(DataAddedInALaterVersion.defaultValue, updated.dataAddedInALaterVersion);
                        Assert.AreEqual(0, UnsafeUtility.MemCmp(updatedPtr + i, &updated, sizeof(ProviderTrackableData)));
                    }

                    for (var i = 0; i < removedCount; i++)
                    {
                        var removed = trackableChanges.removed[i];
                        Assert.AreEqual(removedPtr[i], removed);
                        Assert.AreEqual(0, UnsafeUtility.MemCmp(removedPtr + i, &removed, sizeof(TrackableId)));
                    }
                }
            });
        }

        [Test]
        public unsafe void HandlesZero()
        {
            ForEachAllocator(allocator =>
            {
                using (var trackableChanges = new TrackableChanges<SubsystemTrackableData>(
                    null, 0,
                    null, 0,
                    null, 0,
                    SubsystemTrackableData.defaultValue,
                    UnsafeUtility.SizeOf<ProviderTrackableData>(), allocator))
                {
                    Assert.AreEqual(0, trackableChanges.added.Length);
                    Assert.AreEqual(0, trackableChanges.updated.Length);
                    Assert.AreEqual(0, trackableChanges.removed.Length);
                }

                using (var trackableChanges = new TrackableChanges<SubsystemTrackableData>(0, 0, 0, allocator))
                {
                    Assert.AreEqual(0, trackableChanges.added.Length);
                    Assert.AreEqual(0, trackableChanges.updated.Length);
                    Assert.AreEqual(0, trackableChanges.removed.Length);
                }
            });
        }

        [Test]
        public void IsCreatedIsFalse()
        {
            TrackableChanges<SubsystemTrackableData> changes = default;
            Assert.IsFalse(changes.isCreated);
        }

        [Test]
        public void UncreatedCanBeDisposed()
        {
            TrackableChanges<SubsystemTrackableData> changes = default;
            changes.Dispose();
        }

        [Test]
        public void CanBeDisposedMultipleTimes()
        {
            ForEachAllocator(allocator =>
            {
                var changes = new TrackableChanges<SubsystemTrackableData>(1, 1, 1, allocator);
                Assert.IsTrue(changes.isCreated);
                changes.Dispose();
                Assert.IsFalse(changes.isCreated);
                changes.Dispose();
            });
        }

        [Test]
        public void AnyCountCanBeZero()
        {
            ForEachAllocator(allocator =>
            {
                for (var added = 0; added <= 2; added++)
                {
                    for (var updated = 0; updated <= 2; updated++)
                    {
                        for (var removed = 0; removed <= 2; removed++)
                        {
                            using (new TrackableChanges<SubsystemTrackableData>(added, updated, removed, allocator))
                            {
                                // this area intentionally left blank
                            }
                        }
                    }
                }
            });
        }
    }
}
