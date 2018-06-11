using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Doxie.Core.Services;
using Extenso.Collections;

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

                clbFiles.Items.Clear();
                var files = Directory.EnumerateFiles(assembliesPath, "*.dll").OrderBy(x => x);
                foreach (string file in files)
                {
                    clbFiles.Items.Add(file);
                }

                clbFiles.Enabled = true;
                btnGenerate.Enabled = true;
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var selectedFiles = clbFiles.CheckedItems.OfType<object>().Select(x => x.ToString());

            if (selectedFiles.IsNullOrEmpty())
            {
                MessageBox.Show("You must select at least one assembly.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            JsonHelpFileGenerator.Generate(selectedFiles, assembliesPath);
            MessageBox.Show("The assemblies.json has been generated in the specified folder.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Reset
            //btnGenerate.Enabled = false;
            //clbFiles.Items.Clear();
            //clbFiles.Enabled = false;
            //txtPath.Text = null;
            //assembliesPath = null;

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            string path = Path.Combine(assembliesPath, assemblyName.Name + ".dll");

            if (File.Exists(path))
            {
                return Assembly.LoadFile(path);
            }

            return null;
        }
    }
}