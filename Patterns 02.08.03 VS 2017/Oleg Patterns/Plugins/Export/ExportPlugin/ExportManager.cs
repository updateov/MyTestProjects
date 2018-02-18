using CommonLogger;
using Export.Entities.ExportControlConfig;
using MMSInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Export.Entities;
using AutoMapper;
using Export.PluginDataModel;

namespace Export.Plugin
{
    public class ExportManager
    {
        #region Properties and Members

        private static Object s_lock = new Object();
        private String m_configXMLPath = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + @"\ExportConfig.xml";

        public ExportConfig ExportDataConfig { get; set; }
        public List<ConceptInfo> ConceptsInfos = null;

        #endregion

        #region Singleton

        private static ExportManager s_instance = null;

        private ExportManager()
        {
            InitAutoMapper();
            InitManager();
        }

        public static ExportManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                            s_instance = new ExportManager();
                    }
                }

                return s_instance;
            }
        }

        #endregion
        private void InitAutoMapper()
        {
            Mapper.Initialize(c =>
            {
                c.CreateMap<Entities.BaseConcept, MMSInterfaces.BaseConcept>()
                .Include<Entities.IntConcept, MMSInterfaces.IntConcept>()
                .Include<Entities.DoubleConcept, MMSInterfaces.DoubleConcept>()
                .Include<Entities.StringConcept, MMSInterfaces.StringConcept>()
                .Include<Entities.ComboConcept, MMSInterfaces.ComboConcept>()
                .Include<Entities.CalculatedComboConcept, MMSInterfaces.CalculatedComboConcept>()
                .Include<Entities.CalculatedCheckboxGroupConcept, MMSInterfaces.CalculatedCheckboxGroupConcept>()
                .Include<Entities.ComboMultiValueConcept, MMSInterfaces.ComboMultiValueConcept>();
                c.CreateMap<Entities.IntConcept, MMSInterfaces.IntConcept>();
                c.CreateMap<Entities.DoubleConcept, MMSInterfaces.DoubleConcept>();
                c.CreateMap<Entities.StringConcept, MMSInterfaces.StringConcept>();
                c.CreateMap<Entities.ComboConcept, MMSInterfaces.ComboConcept>();
                c.CreateMap<Entities.CalculatedComboConcept, MMSInterfaces.CalculatedComboConcept>();
                c.CreateMap<Entities.CalculatedCheckboxGroupConcept, MMSInterfaces.CalculatedCheckboxGroupConcept>();
                c.CreateMap<Entities.ComboMultiValueConcept, MMSInterfaces.ComboMultiValueConcept>();
            });
        }

        private void InitManager()
        {
            if (PluginSettings.Instance.CALMServicesEnabled)
            {
                ExportDataConfig = LoadFromConfigXML();

                ConceptsInfos = GetConceptsInfosInGroup();
            }
            else
            {
                ConceptsInfos = GetHardcodedPeriWatchConcepts();
            }

        }
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

        private List<ConceptInfo> GetConceptsInfosInGroup()
        {
            List<ConceptInfo> toRet = new List<ConceptInfo>();
            try
            {
                List<ConceptNumberToColumnMappingModel> dbConcepts = new List<ConceptNumberToColumnMappingModel>();
                DBManager.Instance.CreateDB();
                bool bSucc = DBManager.Instance.GetConceptNumberToColumnMapping(ref dbConcepts);
                List<int> conceptsIds = ExportDataConfig.GetConceptsFromConfig();
                NetTcpBinding bind = new NetTcpBinding(SecurityMode.Transport);
                String url = PluginSettings.Instance.CALMServerLink;
                EndpointAddress addr = new EndpointAddress(url);
                ChannelFactory<IMMService> chn = new ChannelFactory<IMMService>(bind, addr);
                IMMService conn = chn.CreateChannel();
                MMSConceptsInfoResponse res = conn.GetConceptsInfo(conceptsIds);
                toRet.AddRange(res.ConceptsInfo);
                foreach (var item in res.NonExistingConcetpIds)
                {
                    ConceptNumberToColumnMappingModel dbConcept = dbConcepts.FirstOrDefault(t => t.ConceptNumber == item);

                    if (dbConcept != null)
                    {
                        ConceptInfo conceptInfo = new ConceptInfo();
                        conceptInfo.ConceptNo = item;
                        conceptInfo.ConceptType = ConceptType.Concept;
                        conceptInfo.ObjectOFCare = dbConcept.ObjectOfCare;
                        conceptInfo.Caption = dbConcept.Comments;
                        toRet.Add(conceptInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "GetConceptsInfosInGroup", "Error occurred in GetConceptsInfosInGroup.", ex);
            }

            return toRet;
        }

        private List<ConceptInfo> GetHardcodedPeriWatchConcepts()
        {
            List<ConceptInfo> conceptsInfos = new List<ConceptInfo>();

            ConceptInfo conceptInfo1 = new ConceptInfo();
            conceptInfo1.Caption = "Time Range";
            conceptInfo1.ConceptNo = -102100;
            conceptInfo1.ConceptType = ConceptType.Concept;
            conceptInfo1.HasRange = true;
            conceptInfo1.MaxValue = 100.0;
            conceptInfo1.MinValue = 0.0;
            conceptInfo1.OrderNumberInGroup = 1;

            conceptsInfos.Add(conceptInfo1);

            ConceptInfo conceptInfo2 = new ConceptInfo();
            conceptInfo2.Caption = "Contraction Interval Range";
            conceptInfo2.ConceptNo = -102101;
            conceptInfo2.ConceptType = ConceptType.Concept;
            conceptInfo2.DecimalPlaces = 2;
            conceptInfo2.HasRange = true;
            conceptInfo2.MaxValue = 100.0;
            conceptInfo2.MinValue = 0.0;
            conceptInfo2.OrderNumberInGroup = 2;

            conceptsInfos.Add(conceptInfo2);

            ConceptInfo conceptInfo3 = new ConceptInfo();
            conceptInfo3.Caption = "# of Contractions";
            conceptInfo3.ConceptNo = -102102;
            conceptInfo3.ConceptType = ConceptType.Concept;
            conceptInfo3.HasRange = true;
            conceptInfo3.MaxValue = 100.0;
            conceptInfo3.MinValue = 0.0;
            conceptInfo3.OrderNumberInGroup = 3;

            conceptsInfos.Add(conceptInfo3);

            ConceptInfo conceptInfo4 = new ConceptInfo();
            conceptInfo4.Caption = "# of Long Contractions";
            conceptInfo4.ConceptNo = -102103;
            conceptInfo4.ConceptType = ConceptType.Concept;
            conceptInfo4.HasRange = true;
            conceptInfo4.MaxValue = 100.0;
            conceptInfo4.MinValue = 0.0;
            conceptInfo4.OrderNumberInGroup = 4;

            conceptsInfos.Add(conceptInfo4);

            ConceptInfo conceptInfo5 = new ConceptInfo();
            conceptInfo5.Caption = "Montevideo Units (per 10 min)";
            conceptInfo5.ConceptNo = -101395;
            conceptInfo5.ConceptType = ConceptType.Concept;
            conceptInfo5.HasRange = true;
            conceptInfo5.MaxValue = 400.0;
            conceptInfo5.MinValue = 0.0;
            conceptInfo5.OrderNumberInGroup = 5;

            conceptsInfos.Add(conceptInfo5);

            ConceptInfo conceptInfo6 = new ConceptInfo();
            conceptInfo6.Caption = "Baseline FHR";
            conceptInfo6.ConceptNo = -101385;
            conceptInfo6.ConceptType = ConceptType.Concept;
            conceptInfo6.HasRange = true;
            conceptInfo6.MaxValue = 0.0;
            conceptInfo6.MinValue = 0.0;
            conceptInfo6.ObjectOFCare = 1;
            conceptInfo6.OrderNumberInGroup = 6;

            conceptsInfos.Add(conceptInfo6);


            ConceptInfo conceptInfo7 = new ConceptInfo();
            conceptInfo7.Caption = "Variability";
            conceptInfo7.ConceptNo = -101420;
            conceptInfo7.ConceptType = ConceptType.Concept;
            conceptInfo7.HasRange = true;
            conceptInfo7.MinValue = 0.0;
            conceptInfo7.MaxValue = 0.0;
            conceptInfo7.ObjectOFCare = 1;
            conceptInfo7.OrderNumberInGroup = 7;
            conceptInfo7.HasList = true;
            conceptInfo7.ResponseList = new List<string>();
            ConceptInfo subConceptInfo71 = new ConceptInfo();
            subConceptInfo71.Caption = "";
            subConceptInfo71.ConceptNo = 9900742;
            subConceptInfo71.ConceptType = ConceptType.String;
            subConceptInfo71.MaxValue = -1.0;
            subConceptInfo71.MinValue = -1.0;
            subConceptInfo71.OrderNumberInGroup = 1;
            conceptInfo7.SubConcepts.Add(subConceptInfo71);
            conceptInfo7.ResponseList.Add("");

            ConceptInfo subConceptInfo72 = new ConceptInfo();
            subConceptInfo72.Caption = "Absent (0)";
            subConceptInfo72.ConceptNo = 9901706;
            subConceptInfo72.ConceptType = ConceptType.SubConcept;
            subConceptInfo72.HasRange = true;
            subConceptInfo72.MaxValue = 0.0;
            subConceptInfo72.MinValue = 0.0;
            subConceptInfo72.OrderNumberInGroup = 2;
            conceptInfo7.SubConcepts.Add(subConceptInfo72);
            conceptInfo7.ResponseList.Add("Absent (0)");

            ConceptInfo subConceptInfo73 = new ConceptInfo();
            subConceptInfo73.Caption = "Minimal (1-5)";
            subConceptInfo73.ConceptNo = 9901705;
            subConceptInfo73.ConceptType = ConceptType.SubConcept;
            subConceptInfo73.HasRange = true;
            subConceptInfo73.MaxValue = 5.0;
            subConceptInfo73.MinValue = 1.0;
            subConceptInfo73.ObjectOFCare = 1;
            subConceptInfo73.OrderNumberInGroup = 3;
            conceptInfo7.SubConcepts.Add(subConceptInfo73);
            conceptInfo7.ResponseList.Add("Minimal (1-5)");

            ConceptInfo subConceptInfo74 = new ConceptInfo();
            subConceptInfo74.Caption = "Moderate (6-25)";
            subConceptInfo74.ConceptNo = 9901704;
            subConceptInfo74.ConceptType = ConceptType.SubConcept;
            subConceptInfo74.HasRange = true;
            subConceptInfo74.MaxValue = 25.0;
            subConceptInfo74.MinValue = 6.0;
            subConceptInfo74.ObjectOFCare = 1;
            subConceptInfo74.OrderNumberInGroup = 4;
            conceptInfo7.SubConcepts.Add(subConceptInfo74);
            conceptInfo7.ResponseList.Add("Moderate (6-25)");

            ConceptInfo subConceptInfo75 = new ConceptInfo();
            subConceptInfo75.Caption = "Marked (>25)";
            subConceptInfo75.ConceptNo = 9901703;
            subConceptInfo75.ConceptType = ConceptType.SubConcept;
            subConceptInfo75.HasRange = true;
            subConceptInfo75.MaxValue = 999.0;
            subConceptInfo75.MinValue = 26.0;
            subConceptInfo75.ObjectOFCare = 1;
            subConceptInfo75.OrderNumberInGroup = 5;
            conceptInfo7.SubConcepts.Add(subConceptInfo75);
            conceptInfo7.ResponseList.Add("Marked (>25)");

            ConceptInfo subConceptInfo76 = new ConceptInfo();
            subConceptInfo76.Caption = "Sinusoidal";
            subConceptInfo76.ConceptNo = 9902516;
            subConceptInfo76.ConceptType = ConceptType.String;
            subConceptInfo76.MaxValue = -1.0;
            subConceptInfo76.MinValue = -1.0;
            subConceptInfo76.OrderNumberInGroup = 6;
            conceptInfo7.SubConcepts.Add(subConceptInfo76);
            conceptInfo7.ResponseList.Add("Sinusoidal");

            ConceptInfo subConceptInfo77 = new ConceptInfo();
            subConceptInfo77.Caption = "Unable to determine";
            subConceptInfo77.ConceptNo = 9901707;
            subConceptInfo77.ConceptType = ConceptType.String;
            subConceptInfo77.MaxValue = -1.0;
            subConceptInfo77.MinValue = -1.0;
            subConceptInfo77.OrderNumberInGroup = 7;
            conceptInfo7.SubConcepts.Add(subConceptInfo77);
            conceptInfo7.ResponseList.Add("Unable to determine");

            conceptsInfos.Add(conceptInfo7);


            ConceptInfo conceptInfo8 = new ConceptInfo();
            conceptInfo8.Caption = "Accels";
            conceptInfo8.ConceptNo = -101417;
            conceptInfo8.ConceptType = ConceptType.Concept;
            conceptInfo8.HasRange = true;
            conceptInfo8.MinValue = 0.0;
            conceptInfo8.MaxValue = 0.0;
            conceptInfo8.ObjectOFCare = 1;
            conceptInfo8.OrderNumberInGroup = 8;
            conceptInfo8.HasList = true;
            conceptInfo8.ResponseList = new List<string>();

            ConceptInfo subConceptInfo81 = new ConceptInfo();
            subConceptInfo81.Caption = "";
            subConceptInfo81.ConceptNo = 9900742;
            subConceptInfo81.ConceptType = ConceptType.String;
            subConceptInfo81.MaxValue = -1.0;
            subConceptInfo81.MinValue = -1.0;
            subConceptInfo81.OrderNumberInGroup = 1;
            conceptInfo8.SubConcepts.Add(subConceptInfo81);
            conceptInfo8.ResponseList.Add("");

            ConceptInfo subConceptInfo82 = new ConceptInfo();
            subConceptInfo82.Caption = "Yes";
            subConceptInfo82.ConceptNo = 9900508;
            subConceptInfo82.ConceptType = ConceptType.SubConcept;
            subConceptInfo82.HasRange = true;
            subConceptInfo82.MaxValue = 100.0;
            subConceptInfo82.MinValue = 1.0;
            subConceptInfo82.ObjectOFCare = 1;
            subConceptInfo82.OrderNumberInGroup = 2;
            conceptInfo8.SubConcepts.Add(subConceptInfo82);
            conceptInfo8.ResponseList.Add("Yes");

            ConceptInfo subConceptInfo83 = new ConceptInfo();
            subConceptInfo83.Caption = "No";
            subConceptInfo83.ConceptNo = 9900509;
            subConceptInfo83.ConceptType = ConceptType.SubConcept;
            subConceptInfo83.HasRange = true;
            subConceptInfo83.MaxValue = 0.0;
            subConceptInfo83.MinValue = 0.0;
            subConceptInfo83.ObjectOFCare = 1;
            subConceptInfo83.OrderNumberInGroup = 3;
            conceptInfo8.SubConcepts.Add(subConceptInfo83);
            conceptInfo8.ResponseList.Add("No");

            conceptsInfos.Add(conceptInfo8);

            ConceptInfo conceptInfo9 = new ConceptInfo();
            conceptInfo9.Caption = "Decels";
            conceptInfo9.ConceptNo = -101418;
            conceptInfo9.ConceptType = ConceptType.Concept;
            conceptInfo9.HasRange = false;
            conceptInfo9.MinValue = 0.0;
            conceptInfo9.MaxValue = 0.0;
            conceptInfo9.ObjectOFCare = 1;
            conceptInfo9.OrderNumberInGroup = 9;
            conceptInfo9.HasList = true;
            conceptInfo9.ResponseList = new List<string>();

            ConceptInfo subConceptInfo91 = new ConceptInfo();
            subConceptInfo91.Caption = "None";
            subConceptInfo91.ConceptNo = 9900021;
            subConceptInfo91.ConceptType = ConceptType.SubConcept;
            subConceptInfo91.HasRange = true;
            subConceptInfo91.MaxValue = -1.0;
            subConceptInfo91.MinValue = -1.0;
            subConceptInfo91.ObjectOFCare = 1;
            subConceptInfo91.OrderNumberInGroup = 1;
            conceptInfo9.SubConcepts.Add(subConceptInfo91);
            conceptInfo9.ResponseList.Add("None");

            ConceptInfo subConceptInfo92 = new ConceptInfo();
            subConceptInfo92.Caption = "Early";
            subConceptInfo92.ConceptNo = 9901382;
            subConceptInfo92.ConceptType = ConceptType.SubConcept;
            subConceptInfo92.HasRange = true;
            subConceptInfo92.MaxValue = 0.0;
            subConceptInfo92.MinValue = 0.0;
            subConceptInfo92.ObjectOFCare = 1;
            subConceptInfo92.OrderNumberInGroup = 2;
            conceptInfo9.SubConcepts.Add(subConceptInfo92);
            conceptInfo9.ResponseList.Add("Early");

            ConceptInfo subConceptInfo93 = new ConceptInfo();
            subConceptInfo93.Caption = "Variable";
            subConceptInfo93.ConceptNo = 9900798;
            subConceptInfo93.ConceptType = ConceptType.SubConcept;
            subConceptInfo93.HasRange = true;
            subConceptInfo93.MaxValue = 0.0;
            subConceptInfo93.MinValue = 0.0;
            subConceptInfo93.ObjectOFCare = 1;
            subConceptInfo93.OrderNumberInGroup = 3;
            conceptInfo9.SubConcepts.Add(subConceptInfo93);
            conceptInfo9.ResponseList.Add("Variable");

            ConceptInfo subConceptInfo94 = new ConceptInfo();
            subConceptInfo94.Caption = "Late";
            subConceptInfo94.ConceptNo = 9901383;
            subConceptInfo94.ConceptType = ConceptType.SubConcept;
            subConceptInfo94.HasRange = true;
            subConceptInfo94.MaxValue = 0.0;
            subConceptInfo94.MinValue = 0.0;
            subConceptInfo94.ObjectOFCare = 1;
            subConceptInfo94.OrderNumberInGroup = 4;
            conceptInfo9.SubConcepts.Add(subConceptInfo94);
            conceptInfo9.ResponseList.Add("Late");

            ConceptInfo subConceptInfo95 = new ConceptInfo();
            subConceptInfo95.Caption = "Prolonged";
            subConceptInfo95.ConceptNo = 9901381;
            subConceptInfo95.ConceptType = ConceptType.SubConcept;
            subConceptInfo95.HasRange = true;
            subConceptInfo95.MaxValue = 0.0;
            subConceptInfo95.MinValue = 0.0;
            subConceptInfo95.ObjectOFCare = 1;
            subConceptInfo95.OrderNumberInGroup = 5;
            conceptInfo9.SubConcepts.Add(subConceptInfo95);
            conceptInfo9.ResponseList.Add("Prolonged");

            ConceptInfo subConceptInfo96 = new ConceptInfo();
            subConceptInfo96.Caption = "Other";
            subConceptInfo96.ConceptNo = -100451;
            subConceptInfo96.ConceptType = ConceptType.SubConcept;
            subConceptInfo96.HasRange = true;
            subConceptInfo96.MaxValue = 0.0;
            subConceptInfo96.MinValue = 0.0;
            subConceptInfo96.ObjectOFCare = 1;
            subConceptInfo96.OrderNumberInGroup = 6;
            conceptInfo9.SubConcepts.Add(subConceptInfo96);
            conceptInfo9.ResponseList.Add("Other");

            conceptsInfos.Add(conceptInfo9);

            ConceptInfo conceptInfo10 = new ConceptInfo();
            conceptInfo10.Caption = "Mean Contraction Interval";
            conceptInfo10.ConceptNo = 9901731;
            conceptInfo10.ConceptType = ConceptType.Concept;
            conceptInfo10.DecimalPlaces = 2;
            conceptInfo10.HasRange = true;
            conceptInfo10.MaxValue = 100.0;
            conceptInfo10.MinValue = 0.0;
            conceptInfo10.OrderNumberInGroup = 10;

            conceptsInfos.Add(conceptInfo10);


            ConceptInfo conceptInfo11 = new ConceptInfo();
            conceptInfo11.Caption = "Contraction Duration Interval Range";
            conceptInfo11.ConceptNo = -101392;
            conceptInfo11.ConceptType = ConceptType.Concept;
            conceptInfo11.DecimalPlaces = 0;
            conceptInfo11.HasRange = true;
            conceptInfo11.MaxValue = 100.0;
            conceptInfo11.MinValue = 0.0;
            conceptInfo11.OrderNumberInGroup = 11;

            conceptsInfos.Add(conceptInfo11);

            ConceptInfo conceptInfo12 = new ConceptInfo();
            conceptInfo12.Caption = "Internal Intensity  Range";
            conceptInfo12.ConceptNo = -101393;
            conceptInfo12.ConceptType = ConceptType.Concept;
            conceptInfo12.DecimalPlaces = 0;
            conceptInfo12.HasRange = true;
            conceptInfo12.MaxValue = 100.0;
            conceptInfo12.MinValue = 0.0;
            conceptInfo12.OrderNumberInGroup = 12;

            conceptsInfos.Add(conceptInfo12);


            ConceptInfo conceptInfo13 = new ConceptInfo();
            conceptInfo13.Caption = "Calculated Duration  Range";
            conceptInfo13.ConceptNo = 9901732;
            conceptInfo13.ConceptType = ConceptType.Concept;
            conceptInfo13.DecimalPlaces = 0;
            conceptInfo13.HasRange = true;
            conceptInfo13.MaxValue = 100.0;
            conceptInfo13.MinValue = 0.0;
            conceptInfo13.OrderNumberInGroup = 13;

            conceptsInfos.Add(conceptInfo13);

            ConceptInfo conceptInfo14 = new ConceptInfo();
            conceptInfo14.Caption = "Calculated Intensity  Range";
            conceptInfo14.ConceptNo = 9901733;
            conceptInfo14.ConceptType = ConceptType.Concept;
            conceptInfo14.DecimalPlaces = 0;
            conceptInfo14.HasRange = true;
            conceptInfo14.MaxValue = 100.0;
            conceptInfo14.MinValue = 0.0;
            conceptInfo14.OrderNumberInGroup = 14;

            conceptsInfos.Add(conceptInfo14);


            return conceptsInfos;
        }

        public bool SaveInterval(String visitKey, Interval exportedInterval)
        {
            bool toRet = false;
            DateTime conceptTime = exportedInterval.EndTime.AddSeconds(1).ToLocalTime();
            List<MMSInterfaces.BaseConcept> toSend = ConvertConcepts(exportedInterval.Concepts, conceptTime, true);
            try
            {
                NetTcpBinding bind = new NetTcpBinding(SecurityMode.Transport);
                String url = PluginSettings.Instance.CALMServerLink;
                EndpointAddress addr = new EndpointAddress(url);
                ChannelFactory<IMMService> chn = new ChannelFactory<IMMService>(bind, addr);
                IMMService conn = chn.CreateChannel();
                MMSResposeBase res = conn.SaveObservations(visitKey, toSend, ConceptsInfos, exportedInterval.LoginName);
                toRet = res.ResponseCode == MMSResponseCode.Success;
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "ExportManager", "Error saving interval", ex);
            }

            return toRet;
        }

        private List<MMSInterfaces.BaseConcept> ConvertConcepts(List<Entities.BaseConcept> exportConcepts, DateTime conceptTime, bool bFilterReadOnly)
        {
            List<MMSInterfaces.BaseConcept> toRet = new List<MMSInterfaces.BaseConcept>();
            foreach (var item in exportConcepts)
            {
                ExportEntity entity = ExportDataConfig.GetEntityById(item.Id);
                if (entity.ReadOnlyId == item.Id)
                    continue;

                var toAdd = Mapper.Map<MMSInterfaces.BaseConcept>(item);
                toAdd.ConceptTime = conceptTime;
                toRet.Add(toAdd);
            }

            return toRet;
        }
    }
}
