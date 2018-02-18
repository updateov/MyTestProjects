using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perigen.Patterns.NnetControls
{
    public interface IBaseExportControl
    {
        void SetFocus();   
    }

    public interface IValidatableControl
    {
        bool IsValidValue();
    }
}
