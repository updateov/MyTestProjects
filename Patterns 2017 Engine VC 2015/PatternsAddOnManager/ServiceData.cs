using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;
using CommonLogger;

namespace PatternsAddOnManager
{
    public class ServiceStatus
    {
        public String ServiceVersion { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class About
    {
        public String AboutImage { get; set; }
    }

    public class Session
    {
        public String TokenId { get; set; }
        public String EngineHandle { get; set; }
        public int GestationalAge { get; set; }
    }

    [CollectionDataContract(Name = "Sessions")]
    public class SessionsList : List<Session>
    {
        private SessionsList()
        {
            var guidHndlr = GUIDHandler.Init();
            foreach (var item in guidHndlr.Guid2SessionData)
            {
                Add(new Session() 
                { 
                    TokenId = item.Key.ToString(), GestationalAge = item.Value.GestationalAge
                });
            }
        }

        public static SessionsList GetList()
        {
            AllSessions = new SessionsList();
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, Sessions List, Get List", "Created list of Tokens");
            return AllSessions;
        }

        private static SessionsList AllSessions = null;
    }
    public class Artifact
    {
        public String Category { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ArtifactType ArtifactData { get; set; }
    }

    [KnownType(typeof(Baseline))]
    [KnownType(typeof(Contraction))]
    [KnownType(typeof(Acceleration))]
    [KnownType(typeof(Deceleration))]
    public abstract class ArtifactType
    {
        public String Category { get; set; }
        public int Id { get; set; }
    }

    public class Baseline : ArtifactType
    {
        public double BaselineVariability { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }
    }

    public class Contraction : ArtifactType
    {
        public DateTime PeakTime { get; set; }
    }

    public class Acceleration : ArtifactType
    {
        public double Confidence { get; set; }
        public double Height { get; set; }
        public bool IsNonInterpretable { get; set; }
        public DateTime PeakTime { get; set; }
        public double PeakValue { get; set; }

        // gapped data in seconds
        public double Repair { get; set; } 
    }

    public class Deceleration : Acceleration
    {
        public DateTime? ContractionStart { get; set; }
        public String DecelerationCategory { get; set; }
        public bool HasSixtiesNonReassuringFeature { get; set; }
        public bool IsEarlyDeceleration { get; set; }
        public bool IsLateDeceleration { get; set; }
        public bool IsNonAssociatedDeceleration { get; set; }
        public bool IsVariableDeceleration { get; set; }
        public String NonReassuringFeatures { get; set; }
    }

    [CollectionDataContract(Name = "Artifacts")]
    public class ArtifactsList : List<Artifact>
    {
        private ArtifactsList()
        {
        }

        public static ArtifactsList GetList(PatternsSessionData session)
        {
            var AllArtifacts = new ArtifactsList() { Session = session };
            AllArtifacts.FillList();
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, Artifacts List, Get List", "Created list of Artifacts");
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, Artifacts List, Get List", "List size is " + AllArtifacts.Count.ToString());
            return AllArtifacts;
        }

        private void FillList()
        {
            var resultsConverter = new ResultsConverter();
            AddRange(resultsConverter.Convert(Session));
        }

        public static ArtifactsList GetEmptyList()
        {
            var AllArtifacts = new ArtifactsList();
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, Artifacts List, Get List", "Created empty list of Artifacts");
            return AllArtifacts;
        }

        public PatternsSessionData Session { get; private set; }
    }

    public class TracingData
    {
        public String Fhr { get; set; }
        public DateTime PreviousDetectededEndTime { get; set; }
        public DateTime StartTime { get; set; }
        public String Up { get; set; }
    }
}
