//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using LinqConnect template.
// Code is generated on: 1/20/2016 10:38:15
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
//------------------------------------------------------------------------------

using System;
using Devart.Data.Linq;
using Devart.Data.Linq.Mapping;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace DataContextCatalog
{

    [DatabaseAttribute(Name = "Data")]
    [ProviderAttribute(typeof(Devart.Data.SQLite.Linq.Provider.SQLiteDataProvider))]
    public partial class DataContextCatalog : Devart.Data.Linq.DataContext
    {
        public static CompiledQueryCache compiledQueryCache = CompiledQueryCache.RegisterDataContext(typeof(DataContextCatalog));
        private static MappingSource mappingSource = new Devart.Data.Linq.Mapping.AttributeMappingSource();

        #region Extensibility Method Definitions
    
        partial void OnCreated();
        partial void OnSubmitError(Devart.Data.Linq.SubmitErrorEventArgs args);

        partial void InsertPatient(Patient instance);
        partial void UpdatePatient(Patient instance);
        partial void DeletePatient(Patient instance);

        #endregion

        public DataContextCatalog() :
        base(GetConnectionString("DataContextCatalogConnectionString"), mappingSource)
        {
            OnCreated();
        }

        public DataContextCatalog(MappingSource mappingSource) :
        base(GetConnectionString("DataContextCatalogConnectionString"), mappingSource)
        {
            OnCreated();
        }

        private static string GetConnectionString(string connectionStringName)
        {
            System.Configuration.ConnectionStringSettings connectionStringSettings = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringSettings == null)
                throw new InvalidOperationException("Connection string \"" + connectionStringName +"\" could not be found in the configuration file.");
            return connectionStringSettings.ConnectionString;
        }

        public DataContextCatalog(string connection) :
            base(connection, mappingSource)
        {
          OnCreated();
        }

        public DataContextCatalog(System.Data.IDbConnection connection) :
            base(connection, mappingSource)
        {
          OnCreated();
        }

        public DataContextCatalog(string connection, MappingSource mappingSource) :
            base(connection, mappingSource)
        {
          OnCreated();
        }

        public DataContextCatalog(System.Data.IDbConnection connection, MappingSource mappingSource) :
            base(connection, mappingSource)
        {
          OnCreated();
        }

        public Devart.Data.Linq.Table<Patient> Patients
        {
            get
            {
                return this.GetTable<Patient>();
            }
        }
    }

    /// <summary>
    /// There are no comments for Patient in the schema.
    /// </summary>
    [Table(Name = @"Patients")]
    public partial class Patient : INotifyPropertyChanging, INotifyPropertyChanged    
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(System.String.Empty);

        private int _PatientId;

        private int _EpisodeStatus;

        private string _VisitKey;

        private string _DatabaseFile;

        private byte[] _PatientData;

        private long _Created;

        private long _LastMonitored;

        private long _LastUpdated;
    
        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(ChangeAction action);
        partial void OnCreated();
        partial void OnPatientIdChanging(int value);
        partial void OnPatientIdChanged();
        partial void OnEpisodeStatusChanging(int value);
        partial void OnEpisodeStatusChanged();
        partial void OnVisitKeyChanging(string value);
        partial void OnVisitKeyChanged();
        partial void OnDatabaseFileChanging(string value);
        partial void OnDatabaseFileChanged();
        partial void OnPatientDataChanging(byte[] value);
        partial void OnPatientDataChanged();
        partial void OnCreatedChanging(long value);
        partial void OnCreatedChanged();
        partial void OnLastMonitoredChanging(long value);
        partial void OnLastMonitoredChanged();
        partial void OnLastUpdatedChanging(long value);
        partial void OnLastUpdatedChanged();
        #endregion

        public Patient()
        {
            OnCreated();
        }

    
        /// <summary>
        /// Unique patient id, autogenerated sequential number
        /// </summary>
        [Column(Storage = "_PatientId", AutoSync = AutoSync.OnInsert, CanBeNull = false, DbType = "integer NOT NULL", IsDbGenerated = true, IsPrimaryKey = true)]
        public int PatientId
        {
            get
            {
                return this._PatientId;
            }
            set
            {
                if (this._PatientId != value)
                {
                    this.OnPatientIdChanging(value);
                    this.SendPropertyChanging();
                    this._PatientId = value;
                    this.SendPropertyChanged("PatientId");
                    this.OnPatientIdChanged();
                }
            }
        }

    
        /// <summary>
        /// Status of the episode
        /// </summary>
        [Column(Storage = "_EpisodeStatus", CanBeNull = false, DbType = "integer NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public int EpisodeStatus
        {
            get
            {
                return this._EpisodeStatus;
            }
            set
            {
                if (this._EpisodeStatus != value)
                {
                    this.OnEpisodeStatusChanging(value);
                    this.SendPropertyChanging();
                    this._EpisodeStatus = value;
                    this.SendPropertyChanged("EpisodeStatus");
                    this.OnEpisodeStatusChanged();
                }
            }
        }

    
        /// <summary>
        /// There are no comments for VisitKey in the schema.
        /// </summary>
        [Column(Storage = "_VisitKey", DbType = "text", UpdateCheck = UpdateCheck.Never)]
        public string VisitKey
        {
            get
            {
                return this._VisitKey;
            }
            set
            {
                if (this._VisitKey != value)
                {
                    this.OnVisitKeyChanging(value);
                    this.SendPropertyChanging();
                    this._VisitKey = value;
                    this.SendPropertyChanged("VisitKey");
                    this.OnVisitKeyChanged();
                }
            }
        }

    
        /// <summary>
        /// File name of the episode database that contains tracings, patterns and curve data
        /// </summary>
        [Column(Storage = "_DatabaseFile", DbType = "text", UpdateCheck = UpdateCheck.Never)]
        public string DatabaseFile
        {
            get
            {
                return this._DatabaseFile;
            }
            set
            {
                if (this._DatabaseFile != value)
                {
                    this.OnDatabaseFileChanging(value);
                    this.SendPropertyChanging();
                    this._DatabaseFile = value;
                    this.SendPropertyChanged("DatabaseFile");
                    this.OnDatabaseFileChanged();
                }
            }
        }

    
        /// <summary>
        /// Encrypted patient data
        /// </summary>
        [Column(Storage = "_PatientData", CanBeNull = false, DbType = "blob NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public byte[] PatientData
        {
            get
            {
                return this._PatientData;
            }
            set
            {
                if (this._PatientData != value)
                {
                    this.OnPatientDataChanging(value);
                    this.SendPropertyChanging();
                    this._PatientData = value;
                    this.SendPropertyChanged("PatientData");
                    this.OnPatientDataChanged();
                }
            }
        }

    
        /// <summary>
        /// Date and time when the Patient was created in Patterns
        /// </summary>
        [Column(Storage = "_Created", CanBeNull = false, DbType = "integer NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public long Created
        {
            get
            {
                return this._Created;
            }
            set
            {
                if (this._Created != value)
                {
                    this.OnCreatedChanging(value);
                    this.SendPropertyChanging();
                    this._Created = value;
                    this.SendPropertyChanged("Created");
                    this.OnCreatedChanged();
                }
            }
        }

    
        /// <summary>
        /// Date and time when the patient was seen in the external system for the last time
        /// </summary>
        [Column(Storage = "_LastMonitored", CanBeNull = false, DbType = "integer NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public long LastMonitored
        {
            get
            {
                return this._LastMonitored;
            }
            set
            {
                if (this._LastMonitored != value)
                {
                    this.OnLastMonitoredChanging(value);
                    this.SendPropertyChanging();
                    this._LastMonitored = value;
                    this.SendPropertyChanged("LastMonitored");
                    this.OnLastMonitoredChanged();
                }
            }
        }

    
        /// <summary>
        /// Date and time when the patient data was updated for the last time
        /// </summary>
        [Column(Storage = "_LastUpdated", CanBeNull = false, DbType = "integer NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public long LastUpdated
        {
            get
            {
                return this._LastUpdated;
            }
            set
            {
                if (this._LastUpdated != value)
                {
                    this.OnLastUpdatedChanging(value);
                    this.SendPropertyChanging();
                    this._LastUpdated = value;
                    this.SendPropertyChanged("LastUpdated");
                    this.OnLastUpdatedChanged();
                }
            }
        }
   
        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SendPropertyChanging()
        {
            if (this.PropertyChanging != null)
                this.PropertyChanging(this, emptyChangingEventArgs);
        }

        protected virtual void SendPropertyChanging(System.String propertyName) 
        {
            if (this.PropertyChanging != null)
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        protected virtual void SendPropertyChanged(System.String propertyName)
        {
             if (this.PropertyChanged != null)
                 this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
