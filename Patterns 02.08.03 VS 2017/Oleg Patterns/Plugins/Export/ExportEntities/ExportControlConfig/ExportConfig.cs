using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Export.Entities.ExportControlConfig
{
    [Serializable]
    public enum ExportControlTypes
    {
        Int,
 	    Double,
 	    String,
 	    Combo,        
        CalculatedCombo,
 	    CheckboxGroup,
 	    CalculatedCheckboxGroup,
 	    CalculatedCheckboxGroupItem,
        ComboMultiValue,
        Range,
        RangeDoubleConcept,
        Rounding,
        CheckBox
    };

    [Serializable]
    public class ExportConfig
    {
        #region Properties

        [XmlElement("ExportTab", typeof(ExportTab))]
        public List<ExportTab> Tabs { get; set; }

        #endregion

        public ExportConfig()
        {
        }

        public ExportEntity GetEntityById(int conceptId)
        {
            foreach (ExportTab tab in Tabs)
            {
                if (tab.IsVisible == true)
                {
                    foreach (ExportColumn col in tab.Columns)
                    {
                        foreach (ExportEntity control in col.Entities)
                        {
                            if (control.ConceptId == conceptId || control.ReadOnlyId == conceptId)
                            {
                                return control;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public List<int> GetConceptsFromConfig()
        {
            List<int> list = new List<int>();

            foreach (ExportTab tab in Tabs)
            {
                if (tab.IsVisible == true)
                {
                    foreach (ExportColumn col in tab.Columns)
                    {
                        foreach (ExportEntity control in col.Entities)
                        {
                            if (control.IsVisible == true)
                            {
                                list.Add(control.ConceptId);

                                if(control.ReadOnlyId != 0)
                                {
                                    list.Add(control.ReadOnlyId);
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        public void RemoveNotVisible()
        {
            foreach (ExportTab tab in Tabs)
            {
                foreach (ExportColumn col in tab.Columns)
                {
                    // Remove not visible controls
                    col.Entities.RemoveAll(t => t.IsVisible == false);                        
                }
                // Remove empty columns
                //tab.Columns.RemoveAll(t => t.Entities.Count == 0);
            }
            // Remove not visible tabs
            Tabs.RemoveAll(t => t.IsVisible == false);
        }
    }
}
