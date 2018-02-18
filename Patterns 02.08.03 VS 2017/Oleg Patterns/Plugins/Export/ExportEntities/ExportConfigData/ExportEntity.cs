using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Entities.ExportConfigData
{
    [Serializable]
    public class ExportEntity
    {
        #region Properties
        public int ConceptId { get; set; }
        public int ControlType { get; set; }
        public int MaxSelect { get; set; }
        public double RoundBy { get; set; }
        public bool IsVisible { get; set; }
        #endregion

        public ExportEntity()
        {
            MaxSelect = 1;
            RoundBy = 0.1;
        }
    }
}
