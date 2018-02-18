using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Entities.ExportControlConfig
{
    [Serializable]
    public class ExportEntity
    {
        #region Properties
        public int ConceptId { get; set; }
        public ExportControlTypes ControlType { get; set; }
        public int? MaxSelect { get; set; }
        public double? RoundBy { get; set; }
        public bool IsVisible { get; set; }

        //For integration only.
        //IsVisible should be removed.
        public bool IsActive
        {
            get
            {
                return IsVisible;
            }
            set
            {
                IsVisible = value;
            }
        }

        public int? GroupedBy { get; set; }
        public bool IsMVU { get; set; }
        public int ReadOnlyId { get; set; }
        public bool IsHidden { get; set; }
        #endregion

        public ExportEntity()
        {
        }
    }
}
