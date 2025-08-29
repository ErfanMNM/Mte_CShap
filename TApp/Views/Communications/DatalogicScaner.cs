using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TApp.Views.Communications
{
    public partial class DatalogicScaner : UIPage
    {
        public MTs.Datalogic.DatalogicCamera camera { get; set; } = new MTs.Datalogic.DatalogicCamera("127.0.0.1",51236);

        public DatalogicScaner()
        {
            InitializeComponent();
        }

        public void Setup()
        {
            
        }
    }
}
