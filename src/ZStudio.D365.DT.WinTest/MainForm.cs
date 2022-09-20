using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZStudio.D365.DT.WinTest
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnTestCode_Click(object sender, EventArgs e)
        {
            string curSchemaName = "journey";
            string refSchemaName = "lead";
            string originalName = "seedid";

            //auto-generate using the current entity name and the reference entity name with the attribute name, need to be shorter than 82
            string relName = $"{refSchemaName}_{curSchemaName}_{originalName}";

            //append ID if required
            if (!relName.EndsWith("Id", StringComparison.CurrentCultureIgnoreCase))
                relName += "Id";

            //check for length
            if (relName.Length > 82)
            {
                //need a shorter name
                int extraChar = relName.Length - 82 + 2;
                int cutBy = (int)Math.Floor((decimal)(extraChar / 2));

                //cut the table schema names
                string currentTable = curSchemaName.Substring(0, curSchemaName.Length - cutBy);
                string refTable = refSchemaName.Substring(0, refSchemaName.Length - cutBy);

                relName = $"{refTable}_{currentTable}_{originalName}";

                //append ID if required
                if (!relName.EndsWith("Id", StringComparison.CurrentCultureIgnoreCase))
                    relName += "Id";
            }

            //auto-generated name
            string result = string.Format("{0}_{1}", "mrm", relName);

            MessageBox.Show(result + " - " + result.Length);
        }
    }
}