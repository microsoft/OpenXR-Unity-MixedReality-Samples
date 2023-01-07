using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.Compilation;
using UnityEditor.TestTools.CodeCoverage.Utils;
using System.Collections.Generic;
using UnityEditor.TestTools.CodeCoverage.Analytics;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class AssemblyFiltering
    {
        public const string kDefaultExcludedAssemblies = "system*,mono*,nunit*,microsoft*,mscorlib*,roslyn*";
        public const string kUserAlias = "<user>";
        public const string kProjectAlias = "<project>";
        public const string kPackagesAlias = "<packages>";
        public const string kAllAlias = "<all>";

        public string includedAssemblies
        {
            get;
            private set;
        }

        public string excludedAssemblies
        {
            get;
            private set;
        }

        private Regex[] m_IncludeAssemblies;
        private Regex[] m_ExcludeAssemblies;

        public AssemblyFiltering()
        {
            m_IncludeAssemblies = new Regex[] { };
            m_ExcludeAssemblies = new Regex[] { };
        }

        private string[] RemoveDefaultExcludedAssemblies(string[] excludeAssemblyFilters)
        {
            string[] defaultAssemblies = kDefaultExcludedAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> excludedAssemblies = excludeAssemblyFilters.ToList<string>();

            foreach (string defaultAssembly in defaultAssemblies)
            {
                excludedAssemblies.Remove(defaultAssembly);
            }

            return excludedAssemblies.ToArray();
        }

        public void Parse(string includeAssemblies, string excludeAssemblies)
        {
            includedAssemblies = includeAssemblies;
            excludedAssemblies = excludeAssemblies;

            string[] includeAssemblyFilters = includeAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            string[] excludeAssemblyFilters = excludeAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();

            CoverageAnalytics.instance.CurrentCoverageEvent.includedAssemblies = includeAssemblyFilters;
            CoverageAnalytics.instance.CurrentCoverageEvent.excludedAssemblies = RemoveDefaultExcludedAssemblies(excludeAssemblyFilters);

            m_IncludeAssemblies = includeAssemblyFilters
                .Select(f => CreateFilterRegex(f))
                .ToArray();

            m_ExcludeAssemblies = excludeAssemblyFilters
                .Select(f => CreateFilterRegex(f))
                .ToArray();
        }

        public bool IsAssemblyIncluded(string name)
        {
            name = name.ToLowerInvariant();

            if (m_ExcludeAssemblies.Any(f => f.IsMatch(name)))
            {
                return false;
            }
            else
            {
                return m_IncludeAssemblies.Any(f => f.IsMatch(name));
            }
        }

        private static Assembly[] GetAllProjectAssemblies()
        {
            Assembly[] assemblies = CompilationPipeline.GetAssemblies();
            Array.Sort(assemblies, (x, y) => String.Compare(x.name, y.name));
            return assemblies;
        }

        public static string GetAllProjectAssembliesString()
        {
            Assembly[] assemblies = GetAllProjectAssemblies();

            string assembliesString = "";
            int assembliesLength = assemblies.Length;
            for (int i=0; i<assembliesLength; ++i)
            {
                assembliesString += assemblies[i].name;
                if (i < assembliesLength - 1)
                    assembliesString += ",";
            }

            return assembliesString;
        }

        public static string GetUserOnlyAssembliesString()
        {
            return GetStartsWithAssembliesString("Assets");
        }

        public static string GetPackagesOnlyAssembliesString()
        {
            return GetStartsWithAssembliesString("Packages");
        }

        private static string GetStartsWithAssembliesString(string startsWithStr)
        {
            Assembly[] assemblies = GetAllProjectAssemblies();
            List<string> foundAssemblies = new List<string>();

            string assembliesString = "";
            int assembliesLength = assemblies.Length;
            int i;
            for (i = 0; i < assembliesLength; ++i)
            {
                string name = assemblies[i].name;
                string[] sourceFiles = assemblies[i].sourceFiles;

                if (sourceFiles.Length > 0 &&
                    sourceFiles[0].StartsWith(startsWithStr, StringComparison.InvariantCultureIgnoreCase))
                {
                    foundAssemblies.Add(name);
                }
            }

            int foundAssembliesLength = foundAssemblies.Count;
            for (i = 0; i < foundAssembliesLength; ++i)
            {
                assembliesString += foundAssemblies[i];
                if (i < foundAssembliesLength - 1)
                    assembliesString += ",";
            }

            return assembliesString;
        }

        public static Regex CreateFilterRegex(string filter)
        {
            filter = filter.ToLowerInvariant();

            return new Regex(CoverageUtils.GlobToRegex(filter), RegexOptions.Compiled);
        }
    }
}
