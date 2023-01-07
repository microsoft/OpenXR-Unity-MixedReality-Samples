using System;
using NUnit.Framework;

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class DiagnosticReportTests
    {
        const string k_SectionOneTitle = "Section One";
        const string k_SectionTwoTitle = "Section Two";

        [SetUp]
        public void SetUp()
        {
            DiagnosticReport.StartReport();
        }

        [Test]
        public void GettingSectionReturnsValidHandle()
        {
            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            Assert.AreNotEqual(DiagnosticReport.k_NullSection, sectionOneHandle);

        }

        [Test]
        public void SameSectionTitleGivesSameSectionHandle()
        {
            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            var sectionOneHandleTwo = DiagnosticReport.GetSection(k_SectionOneTitle);
            Assert.AreEqual(sectionOneHandle, sectionOneHandleTwo);
        }

        [Test]
        public void DifferentSectionTitlesGiveDifferentSectionHandles()
        {
            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            var sectionTwoHandle = DiagnosticReport.GetSection(k_SectionTwoTitle);
            Assert.AreNotEqual(sectionOneHandle, sectionTwoHandle);
        }

        [Test]
        public void CheckSimpleReportGenerationIsCorrect()
        {
            const string k_ExpectedOutput = "==== Section One ====\n\n==== Last 20 Events ====\n";

            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            var report = DiagnosticReport.GenerateReport();
            Assert.IsFalse(String.IsNullOrEmpty(report));
            Assert.AreEqual(k_ExpectedOutput, report);
        }

        [Test]
        public void CheckGenerateReportWithEntries()
        {
            const string k_ExpectedOutput = @"==== Section One ====
Entry Header: Entry Body
Entry Header 2: Entry Body 2
Entry Header 3: Entry Body 3

==== Last 20 Events ====
";

            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header", "Entry Body");
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header 2", "Entry Body 2");
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header 3", "Entry Body 3");

            var report = DiagnosticReport.GenerateReport();
            Assert.AreEqual(k_ExpectedOutput, report);
        }

        [Test]
        public void CheckGenerateReportWithMultipleSectionsAndEntries()
        {
            const string k_ExpectedOutput = @"==== Section One ====
Entry Header: Entry Body
Entry Header 2: Entry Body 2
Entry Header 3: Entry Body 3

==== Section Two ====
Entry Header 4: Entry Body 4
Entry Header 5: Entry Body 5
Entry Header 6: Entry Body 6

==== Last 20 Events ====
";

            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header", "Entry Body");
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header 2", "Entry Body 2");
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header 3", "Entry Body 3");

            var sectionTwoHandle = DiagnosticReport.GetSection(k_SectionTwoTitle);
            DiagnosticReport.AddSectionEntry(sectionTwoHandle, "Entry Header 4", "Entry Body 4");
            DiagnosticReport.AddSectionEntry(sectionTwoHandle, "Entry Header 5", "Entry Body 5");
            DiagnosticReport.AddSectionEntry(sectionTwoHandle, "Entry Header 6", "Entry Body 6");

            var report = DiagnosticReport.GenerateReport();
            Assert.AreEqual(k_ExpectedOutput, report);
        }

        [Test]
        public void CheckGeneratedEventsAreReported()
        {
            const string k_ExpectedOutput = @"==== Last 20 Events ====
Event One: Event Body One
";

            DiagnosticReport.AddEventEntry("Event One", "Event Body One");

            var report = DiagnosticReport.GenerateReport();
            Assert.AreEqual(k_ExpectedOutput, report);

        }

        [Test]
        public void CheckGeneratedEventsOverTwentyAreReported()
        {
            const string k_ExpectedOutput = @"==== Last 20 Events ====
Event 11: Event Body 11
Event 12: Event Body 12
Event 13: Event Body 13
Event 14: Event Body 14
Event 15: Event Body 15
Event 16: Event Body 16
Event 17: Event Body 17
Event 18: Event Body 18
Event 19: Event Body 19
Event 20: Event Body 20
Event 21: Event Body 21
Event 22: Event Body 22
Event 23: Event Body 23
Event 24: Event Body 24
Event 25: Event Body 25
Event 26: Event Body 26
Event 27: Event Body 27
Event 28: Event Body 28
Event 29: Event Body 29
Event 30: Event Body 30
";

            for (int i = 0; i <= 30; i++)
            {
                DiagnosticReport.AddEventEntry($"Event {i}", $"Event Body {i}");
            }


            var report = DiagnosticReport.GenerateReport();
            Assert.AreEqual(k_ExpectedOutput, report);

        }

        [Test]
        public void CheckFullReport()
        {
            const string k_ExpectedOutput = @"==== Section One ====
Section One Entry One: Simple

==== Section Two ====
Section Two Entry One: Simple

Section Two Entry Two: (2)
    FOO=BAR
    BAZ=100

==== Last 20 Events ====
Event 11: Event Body 11
Event 12: Event Body 12
Event 13: Event Body 13
Event 14: Event Body 14
Event 15: Event Body 15
Event 16: Event Body 16
Event 17: Event Body 17
Event 18: Event Body 18
Event 19: Event Body 19
Event 20: Event Body 20
Event 21: Event Body 21
Event 22: Event Body 22
Event 23: Event Body 23
Event 24: Event Body 24
Event 25: Event Body 25
Event 26: Event Body 26
Event 27: Event Body 27
Event 28: Event Body 28
Event 29: Event Body 29
Event 30: Event Body 30
";

            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Section One Entry One", "Simple");

            for (int i = 0; i <= 30; i++)
            {
                DiagnosticReport.AddEventEntry($"Event {i}", $"Event Body {i}");
            }

            var sectionTwoHandle = DiagnosticReport.GetSection(k_SectionTwoTitle);
            DiagnosticReport.AddSectionEntry(sectionTwoHandle, "Section Two Entry One", "Simple");
            DiagnosticReport.AddSectionBreak(sectionTwoHandle);
            DiagnosticReport.AddSectionEntry(sectionTwoHandle, "Section Two Entry Two", @"(2)
    FOO=BAR
    BAZ=100
");

            var report = DiagnosticReport.GenerateReport();
            Debug.Log(report);
            Assert.AreEqual(k_ExpectedOutput, report);

        }

        [Test]
        public void SectionReportsStayInCreatedOrder()
        {
            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            var sectionTwoHandle = DiagnosticReport.GetSection(k_SectionTwoTitle);
            var reportOne = DiagnosticReport.GenerateReport();

            DiagnosticReport.StartReport();
            sectionTwoHandle = DiagnosticReport.GetSection(k_SectionTwoTitle);
            sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            var reportTwo = DiagnosticReport.GenerateReport();

            Assert.AreNotEqual(reportOne, reportTwo);

        }

    }
}