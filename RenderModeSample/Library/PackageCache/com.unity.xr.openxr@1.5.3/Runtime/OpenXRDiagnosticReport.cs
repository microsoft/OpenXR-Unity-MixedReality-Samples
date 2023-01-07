using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR
{
    internal class DiagnosticReport
    {
        private const string LibraryName = "UnityOpenXR";
        public static readonly ulong k_NullSection = 0;

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_StartReport")]
        public static extern void StartReport();

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_GetSection", CharSet = CharSet.Ansi)]
        public static extern ulong GetSection(string sectionName);

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_AddSectionEntry", CharSet = CharSet.Ansi)]
        public static extern void AddSectionEntry(ulong sectionHandle, string sectionEntry, string sectionBody);

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_AddSectionBreak", CharSet = CharSet.Ansi)]
        public static extern void AddSectionBreak(ulong sectionHandle);

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_AddEventEntry", CharSet = CharSet.Ansi)]
        public static extern void AddEventEntry(string eventName, string eventData);

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_DumpReport")]
        private static extern void Internal_DumpReport();

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_DumpReportWithReason")]
        private static extern void Internal_DumpReport(string reason);

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_GenerateReport")]
        static extern IntPtr Internal_GenerateReport();

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_ReleaseReport")]
        static extern void Internal_ReleaseReport(IntPtr report);

        internal static string GenerateReport()
        {
            string ret = "";

            IntPtr buffer = Internal_GenerateReport();
            if (buffer != IntPtr.Zero)
            {
                ret = Marshal.PtrToStringAnsi(buffer);
                Internal_ReleaseReport(buffer);
                buffer = IntPtr.Zero;
            }
            return ret;
        }

        public static void DumpReport(string reason)
        {
            Internal_DumpReport(reason);
        }
    }
}