using System;
using NUnit.Framework;
using Unity.XR.CoreUtils.Editor;

namespace Unity.XR.CoreUtils.EditorTests
{
    class PackageVersionTests
    {
        [Test]
        public void ParseStringPackageVersion()
        {
            // These should not throw
            var v1 = new PackageVersion("1.0.0");
            Assert.That(v1.IsPrerelease, Is.False);
            var v2 = new PackageVersion("1.0.0-preview");
            Assert.That(v2.IsPrerelease, Is.True);
            var v3 = new PackageVersion("1.0.0-preview.1");
            Assert.That(v3.IsPrerelease, Is.True);
            var v4 = new PackageVersion("1.0.0-pre.1");
            Assert.That(v4.IsPrerelease, Is.True);
            var v5 = new PackageVersion("1.0.0-abc.1");
            Assert.That(v5.IsPrerelease, Is.True);

            const string maxValue = "18446744073709551615";
            const string maxPlusOneValue = "18446744073709551616";
            Assert.DoesNotThrow(() => new PackageVersion($"{maxValue}.{maxValue}.{maxValue}"));
            Assert.Throws<OverflowException>(() => new PackageVersion($"{maxPlusOneValue}.0.0"));
            Assert.Throws<OverflowException>(() => new PackageVersion($"0.{maxPlusOneValue}.0"));
            Assert.Throws<OverflowException>(() => new PackageVersion($"0.0.{maxPlusOneValue}"));

            Assert.DoesNotThrow(() =>
            {
                var v = new PackageVersion("");
                Assert.That(v == default);
                Assert.That(v == null);
            });
        }

        [Test]
        [TestCase("0.0.4")]
        [TestCase("1.2.3")]
        [TestCase("10.20.30")]
        [TestCase("1.1.2-prerelease+meta")]
        [TestCase("1.1.2+meta")]
        [TestCase("1.1.2+meta-valid")]
        [TestCase("1.0.0-alpha")]
        [TestCase("1.0.0-beta")]
        [TestCase("1.0.0-alpha.beta")]
        [TestCase("1.0.0-alpha.beta.1")]
        [TestCase("1.0.0-alpha.1")]
        [TestCase("1.0.0-alpha0.valid")]
        [TestCase("1.0.0-alpha.0valid")]
        [TestCase("1.0.0-alpha-a.b-c-somethinglong+build.1-aef.1-its-okay")]
        [TestCase("1.0.0-rc.1+build.1")]
        [TestCase("2.0.0-rc.1+build.123")]
        [TestCase("1.2.3-beta")]
        [TestCase("10.2.3-DEV-SNAPSHOT")]
        [TestCase("1.2.3-SNAPSHOT-123")]
        [TestCase("1.0.0")]
        [TestCase("2.0.0")]
        [TestCase("1.1.7")]
        [TestCase("2.0.0+build.1848")]
        [TestCase("2.0.1-alpha.1227")]
        [TestCase("1.0.0-alpha+beta")]
        [TestCase("1.2.3----RC-SNAPSHOT.12.9.1--.12+788")]
        [TestCase("1.2.3----R-S.12.9.1--.12+meta")]
        [TestCase("1.2.3----RC-SNAPSHOT.12.9.1--.12")]
        [TestCase("1.0.0+0.build.1-rc.10000aaa-kk-0.1")]
        [TestCase("999999999999999999.999999999999999999.999999999999999999")]
        [TestCase("1.0.0-0A.is.legal")]
        [TestCase("18446744073709551615.18446744073709551615.18446744073709551615")]
        public void ValidPackageVersion(string version)
        {
            Assert.DoesNotThrow(() => new PackageVersion(version));
        }

        [Test]
        [TestCase("1")]
        [TestCase("1.2")]
        [TestCase("1.2.3-0123")]
        [TestCase("1.2.3-0123.0123")]
        [TestCase("1.1.2+.123")]
        [TestCase("+invalid")]
        [TestCase("-invalid")]
        [TestCase("-invalid+invalid")]
        [TestCase("-invalid.01")]
        [TestCase("alpha")]
        [TestCase("alpha.beta")]
        [TestCase("alpha.beta.1")]
        [TestCase("alpha.1")]
        [TestCase("alpha+beta")]
        [TestCase("alpha_beta")]
        [TestCase("alpha.")]
        [TestCase("alpha..")]
        [TestCase("beta")]
        [TestCase("1.0.0-alpha_beta")]
        [TestCase("-alpha.")]
        [TestCase("1.0.0-alpha..")]
        [TestCase("1.0.0-alpha..1")]
        [TestCase("1.0.0-alpha...1")]
        [TestCase("1.0.0-alpha....1")]
        [TestCase("1.0.0-alpha.....1")]
        [TestCase("1.0.0-alpha......1")]
        [TestCase("1.0.0-alpha.......1")]
        [TestCase("01.1.1")]
        [TestCase("1.01.1")]
        [TestCase("1.1.01")]
        [TestCase("1.2")]
        [TestCase("1.2.3.DEV")]
        [TestCase("1.2-SNAPSHOT")]
        [TestCase("1.2.31.2.3----RC-SNAPSHOT.12.09.1--..12+788")]
        [TestCase("1.2-RC-SNAPSHOT")]
        [TestCase("-1.0.3-gamma+b7718")]
        [TestCase("+justmeta")]
        [TestCase("9.8.7+meta+meta")]
        [TestCase("9.8.7-whatever+meta+meta")]
        [TestCase("18446744073709551615.18446744073709551615.18446744073709551615----RC-SNAPSHOT.12.09.1--------------------------------..12")]
        public void InvalidFormatPackageVersion(string version)
        {
            Assert.Throws<FormatException>(() => new PackageVersion(version));
        }

        [Test]
        [TestCase("18446744073709551616.18446744073709551616.18446744073709551616")]
        public void OverFlowFormatPackageVersion(string version)
        {
            Assert.Throws<OverflowException>(() => new PackageVersion(version));
        }

        [Test]
        public void ImplicitCastTest()
        {
            Assert.DoesNotThrow(() =>
            {
                PackageVersion v = "1.0.0";
                Assert.That(v.IsPrerelease, Is.False);
            });

            Assert.DoesNotThrow(() =>
            {
                PackageVersion v = string.Empty;
                Assert.That(v == default);
                Assert.That(v == null);
            });
        }

        [Test]
        [TestCase("1.0.0", "1.0.0")]
        [TestCase("2.0.0", "1.0.0")]
        public void HigherOrEqualComparisonTest(string higherVersion, string lowerVersion)
        {
            Assert.That(new PackageVersion("1.0.0") >= "1.0.0");
        }

        [Test]
        [TestCase("2.0.0", "1.0.0")]
        [TestCase("1.1.0", "1.0.0")]
        [TestCase("1.0.1", "1.0.0")]
        [TestCase("1.0.0-preview.1", "1.0.0-preview.0")]
        [TestCase("1.0.0-preview.2", "1.0.0-preview.1")]
        [TestCase("1.0.0-preview", "1.0.0-preview.1")]
        [TestCase("1.0.0", "1.0.0-preview.0")]
        public void HigherComparisonTest(string higherVersion, string lowerVersion)
        {
            Assert.That(new PackageVersion(higherVersion) > lowerVersion);
        }

        [Test]
        [TestCase("1.0.0", "2.0.0")]
        [TestCase("1.0.0", "1.1.0")]
        [TestCase("1.0.0", "1.0.1")]
        [TestCase("1.0.0-preview.0", "1.0.0-preview.1")]
        [TestCase("1.0.0-preview.1", "1.0.0-preview.2")]
        [TestCase("1.0.0-preview.0", "1.0.0")]
        public void LowerOrEqualComparisonTest(string lowerVersion, string higherVersion)
        {
            Assert.That(new PackageVersion(lowerVersion) <= higherVersion);
        }

        [Test]
        [TestCase("1.0.0", "2.0.0")]
        [TestCase("1.0.0", "1.1.0")]
        [TestCase("1.0.0", "1.0.1")]
        [TestCase("1.0.0-preview.0", "1.0.0-preview.1")]
        [TestCase("1.0.0-preview.1", "1.0.0-preview.2")]
        [TestCase("1.0.0-preview.1+build.123", "1.0.0-preview.1")]
        [TestCase("1.0.0-preview.1+build.123", "1.0.0-preview.1+build.456")]
        [TestCase("1.0.0-preview.1+build.456", "1.0.0-preview.2+build.123")]
        public void LowerComparisonTest(string lowerVersion, string higherVersion)
        {
            Assert.That(new PackageVersion(lowerVersion) < higherVersion);
        }

        [Test]
        [TestCase("1.3.4", ExpectedResult = false)]
        [TestCase("1.3.4-pre.1", ExpectedResult = true)]
        [TestCase("1.3.4-preview.1", ExpectedResult = true)]
        public bool CheckPreviewVersionTest(string version)
        {
            return new PackageVersion(version).IsPrerelease;
        }

        [Test]
        [TestCase("1.3.4", "1.3.1")]
        [TestCase("1.3.5", "1.3.4")]
        [TestCase("1.3.4-pre.1", "1.3.4")]
        [TestCase("1.3.1-pre.1", "1.3.1-pre.2")]
        [TestCase("1.4.1-pre.1", "1.4.1-pre.1")]
        public void ConversionTest(string higherVersion, string lowerVersion)
        {
            Assert.That(new PackageVersion(higherVersion).ToMajor() >= new PackageVersion(lowerVersion).ToMajor());
            Assert.That(new PackageVersion(higherVersion).ToMajorMinor() > new PackageVersion(lowerVersion).ToMajor());
            Assert.That(new PackageVersion(higherVersion).ToMajorMinor() >= new PackageVersion(lowerVersion).ToMajorMinor());
            Assert.That(new PackageVersion(higherVersion).ToMajorMinorPatch() > new PackageVersion(lowerVersion).ToMajorMinor());
        }

        [Test]
        public void GetPackageVersionTest()
        {
            Assert.That(PackageVersionUtility.IsPackageInstalled("com.unity.xr.core-utils"));
            Assert.That(!PackageVersionUtility.IsPackageInstalled("com.unity.super-flux-capacitor"));
            Assert.That(PackageVersionUtility.GetPackageVersion("com.unity.xr.core-utils") >= "1.0.0");
        }

        [Test]
        [TestCase("alpha", "", -1)]
        [TestCase("alpha", "beta", -1)]
        [TestCase("alpha.1", "alpha", -1)]
        [TestCase("alpha", "alpha", 0)]
        [TestCase("ABC.123", "ABC.567", -1)]
        [TestCase("123", "ABC", -1)]
        [TestCase("123.ABC", "123.XYZ", -1)]
        [TestCase("---RC-SNAPSHOT.12.9.1--.12", "---RC-SNAPSHOT.12.9.1--.13", -1)]
        [TestCase("---RC-SNAPSHOT.12.9.1--.12", "---RC-SNAPSHOT.12.9.1--.1", 1)]
        [TestCase("---RC-SNAPSHOT.12.9.3--.12", "---RC-SNAPSHOT.12.9.1--.12", 1)]
        public void EmptyOrNullSubVersionCompareTest(string lh, string rh, int result)
        {
            Assert.That(PackageVersionUtility.EmptyOrNullSubVersionCompare(lh, rh) == result);
        }
    }
}
