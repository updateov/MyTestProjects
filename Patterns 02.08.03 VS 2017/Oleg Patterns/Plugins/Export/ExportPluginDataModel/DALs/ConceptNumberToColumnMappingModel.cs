using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.PluginDataModel
{
    public class ConceptNumberToColumnMappingModel
    {
        public int ConceptNumber { get; set; }
        public int ObjectOfCare { get; set; }
        public int OrderId { get; set; }
        public string ColumnName { get; set; }
        public string ConceptType { get; set; }
        public string Comments { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateInserted { get; set; }


        public ConceptNumberToColumnMappingModel()
        { 

        }

        public ConceptNumberToColumnMappingModel(ConceptNumberToColumnMappingModel model)
        {
            ConceptNumber = model.ConceptNumber;
            ObjectOfCare = model.ObjectOfCare;
            OrderId = model.OrderId;
            ColumnName = model.ColumnName;
            ConceptType = model.ConceptType;
            Comments = model.Comments;
            DateUpdated = model.DateUpdated;
            DateInserted = model.DateInserted;
        }
    }
}
