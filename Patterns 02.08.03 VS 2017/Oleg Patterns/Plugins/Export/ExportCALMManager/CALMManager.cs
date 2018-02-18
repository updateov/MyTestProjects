//Review: 27/12/2015
using CALMConnector;
using CommonLogger;
using Export.Entities;
using Export.Entities.ExportControlConfig;
using Export.PluginDataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml.Serialization;

namespace Export.CALMManager
{
    public class CALMManager
    {
        #region Properties & Members

        private string m_configXMLPath = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + @"\ExportConfig.xml";
        private object m_lock = new object();
        private CALMConnection m_calmConnection = null;
        private Timer m_calmConnectionTimer;

        public List<CALMConceptInfo> ConceptsInfos = null;

        public ExportConfig ExportDataConfig { get; set; }
        public bool CALMEnabled { get; set; }

        #endregion

        #region Initialization & Singleton functionality

        private static CALMManager s_CALMManager = null;
        private static Object s_lockObject = new Object();

        private CALMManager()
        {
            if (CALMManagerSettings.Instance.CALMServicesEnabled)
            {
                CALMEnabled = true;
                ExportDataConfig = LoadFromConfigXML();

                InitCALMConnection();

                m_calmConnectionTimer = new Timer();
                m_calmConnectionTimer.Interval = CALMManagerSettings.Instance.CALMCheckConnectionInterval;
                m_calmConnectionTimer.Elapsed += OnCheckConnection;
                m_calmConnectionTimer.Enabled = true;
            }
            else
            {
                CALMEnabled = false;
                ConceptsInfos = GetHardcodedPeriWatchConcepts();
            }
        }

        private void InitCALMConnection()
        {
            if (CALMManagerSettings.Instance.CALMServicesEnabled)
            {
                m_calmConnection = CALMConnection.Instance;
                m_calmConnection.Start();

                ConceptsInfos = GetConceptsInfosInGroup(CALMManagerSettings.Instance.CALMExportGroup);
            }
            else
            {
                ConceptsInfos = GetHardcodedPeriWatchConcepts();
            }
            
        }

        public static CALMManager Instance
        {
            get
            {
                if (s_CALMManager == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_CALMManager == null)
                        {
                            s_CALMManager = new CALMManager();
                        }
                    }
                }

                return s_CALMManager;
            }
        }

        #endregion

        #region Event & Delegates

        private void OnCheckConnection(object sender, ElapsedEventArgs e)
        {
            if (CALMManagerSettings.Instance.CALMServicesEnabled)
            {
                lock (m_lock)
                {
                    if (m_calmConnection.IsConnected() == false)
                    {
                        m_calmConnectionTimer.Enabled = false;

                        InitCALMConnection();

                        if (m_calmConnection.IsConnected() == true)
                        {
                            Logger.WriteLogEntry(TraceEventType.Warning, "CALMManager", "Identified connection error. Succeeded to reconnect CALM Services");
                        }
                        else
                        {
                            Logger.WriteLogEntry(TraceEventType.Critical, "CALMManager", "Identified connection error. Failed to reconnect CALM Services");
                        }

                        m_calmConnectionTimer.Enabled = true;
                    }
                }
            }
        }

        #endregion

        public bool SaveInterval(string visitKey, Interval exportedInterval)
        {
            if (CALMManagerSettings.Instance.CALMServicesEnabled)
            {
                lock (m_lock)
                {
                    bool bSucc = false;
                    string errorString = String.Empty;

                    try
                    {
                        //login
                        bSucc = m_calmConnection.Login(exportedInterval.LoginName);

                        if (bSucc)
                        {
                            DateTime conceptTime = exportedInterval.EndTime.AddSeconds(1).ToLocalTime();
                            //save all concepts
                            foreach (var concept in exportedInterval.Concepts)
                            {
                                ExportEntity entity = ExportDataConfig.GetEntityById(concept.Id);

                                if (entity.ReadOnlyId != concept.Id)
                                {
                                    if (concept.Value != null && concept.Value.ToString() != String.Empty)
                                    {
                                        UpdateOtherUserInputConcept(concept);

                                        bSucc = m_calmConnection.AddEditObservation(visitKey, concept.Id, concept.OOC, concept.Value.ToString(), conceptTime);
                                    }
                                    else
                                    {
                                        // StrikeOut only in case if value was deleted
                                        if (concept.OriginalValue != null && concept.OriginalValue.ToString() != String.Empty)
                                        {
                                            //no need to check bSucc because Strikeout will fail only if observation did not exist
                                            m_calmConnection.StrikeOutObservation(visitKey, concept.Id, concept.OOC, conceptTime);
                                        }
                                    }
                                }

                                if (!bSucc)
                                {
                                    //cancel changes if event one concept fails to save
                                    m_calmConnection.CancelAllChanges(out errorString);
                                    break;
                                }
                            }
                        }

                        //commit all changes
                        if (bSucc)
                        {
                            bSucc = m_calmConnection.SaveAllChanges(out errorString);
                        }
                    }
                    catch (Exception ex)
                    {
                        //cancel changes if event one concept fails to save
                        m_calmConnection.CancelAllChanges(out errorString);
                        Logger.WriteLogEntry(TraceEventType.Critical, "CALMManager", "Failed to save concept", ex);
                    }
                    finally
                    {
                        //logout
                        bSucc = bSucc & m_calmConnection.Logout();
                    }

                    return bSucc;
                }
            }
            else
                return true;
        }

        private void UpdateOtherUserInputConcept(BaseConcept conceptToUpdate)
        {
            CALMConceptInfo concept = ConceptsInfos.FirstOrDefault(c => c.ConceptNo == conceptToUpdate.Id && c.SubConcepts != null);
            if (concept != null)
            {
                CALMConceptInfo otherUserInputConcept = concept.SubConcepts.FirstOrDefault(sc => sc.ConceptNo == -100451);
                if (otherUserInputConcept != null)
                {
                    string conceptValue = conceptToUpdate.Value.ToString();
                    if (!string.IsNullOrEmpty(conceptValue))
                        conceptToUpdate.Value = conceptValue.Replace(otherUserInputConcept.Caption, "OTHERUSERINPUT ");
                }
            }
        }

        //For test only!!!
        private ExportConfig LoadFromConfigXML()
        {
            ExportConfig data = null;

            if (File.Exists(m_configXMLPath) == true)
            {
                XmlSerializer formatter = new XmlSerializer(typeof(ExportConfig));

                using (FileStream fs = new FileStream(m_configXMLPath, FileMode.Open))
                {
                    data = (ExportConfig)formatter.Deserialize(fs);
                }

                data.RemoveNotVisible();
            }
   
            return data;
        }  

        private List<CALMConceptInfo> GetConceptsInfosInGroup(int groupNo)
        {
            List<CALMConceptInfo> conceptsInfos = new List<CALMConceptInfo>();
            List<int> conceptsIds = ExportDataConfig.GetConceptsFromConfig();

            List<ConceptNumberToColumnMappingModel> dbConcepts = new List<ConceptNumberToColumnMappingModel>();
            DBManager.Instance.CreateDB();
            bool bSucc = DBManager.Instance.GetConceptNumberToColumnMapping(ref dbConcepts);

            int orderNumberInGroup = 1;
            foreach (var conceptId in conceptsIds)
	        {                
                CALMConceptInfo conceptInfo = SafeGetConceptInfo(conceptId, dbConcepts);
                
                conceptInfo.OrderNumberInGroup = orderNumberInGroup;
                conceptInfo.ConceptType = CALMConceptType.Concept;

                if (conceptInfo.HasList)
                {
                    //conceptInfo.SubConcepts = new List<CALMConceptInfo>();
                    int listNo = LMS.Metadata.Utils.GetListNoFromConceptNo(conceptId);
                    LMS.Data.Concepts.List subConceptsIds = LMS.Metadata.Utils.GetListOfConcepts(listNo);
                    
                    for (int i = 0; i < subConceptsIds.Count; i++ )
                    {
                        CALMConceptInfo subConceptInfo = new CALMConceptInfo();
                        
                        int subConceptId = subConceptsIds.get_Item(i);                        
                        if (LMS.Metadata.ConceptDef.IsConcept(subConceptId))
                        {
                            subConceptInfo = SafeGetConceptInfo(subConceptId, dbConcepts);
                            subConceptInfo.ConceptType = CALMConceptType.SubConcept;
                        }
                        else
                        {
                            subConceptInfo.ConceptNo = subConceptId;
                            subConceptInfo.Caption = LMS.Metadata.Utils.GetStr(subConceptId);
                            subConceptInfo.MinValue = subConceptInfo.MaxValue = -1;                            
                            subConceptInfo.ConceptType = CALMConceptType.String;
                        }
                        
                        subConceptInfo.OrderNumberInGroup = i + 1;
                        
                        conceptInfo.SubConcepts.Add(subConceptInfo);
                    }                                            
                }

                conceptsInfos.Add(conceptInfo);
                orderNumberInGroup++;
	        }

            return conceptsInfos;
        }

        private List<CALMConceptInfo> GetHardcodedPeriWatchConcepts()
        {
            List<CALMConceptInfo> conceptsInfos = new List<CALMConceptInfo>();

            CALMConceptInfo conceptInfo1 = new CALMConceptInfo();
            conceptInfo1.AbreviatedCaption = "Time Range";            
            conceptInfo1.CALMValueType = CALMValueType.Integer;
            conceptInfo1.CapitalizedCaption = "Time Range";
            conceptInfo1.Caption = "Time Range";
            conceptInfo1.ConceptNo = -102100;
            conceptInfo1.ConceptType = CALMConceptType.Concept;
            conceptInfo1.DateTimeFormat = null;
            conceptInfo1.HasRange = true;
            conceptInfo1.IsCalculationSource = true;
            conceptInfo1.IsTextFormatNumerical = true;
            conceptInfo1.IsTimeSeriesConcept = true;
            conceptInfo1.MaxValue = 100.0;
            conceptInfo1.MinValue = 0.0;
            conceptInfo1.OrderNumberInGroup = 1;
            conceptInfo1.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo1);

            CALMConceptInfo conceptInfo2 = new CALMConceptInfo();
            conceptInfo2.AbreviatedCaption = "Contraction Interval Range";
            conceptInfo2.CALMValueType = CALMValueType.String;
            conceptInfo2.CapitalizedCaption = "Contraction Interval Range";
            conceptInfo2.Caption = "Contraction Interval Range";
            conceptInfo2.ConceptNo = -102101;
            conceptInfo2.ConceptType = CALMConceptType.Concept;
            conceptInfo2.DateTimeFormat = null;
            conceptInfo2.DecimalPlaces = 2;
            conceptInfo2.HasRange = true;
            conceptInfo2.IsCalculationSource = true;
            conceptInfo2.IsTextFormatNumerical = true;
            conceptInfo2.IsTimeSeriesConcept = true;
            conceptInfo2.MaxValue = 100.0;
            conceptInfo2.MinValue = 0.0;
            conceptInfo2.OrderNumberInGroup = 2;
            conceptInfo2.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo2);
            
            CALMConceptInfo conceptInfo3 = new CALMConceptInfo();
            conceptInfo3.AbreviatedCaption = "# of Contractions";
            conceptInfo3.CALMValueType = CALMValueType.Integer;
            conceptInfo3.CapitalizedCaption = "# Of Contractions";
            conceptInfo3.Caption = "# of Contractions";
            conceptInfo3.ConceptNo = -102102;
            conceptInfo3.ConceptType = CALMConceptType.Concept;
            conceptInfo3.HasRange = true;
            conceptInfo3.IsCalculationSource = true;
            conceptInfo3.IsTextFormatNumerical = true;
            conceptInfo3.IsTimeSeriesConcept = true;
            conceptInfo3.MaxValue = 100.0;
            conceptInfo3.MinValue = 0.0;
            conceptInfo3.OrderNumberInGroup = 3;
            conceptInfo3.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo3);

            CALMConceptInfo conceptInfo4 = new CALMConceptInfo();
            conceptInfo4.AbreviatedCaption = "# of Long Contractions";
            conceptInfo4.CALMValueType = CALMValueType.Integer;
            conceptInfo4.CapitalizedCaption = "# Of Long Contractions";
            conceptInfo4.Caption = "# of Long Contractions";
            conceptInfo4.ConceptNo = -102103;
            conceptInfo4.ConceptType = CALMConceptType.Concept;
            conceptInfo4.HasRange = true;
            conceptInfo4.IsCalculationSource = true;
            conceptInfo4.IsTextFormatNumerical = true;
            conceptInfo4.IsTimeSeriesConcept = true;
            conceptInfo4.MaxValue = 100.0;
            conceptInfo4.MinValue = 0.0;
            conceptInfo4.OrderNumberInGroup = 4;
            conceptInfo4.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo4);

            CALMConceptInfo conceptInfo5 = new CALMConceptInfo();
            conceptInfo5.AbreviatedCaption = "Montevideo Units (per 10 min)";
            conceptInfo5.CALMValueType = CALMValueType.Integer;
            conceptInfo5.CapitalizedCaption = "Montevideo Units (per 10 min)";
            conceptInfo5.Caption = "Montevideo Units (per 10 min)";
            conceptInfo5.ConceptNo = -101395;
            conceptInfo5.ConceptType = CALMConceptType.Concept;
            conceptInfo5.HasRange = true;
            conceptInfo5.IsCalculationSource = true;
            conceptInfo5.IsTextFormatNumerical = true;
            conceptInfo5.IsTimeSeriesConcept = true;
            conceptInfo5.MaxValue = 400.0;
            conceptInfo5.MinValue = 0.0;
            conceptInfo5.OrderNumberInGroup = 5;
            conceptInfo5.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo5);

            CALMConceptInfo conceptInfo6 = new CALMConceptInfo();
            conceptInfo6.AbreviatedCaption = "Baseline FHR";
            conceptInfo6.CALMValueType = CALMValueType.String;
            conceptInfo6.CapitalizedCaption = "Baseline FHR";
            conceptInfo6.Caption = "Baseline FHR";
            conceptInfo6.ConceptNo = -101385;
            conceptInfo6.ConceptType = CALMConceptType.Concept; 
            conceptInfo6.IsTimeSeriesConcept = true;
            conceptInfo6.HasRange = true;
            conceptInfo6.MaxValue = 0.0;
            conceptInfo6.MinValue = 0.0;
            conceptInfo6.ObjectOFCare = 1;
            conceptInfo6.OrderNumberInGroup = 6;
            conceptInfo6.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo6);


            CALMConceptInfo conceptInfo7 = new CALMConceptInfo();
            conceptInfo7.AbreviatedCaption = "Variability";
            conceptInfo7.CALMValueType = CALMValueType.String;
            conceptInfo7.CapitalizedCaption = "Variability";
            conceptInfo7.Caption = "Variability";
            conceptInfo7.ConceptNo = -101420;
            conceptInfo7.ConceptType = CALMConceptType.Concept;
            conceptInfo7.IsTimeSeriesConcept = true;
            conceptInfo7.HasRange = true;
            conceptInfo7.MinValue = 0.0;
            conceptInfo7.MaxValue = 0.0;
            conceptInfo7.ObjectOFCare = 1;
            conceptInfo7.OrderNumberInGroup = 7;
            conceptInfo7.TextFormatOverride = TextFormat.eAlpha;
            conceptInfo7.HasList = true;
            conceptInfo7.ResponseList = new List<string>();
                CALMConceptInfo subConceptInfo71 = new CALMConceptInfo();
                subConceptInfo71.AbreviatedCaption = null;
                subConceptInfo71.CALMValueType = CALMValueType.Abstract;
                subConceptInfo71.CapitalizedCaption = null;
                subConceptInfo71.Caption = "";
                subConceptInfo71.ConceptNo = 9900742;
                subConceptInfo71.ConceptType = CALMConceptType.String;
            
            subConceptInfo71.MaxValue = -1.0;
                subConceptInfo71.MinValue = -1.0;
                subConceptInfo71.OrderNumberInGroup = 1;
                subConceptInfo71.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo7.SubConcepts.Add(subConceptInfo71);
            conceptInfo7.ResponseList.Add("");

                CALMConceptInfo subConceptInfo72 = new CALMConceptInfo();
                subConceptInfo72.AbreviatedCaption = "Absent (0)";
                subConceptInfo72.CALMValueType = CALMValueType.Integer;
                subConceptInfo72.CapitalizedCaption = "Absent (0)";
                subConceptInfo72.Caption = "Absent (0)";
                subConceptInfo72.ConceptNo = 9901706;
                subConceptInfo72.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo72.HasRange = true;
                subConceptInfo72.IsTextFormatNumerical = true;
                subConceptInfo72.MaxValue = 0.0;
                subConceptInfo72.MinValue = 0.0;
                subConceptInfo72.OrderNumberInGroup = 2;
                subConceptInfo72.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo7.SubConcepts.Add(subConceptInfo72);
            conceptInfo7.ResponseList.Add("Absent (0)");

            CALMConceptInfo subConceptInfo73 = new CALMConceptInfo();
                subConceptInfo73.AbreviatedCaption = "Minimal (1-5)";
                subConceptInfo73.CALMValueType = CALMValueType.Integer;
                subConceptInfo73.CapitalizedCaption = "Minimal (1-5)";
                subConceptInfo73.Caption = "Minimal (1-5)";
                subConceptInfo73.ConceptNo = 9901705;
                subConceptInfo73.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo73.HasRange = true;
                subConceptInfo73.IsTextFormatNumerical = true;
                subConceptInfo73.MaxValue = 5.0;
                subConceptInfo73.MinValue = 1.0;
                subConceptInfo73.ObjectOFCare = 1;
                subConceptInfo73.OrderNumberInGroup = 3;
                subConceptInfo73.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo7.SubConcepts.Add(subConceptInfo73);
            conceptInfo7.ResponseList.Add("Minimal (1-5)");

            CALMConceptInfo subConceptInfo74 = new CALMConceptInfo();
                subConceptInfo74.AbreviatedCaption = "Moderate (6-25)";
                subConceptInfo74.CALMValueType = CALMValueType.Integer;
                subConceptInfo74.CapitalizedCaption = "Moderate (6-25)";
                subConceptInfo74.Caption = "Moderate (6-25)";
                subConceptInfo74.ConceptNo = 9901704;
                subConceptInfo74.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo74.HasRange = true;
                subConceptInfo74.IsTextFormatNumerical = true;
                subConceptInfo74.MaxValue = 25.0;
                subConceptInfo74.MinValue = 6.0;
                subConceptInfo74.ObjectOFCare = 1;
                subConceptInfo74.OrderNumberInGroup = 4;
                subConceptInfo74.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo7.SubConcepts.Add(subConceptInfo74);
            conceptInfo7.ResponseList.Add("Moderate (6-25)");

            CALMConceptInfo subConceptInfo75 = new CALMConceptInfo();
                subConceptInfo75.AbreviatedCaption = "Marked (>25)";
                subConceptInfo75.CALMValueType = CALMValueType.Integer;
                subConceptInfo75.CapitalizedCaption = "Marked (>25)";
                subConceptInfo75.Caption = "Marked (>25)";
                subConceptInfo75.ConceptNo = 9901703;
                subConceptInfo75.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo75.HasRange = true;
                subConceptInfo75.IsTextFormatNumerical = true;
                subConceptInfo75.MaxValue = 999.0;
                subConceptInfo75.MinValue = 26.0;
                subConceptInfo75.ObjectOFCare = 1;
                subConceptInfo75.OrderNumberInGroup = 5;
                subConceptInfo75.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo7.SubConcepts.Add(subConceptInfo75);
            conceptInfo7.ResponseList.Add("Marked (>25)");

            CALMConceptInfo subConceptInfo76 = new CALMConceptInfo();
                subConceptInfo76.AbreviatedCaption = null;
                subConceptInfo76.CALMValueType = CALMValueType.Abstract;
                subConceptInfo76.CapitalizedCaption = null;
                subConceptInfo76.Caption = "Sinusoidal";
                subConceptInfo76.ConceptNo = 9902516;
                subConceptInfo76.ConceptType = CALMConceptType.String;
            //subConceptInfo96.HasRange = true;
            subConceptInfo76.MaxValue = -1.0;
                subConceptInfo76.MinValue = -1.0;
                subConceptInfo76.OrderNumberInGroup = 6;
                subConceptInfo76.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo7.SubConcepts.Add(subConceptInfo76);
            conceptInfo7.ResponseList.Add("Sinusoidal");

            CALMConceptInfo subConceptInfo77 = new CALMConceptInfo();
                subConceptInfo77.AbreviatedCaption = null;
                subConceptInfo77.CALMValueType = CALMValueType.Abstract;
                subConceptInfo77.CapitalizedCaption = null;
                subConceptInfo77.Caption = "Unable to determine";
                subConceptInfo77.ConceptNo = 9901707;
                subConceptInfo77.ConceptType = CALMConceptType.String;
                subConceptInfo77.MaxValue = -1.0;
                subConceptInfo77.MinValue = -1.0;
                subConceptInfo77.OrderNumberInGroup = 7;
                subConceptInfo77.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo7.SubConcepts.Add(subConceptInfo77);
            conceptInfo7.ResponseList.Add("Unable to determine");

            conceptsInfos.Add(conceptInfo7);


            CALMConceptInfo conceptInfo8 = new CALMConceptInfo();
            conceptInfo8.AbreviatedCaption = "Accels";
            conceptInfo8.CALMValueType = CALMValueType.String;
            conceptInfo8.CapitalizedCaption = "Accels";
            conceptInfo8.Caption = "Accels";
            conceptInfo8.ConceptNo = -101417;
            conceptInfo8.ConceptType = CALMConceptType.Concept;
            conceptInfo8.IsTimeSeriesConcept = true;
            conceptInfo8.HasRange = true;
            conceptInfo8.MinValue = 0.0;
            conceptInfo8.MaxValue = 0.0;
            conceptInfo8.ObjectOFCare = 1;
            conceptInfo8.OrderNumberInGroup = 8;
            conceptInfo8.TextFormatOverride = TextFormat.eAlpha;
            conceptInfo8.HasList = true;
            conceptInfo8.ResponseList = new List<string>();
                CALMConceptInfo subConceptInfo81 = new CALMConceptInfo();
                subConceptInfo81.AbreviatedCaption = null;
                subConceptInfo81.CALMValueType = CALMValueType.Abstract;
                subConceptInfo81.CapitalizedCaption = null;
                subConceptInfo81.Caption = "";
                subConceptInfo81.ConceptNo = 9900742;
                subConceptInfo81.ConceptType = CALMConceptType.String;
                subConceptInfo81.MaxValue = -1.0;
                subConceptInfo81.MinValue = -1.0;
                subConceptInfo81.OrderNumberInGroup = 1;
                subConceptInfo81.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo8.SubConcepts.Add(subConceptInfo81);
            conceptInfo8.ResponseList.Add("");

                CALMConceptInfo subConceptInfo82 = new CALMConceptInfo();
                subConceptInfo82.AbreviatedCaption = "Yes";
                subConceptInfo82.CALMValueType = CALMValueType.Integer;
                subConceptInfo82.CapitalizedCaption = "Yes";
                subConceptInfo82.Caption = "Yes";
                subConceptInfo82.ConceptNo = 9900508;
                subConceptInfo82.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo82.HasRange = true;
                subConceptInfo82.IsTextFormatNumerical = true;
                subConceptInfo82.MaxValue = 100.0;
                subConceptInfo82.MinValue = 1.0;
                subConceptInfo82.ObjectOFCare = 1;
                subConceptInfo82.OrderNumberInGroup = 2;
                subConceptInfo82.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo8.SubConcepts.Add(subConceptInfo82);
            conceptInfo8.ResponseList.Add("Yes");

            CALMConceptInfo subConceptInfo83 = new CALMConceptInfo();
                subConceptInfo83.AbreviatedCaption = "No";
                subConceptInfo83.CALMValueType = CALMValueType.Integer;
                subConceptInfo83.CapitalizedCaption = "No";
                subConceptInfo83.Caption = "No";
                subConceptInfo83.ConceptNo = 9900509;
                subConceptInfo83.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo83.HasRange = true;
                subConceptInfo83.IsTextFormatNumerical = true;
                subConceptInfo83.MaxValue = 0.0;
                subConceptInfo83.MinValue = 0.0;
                subConceptInfo83.ObjectOFCare = 1;
                subConceptInfo83.OrderNumberInGroup = 3;
                subConceptInfo83.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo8.SubConcepts.Add(subConceptInfo83);
            conceptInfo8.ResponseList.Add("No");

            conceptsInfos.Add(conceptInfo8);

            CALMConceptInfo conceptInfo9 = new CALMConceptInfo();
            conceptInfo9.AbreviatedCaption = "Decels";
            conceptInfo9.CALMValueType = CALMValueType.String;
            conceptInfo9.CapitalizedCaption = "Decels";
            conceptInfo9.Caption = "Decels";
            conceptInfo9.ConceptNo = -101418;
            conceptInfo9.ConceptType = CALMConceptType.Concept;
            conceptInfo9.IsTimeSeriesConcept = true;
            conceptInfo9.HasRange = false;
            conceptInfo9.MinValue = 0.0;
            conceptInfo9.MaxValue = 0.0;
            conceptInfo9.ObjectOFCare = 1;
            conceptInfo9.OrderNumberInGroup = 9;
            conceptInfo9.TextFormatOverride = TextFormat.eAlpha;
            conceptInfo9.HasList = true;
            conceptInfo9.ResponseList = new List<string>();
                CALMConceptInfo subConceptInfo91 = new CALMConceptInfo();
                subConceptInfo91.AbreviatedCaption = "None";
                subConceptInfo91.CALMValueType = CALMValueType.Integer;
                subConceptInfo91.CapitalizedCaption = "None";
                subConceptInfo91.Caption = "None";
                subConceptInfo91.ConceptNo = 9900021;
                subConceptInfo91.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo91.HasRange = true;
                subConceptInfo91.IsTextFormatNumerical = true;
                subConceptInfo91.MaxValue = -1.0;
                subConceptInfo91.MinValue = -1.0;
                subConceptInfo91.ObjectOFCare = 1;

                subConceptInfo91.OrderNumberInGroup = 1;
                subConceptInfo91.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo9.SubConcepts.Add(subConceptInfo91);
                conceptInfo9.ResponseList.Add("None");

                CALMConceptInfo subConceptInfo92 = new CALMConceptInfo();
                subConceptInfo92.AbreviatedCaption = "Early";
                subConceptInfo92.CALMValueType = CALMValueType.Integer;
                subConceptInfo92.CapitalizedCaption = "Early";
                subConceptInfo92.Caption = "Early";
                subConceptInfo92.ConceptNo = 9901382;
                subConceptInfo92.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo92.HasRange = true;
                subConceptInfo92.IsTextFormatNumerical = true;
                subConceptInfo92.MaxValue = 0.0;
                subConceptInfo92.MinValue = 0.0;
                subConceptInfo92.ObjectOFCare = 1;
                subConceptInfo92.OrderNumberInGroup = 2;
                subConceptInfo92.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo9.SubConcepts.Add(subConceptInfo92);
                conceptInfo9.ResponseList.Add("Early");

                CALMConceptInfo subConceptInfo93 = new CALMConceptInfo();
                subConceptInfo93.AbreviatedCaption = "Variable";
                subConceptInfo93.CALMValueType = CALMValueType.Integer;
                subConceptInfo93.CapitalizedCaption = "Variable";
                subConceptInfo93.Caption = "Variable";
                subConceptInfo93.ConceptNo = 9900798;
                subConceptInfo93.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo93.HasRange = true;
                subConceptInfo93.IsTextFormatNumerical = true;
                subConceptInfo93.MaxValue = 0.0;
                subConceptInfo93.MinValue = 0.0;
                subConceptInfo93.ObjectOFCare = 1;
                subConceptInfo93.OrderNumberInGroup = 3;
                subConceptInfo93.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo9.SubConcepts.Add(subConceptInfo93);
                conceptInfo9.ResponseList.Add("Variable");

                CALMConceptInfo subConceptInfo94 = new CALMConceptInfo();
                subConceptInfo94.AbreviatedCaption = "Late";
                subConceptInfo94.CALMValueType = CALMValueType.Integer;
                subConceptInfo94.CapitalizedCaption = "Late";
                subConceptInfo94.Caption = "Late";
                subConceptInfo94.ConceptNo = 9901383;
                subConceptInfo94.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo94.HasRange = true;
                subConceptInfo94.IsTextFormatNumerical = true;
                subConceptInfo94.MaxValue = 0.0;
                subConceptInfo94.MinValue = 0.0;
                subConceptInfo94.ObjectOFCare = 1;
                subConceptInfo94.OrderNumberInGroup = 4;
                subConceptInfo94.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo9.SubConcepts.Add(subConceptInfo94);
                conceptInfo9.ResponseList.Add("Late");

                CALMConceptInfo subConceptInfo95 = new CALMConceptInfo();
                subConceptInfo95.AbreviatedCaption = "Prolonged";
                subConceptInfo95.CALMValueType = CALMValueType.Integer;
                subConceptInfo95.CapitalizedCaption = "Prolonged";
                subConceptInfo95.Caption = "Prolonged";
                subConceptInfo95.ConceptNo = 9901381;
                subConceptInfo95.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo95.HasRange = true;
                subConceptInfo95.IsTextFormatNumerical = true;
                subConceptInfo95.MaxValue = 0.0;
                subConceptInfo95.MinValue = 0.0;
                subConceptInfo95.ObjectOFCare = 1;
                subConceptInfo95.OrderNumberInGroup = 5;
                subConceptInfo95.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo9.SubConcepts.Add(subConceptInfo95);
                conceptInfo9.ResponseList.Add("Prolonged"); 

                CALMConceptInfo subConceptInfo96 = new CALMConceptInfo();
                subConceptInfo96.AbreviatedCaption = "Other";
                subConceptInfo96.CALMValueType = CALMValueType.String;
                subConceptInfo96.CapitalizedCaption = "Other";
                subConceptInfo96.Caption = "Other";
                subConceptInfo96.ConceptNo = -100451;
                subConceptInfo96.ConceptType = CALMConceptType.SubConcept;
                subConceptInfo96.HasRange = true;
                subConceptInfo96.IsTextFormatNumerical = true;
                subConceptInfo96.MaxValue = 0.0;
                subConceptInfo96.MinValue = 0.0;
                subConceptInfo96.ObjectOFCare = 1;
                subConceptInfo96.OrderNumberInGroup = 6;
                subConceptInfo96.TextFormatOverride = TextFormat.eAlpha;
                conceptInfo9.SubConcepts.Add(subConceptInfo96);
                conceptInfo9.ResponseList.Add("Other");

            conceptsInfos.Add(conceptInfo9);

            CALMConceptInfo conceptInfo10 = new CALMConceptInfo();
            conceptInfo10.AbreviatedCaption = "Mean Contraction Interval";
            conceptInfo10.CALMValueType = CALMValueType.String;
            conceptInfo10.CapitalizedCaption = "Mean Contraction Interval";
            conceptInfo10.Caption = "Mean Contraction Interval";
            conceptInfo10.ConceptNo = 9901731;
            conceptInfo10.ConceptType = CALMConceptType.Concept;
            conceptInfo10.DateTimeFormat = null;
            conceptInfo10.DecimalPlaces = 2;
            conceptInfo10.HasRange = true;
            conceptInfo10.IsCalculationSource = true;
            conceptInfo10.IsTextFormatNumerical = true;
            conceptInfo10.IsTimeSeriesConcept = true;
            conceptInfo10.MaxValue = 100.0;
            conceptInfo10.MinValue = 0.0;
            conceptInfo10.OrderNumberInGroup = 10;
            conceptInfo10.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo10);


            CALMConceptInfo conceptInfo11 = new CALMConceptInfo();
            conceptInfo11.AbreviatedCaption = "Contraction Duration Range";
            conceptInfo11.CALMValueType = CALMValueType.String;
            conceptInfo11.CapitalizedCaption = "Contraction Duration Range";
            conceptInfo11.Caption = "Contraction Duration Interval Range";
            conceptInfo11.ConceptNo = -101392;
            conceptInfo11.ConceptType = CALMConceptType.Concept;
            conceptInfo11.DateTimeFormat = null;
            conceptInfo11.DecimalPlaces = 0;
            conceptInfo11.HasRange = true;
            conceptInfo11.IsCalculationSource = true;
            conceptInfo11.IsTextFormatNumerical = true;
            conceptInfo11.IsTimeSeriesConcept = true;
            conceptInfo11.MaxValue = 100.0;
            conceptInfo11.MinValue = 0.0;
            conceptInfo11.OrderNumberInGroup = 11;
            conceptInfo11.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo11);

            CALMConceptInfo conceptInfo12 = new CALMConceptInfo();
            conceptInfo12.AbreviatedCaption = "Internal Intensity  Range";
            conceptInfo12.CALMValueType = CALMValueType.String;
            conceptInfo12.CapitalizedCaption = "Internal Intensity  Range";
            conceptInfo12.Caption = "Internal Intensity  Range";
            conceptInfo12.ConceptNo = -101393;
            conceptInfo12.ConceptType = CALMConceptType.Concept;
            conceptInfo12.DateTimeFormat = null;
            conceptInfo12.DecimalPlaces = 0;
            conceptInfo12.HasRange = true;
            conceptInfo12.IsCalculationSource = true;
            conceptInfo12.IsTextFormatNumerical = true;
            conceptInfo12.IsTimeSeriesConcept = true;
            conceptInfo12.MaxValue = 100.0;
            conceptInfo12.MinValue = 0.0;
            conceptInfo12.OrderNumberInGroup = 12;
            conceptInfo12.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo12);


            CALMConceptInfo conceptInfo13 = new CALMConceptInfo();
            conceptInfo13.AbreviatedCaption = "Calculated Duration  Range";
            conceptInfo13.CALMValueType = CALMValueType.String;
            conceptInfo13.CapitalizedCaption = "Calculated Duration  Range";
            conceptInfo13.Caption = "Calculated Duration  Range";
            conceptInfo13.ConceptNo = 9901732;
            conceptInfo13.ConceptType = CALMConceptType.Concept;
            conceptInfo13.DateTimeFormat = null;
            conceptInfo13.DecimalPlaces = 0;
            conceptInfo13.HasRange = true;
            conceptInfo13.IsCalculationSource = true;
            conceptInfo13.IsTextFormatNumerical = true;
            conceptInfo13.IsTimeSeriesConcept = true;
            conceptInfo13.MaxValue = 100.0;
            conceptInfo13.MinValue = 0.0;
            conceptInfo13.OrderNumberInGroup = 13;
            conceptInfo13.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo13);

            CALMConceptInfo conceptInfo14 = new CALMConceptInfo();
            conceptInfo14.AbreviatedCaption = "Calculated Intensity  Range";
            conceptInfo14.CALMValueType = CALMValueType.String;
            conceptInfo14.CapitalizedCaption = "Calculated Intensity  Range";
            conceptInfo14.Caption = "Calculated Intensity  Range";
            conceptInfo14.ConceptNo = 9901733;
            conceptInfo14.ConceptType = CALMConceptType.Concept;
            conceptInfo14.DateTimeFormat = null;
            conceptInfo14.DecimalPlaces = 0;
            conceptInfo14.HasRange = true;
            conceptInfo14.IsCalculationSource = true;
            conceptInfo14.IsTextFormatNumerical = true;
            conceptInfo14.IsTimeSeriesConcept = true;
            conceptInfo14.MaxValue = 100.0;
            conceptInfo14.MinValue = 0.0;
            conceptInfo14.OrderNumberInGroup = 14;
            conceptInfo14.TextFormatOverride = TextFormat.eAlpha;

            conceptsInfos.Add(conceptInfo14);


            return conceptsInfos;
        }

        private CALMConceptInfo SafeGetConceptInfo(int conceptId, List<ConceptNumberToColumnMappingModel> dbConcepts)
        {
            CALMConceptInfo conceptInfo = GetConceptInfo(conceptId);

            if(conceptInfo == null)
            {
                ConceptNumberToColumnMappingModel dbConcept = dbConcepts.FirstOrDefault(t => t.ConceptNumber == conceptId);

                if (dbConcept != null)
                {
                    conceptInfo = new CALMConceptInfo();
                    conceptInfo.ConceptNo = conceptId;
                    conceptInfo.ConceptType = CALMConceptType.Concept;
                    conceptInfo.CALMValueType = CALMValueType.String;
                    conceptInfo.ObjectOFCare = dbConcept.ObjectOfCare;
                    conceptInfo.Caption = dbConcept.Comments;
                }
            }

            return conceptInfo;
        }

        private CALMConceptInfo GetConceptInfo(int conceptId)
        {
            try
            {
                CALMConceptInfo conceptInfo = new CALMConceptInfo();

                LMS.Metadata.ConceptDef conceptDef = LMS.Metadata.ConceptDef.GetConceptDef(conceptId);
                int valueTypeInt = LMS.Metadata.Utils.GetConceptValueType(conceptId);
                CALMValueType calmValueType = (CALMValueType)valueTypeInt;

                conceptInfo.AbreviatedCaption = conceptDef.AbreviatedCaption;
                conceptInfo.CanHaveMultipleValues = conceptDef.CanHaveMultipleValues;
                conceptInfo.CapitalizedCaption = conceptDef.CapitalizedCaption;
                conceptInfo.Caption = conceptDef.Caption;
                conceptInfo.ConceptNo = conceptDef.ConceptNo;
                conceptInfo.CALMValueType = calmValueType;
                conceptInfo.ObjectOFCare = LMS.Metadata.Utils.IsMotherConcept(conceptId) ? 0 : 1;
                conceptInfo.HasIdList = conceptDef.HasIdList;
                conceptInfo.HasList = conceptDef.HasList;
                conceptInfo.HasRange = conceptDef.HasRange;
                conceptInfo.IsCalculated = conceptDef.IsCalculated;
                conceptInfo.IsCalculationSource = conceptDef.IsCalculationSource;
                conceptInfo.IsCheckBox = conceptDef.IsCheckBox;
                conceptInfo.IsDateTime = conceptDef.IsDateTime;
                conceptInfo.IsFreeText = conceptDef.IsFreeText;
                conceptInfo.IsPhoneNumber = conceptDef.IsPhoneNumber;
                conceptInfo.IsTextFormatNumerical = conceptDef.IsTextFormatNumerical;
                conceptInfo.IsTimeSeriesConcept = conceptDef.IsTimeSeriesConcept;

                if (conceptDef.HasRange)
                {
                    conceptInfo.MinValue = conceptDef.MinValue;
                    conceptInfo.MaxValue = conceptDef.MaxValue;
                    conceptInfo.DecimalPlaces = conceptDef.DecimalPlaces;
                }

                if (conceptDef.HasList)
                {
                    conceptInfo.ResponseList = conceptDef.ResponseList.Cast<string>().ToList();
                }

                //TODO: check when these values are used
                //conceptInfo.DateTimeFormat = conceptDef.DateTimeFormat;                                             
                //conceptInfo.IncValue = conceptDef.IncValue;
                //conceptInfo.ListId = conceptDef.ListId;
                //conceptInfo.MaxChars = conceptDef.MaxChars;
                //conceptInfo.MaxCharsOverride = conceptDef.MaxCharsOverride;
                //conceptInfo.RangeList = conceptDef.RangeList;
                //conceptInfo.ResponseIdList = conceptDef.ResponseIdList;
                //conceptInfo.TextFormatOverride = conceptDef.TextFormatOverride;

                return conceptInfo;
            }
            catch(Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Warning, "CALMManager", "GetConceptInfo for " + conceptId.ToString(), ex);
            }

            return null;
        }

        public CALMResults CheckUserRights(string loginName, string password, int function, int action)
        {
            if (CALMManagerSettings.Instance.CALMServicesEnabled)
            {
                lock (m_lock)
                {
                    bool bRes = false;
                    CALMResults result = CALMResults.UnKnown;

                    try
                    {
                        //login
                        bRes = m_calmConnection.Login(loginName, password);
                        if (bRes)
                        {
                            string errorString = String.Empty;
                            bRes = m_calmConnection.CheckUserRights(function, action, out errorString);

                            if (bRes)
                            {
                                Logger.WriteLogEntry(TraceEventType.Verbose, "CALMManager", "Success to CheckUserRights");
                                result = CALMResults.IsAuthorised;
                            }
                            else
                            {
                                Logger.WriteLogEntry(TraceEventType.Error, "CALMManager", "Failed to CheckUserRights. error: " + errorString);
                                result = CALMResults.IsNotAuthorised;
                            }
                        }
                        else
                        {
                            result = CALMResults.WrongLoginOrPassword;
                        }
                    }
                    catch (System.Exception ex) // Catches every exception
                    {
                        Logger.WriteLogEntry(TraceEventType.Critical, "CALMManager", "Error on CheckUserRights", ex);
                    }
                    finally
                    {
                        //logout
                        bRes = bRes & m_calmConnection.Logout();
                    }

                    return result;
                }
            }
            else
                return CALMResults.UnKnown; 
        }

        public bool IsPluginEnabled()
        {
            if (CALMEnabled)
            {
                lock (m_lock)
                {
                    bool bRes = false;
                    try
                    {
                        bRes = m_calmConnection.GetSiteConfig().ExportAccessEnabled;
                    }
                    catch (System.Exception ex) // Catches every exception
                    {
                        Logger.WriteLogEntry(TraceEventType.Critical, "CALMManager", "Error on IsPluginEnabled", ex);
                    }

                    return bRes;

                }
            }
            else
                return true;
        }
    }
}
