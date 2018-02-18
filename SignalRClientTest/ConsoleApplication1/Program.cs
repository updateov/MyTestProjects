using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            HubConnection m_hubConnection;
            IHubProxy m_hubProxy;
            string m_changesHubName = "PatientsHub";

            string serviceURL = "http://localhost:8088";

            m_hubConnection = new HubConnection(serviceURL);
            m_hubProxy = m_hubConnection.CreateHubProxy(m_changesHubName);

            m_hubConnection.Start().Wait();

            var patients = m_hubProxy.Invoke<IEnumerable<Patient>>("GetActivePatients").Result;
        }
    }

        /// <summary>
    /// This is a patient information snapshot, with emphesis on documentation fields.
    /// </summary>
    [DataContract]
    public class Patient
    {
        [DataMember]
        public int ID { get; set; }
        
        [DataMember]
        public string MRN { get; set; }

        [DataMember]
        public PersonName Name { get; set; }

        [DataMember]
        public ChartStatuses ChartStatus { get; set; }

        [DataMember]
        public Location Location { get; set; }

        [DataMember]
        public Provider Provider { get; set; }

        /// <summary>
        /// Currently the only member of the details column that will be visible to the user.
        /// The rest of the details definition will come later.
        /// </summary>
        [DataMember]
        public string Comment { get; set; }

        /// <summary>
        /// Fields defined dynamically by devTool for the patient, in a key,value structure
        /// key should match the key as defined in the columns configuration - most likely the devTool value name
        /// </summary>
        [DataMember]
        public IDictionary<string, string> DynamicFields { get; set; }
    }

    [DataContract]
    public class PersonName
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string MiddleName { get; set; }
        [DataMember]
        public string Surname { get; set; }
    }

    [DataContract]
    public enum ChartStatuses
    {
        Closed = 0,
        /// <summary>
        /// Chart opened at another station
        /// </summary>
        Opened,
        /// <summary>
        /// Registered, but no chart was created, yet
        /// </summary>
        RegisteredNoChart
    }

    // TODO: Consider moving to another domain
    public class Location
    {
        public int ID { get; set; }
        public string Name { get; set; }
        // TODO: Not required for now
        public RemoteType? RemoteType { get; set; }
    }

    public enum RemoteType
    {
        Fixed = 0,
        Roaming = 1,
        CitrixFixed = 2
    }


    public class Provider
    {
        public int? ID { get; set; }
        public int? Type { get; set; }
        public PersonName Name { get; set; }
    }
}
