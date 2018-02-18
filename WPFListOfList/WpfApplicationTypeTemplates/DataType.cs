using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplicationTypeTemplates
{
    public class IntDataType : BaseDataType
    {
        public IntDataType()
        {
        }

        public int Data { get; set; }
        public override string DisplayValue
        {
            get
            {
                return Data.ToString();
            }
            set
            {
                String val = value;
                int nVal;
                Int32.TryParse(val, out nVal);
                Data = nVal;
            }
        }
    }

    public class FloatDataType : BaseDataType
    {
        public FloatDataType()
        {
        }

        public float Data { get; set; }

        public override string DisplayValue
        {
            get
            {
                return Data.ToString();
            }
            set
            {
                String val = value;
                float nVal;
                Single.TryParse(val, out nVal);
                Data = nVal;
            }
        }
    }
}
