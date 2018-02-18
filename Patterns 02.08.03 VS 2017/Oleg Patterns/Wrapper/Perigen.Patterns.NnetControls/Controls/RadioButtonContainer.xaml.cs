using Export.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Perigen.Patterns.NnetControls.Controls
{
    public partial class RadioButtonContainer : UserControl, IBaseExportControl
    {
        public RadioButton Control
        {
            get {
                return rb;
            }
        }

        public RadioButtonContainer()
        {
            InitializeComponent();
        }

        public void SetFocus()
        {
            rb.Focus();
        }
    }
}
