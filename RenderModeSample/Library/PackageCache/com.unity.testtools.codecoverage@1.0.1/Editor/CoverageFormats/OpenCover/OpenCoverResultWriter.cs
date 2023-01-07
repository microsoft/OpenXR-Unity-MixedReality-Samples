using System.Xml.Serialization;
using System.IO;
using UnityEngine;
using OpenCover.Framework.Model;
using UnityEditor.TestTools.CodeCoverage.Utils;

namespace UnityEditor.TestTools.CodeCoverage.OpenCover
{
    internal class OpenCoverResultWriter : CoverageResultWriterBase<CoverageSession>
    {
        public OpenCoverResultWriter(CoverageSettings coverageSettings) : base(coverageSettings)
        {
        }

        public override void WriteCoverageSession()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CoverageSession));
            string fileFullPath = GetNextFullFilePath();
            using (TextWriter writer = new StreamWriter(fileFullPath))
            {
                serializer.Serialize(writer, CoverageSession);
            }

            ResultsLogger.Log(ResultID.Log_ResultsSaved, fileFullPath);

            base.WriteCoverageSession();
        }
    }
}
