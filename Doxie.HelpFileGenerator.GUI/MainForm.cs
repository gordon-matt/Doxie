using System;
using System.Windows.Forms;
using Doxie.Core.Services;

namespace Doxie.HelpFileGenerator.GUI
{
    public partial class MainForm : Form
    {
        private string assembliesPath;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (dlgFolderBrowser.ShowDialog() == DialogResult.OK)
            {
                assembliesPath = dlgFolderBrowser.SelectedPath;
                txtPath.Text = assembliesPath;
                btnGenerate.Enabled = true;
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            var assemblyFinderService = new AssemblyFinderService(assembliesPath);
            assemblyFinderService.GenerateJsonFile();
            MessageBox.Show("The assemblies.json has been generated in the specified folder.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Reset
            assembliesPath = null;
            txtPath.Text = null;
            btnGenerate.Enabled = false;
        }
    }
}