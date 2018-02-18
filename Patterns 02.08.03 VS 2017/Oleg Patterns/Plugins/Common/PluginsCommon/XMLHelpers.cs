using PluginsAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PatternsPluginsCommon
{
    public static class XMLHelpers
    {
        public static List<PluginDetectionArtifact> DeserializeDetectedArtifacts(long absoluteStart, IEnumerable<XElement> artifactsList)
        {
            List<PluginDetectionArtifact> detectedObjects = new List<PluginDetectionArtifact>();
            foreach (XElement artifact in artifactsList)
            {
                String line = artifact.Attribute("data").Value;
                var toAdd = DATFileReader.ReadDAT(line, absoluteStart);
                detectedObjects.Add(toAdd);
            }

            return detectedObjects;
        }

        public static void CollectPatientData(XElement patient, out XElement artifacts, out XElement tracings, out XElement request, out int episodeID)
        {
            artifacts = patient.Element("artifacts");
            tracings = patient.Element("tracings");
            request = patient.Element("request");
            episodeID = request.Attribute2Int("id");
        }

        public static bool ValidatePatient(XElement patient)
        {
            String gaStr = patient.Attribute2String("ga");
            if (gaStr != null && gaStr != String.Empty)
                gaStr = gaStr.Substring(0, gaStr.IndexOf("+"));

            int ga;
            if (!Int32.TryParse(gaStr, out ga))
                ga = -1;

            int fetuses = patient.Attribute2Int("fetus");
            bool isValid = ga >= 36 && fetuses == 1;

            return isValid;
        }
    }
}
