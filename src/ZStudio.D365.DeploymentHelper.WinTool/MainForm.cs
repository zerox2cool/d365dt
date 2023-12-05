using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZStudio.D365.DeploymentHelper.WinTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void btnTestCode_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> config = new Dictionary<string, object>();
            config.Add("SolutionName", "cms100fullsolution");
            config.Add("IncrementRevision", true);
            config.Add("IncrementBuild", false);
            config.Add("IncrementMinor", false);
            config.Add("IncrementMajor", false);

            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            Clipboard.SetText(json);
        }
    }
}