using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aspire.Studio
{
    public partial class ConfigureDialog : Form
    {
        public ConfigureDialog()
        {
            InitializeComponent();
            transportComboBox.SelectedIndex = 0;

        }

        public string Transport { get { return this.transportComboBox.SelectedItem as string; } }
    }
}
