using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IMI_SummaeryDemo
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1(string filepath)
        {
            InitializeComponent();

            this.axAcroPDF1.LoadFile(filepath);
        }
    }
}
