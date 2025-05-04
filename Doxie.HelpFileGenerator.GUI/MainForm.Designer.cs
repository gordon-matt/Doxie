namespace Doxie.HelpFileGenerator.GUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnBrowse = new Button();
            btnGenerate = new Button();
            dlgFolderBrowser = new FolderBrowserDialog();
            txtPath = new TextBox();
            lblPath = new Label();
            clbFiles = new CheckedListBox();
            SuspendLayout();
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(545, 27);
            btnBrowse.Margin = new Padding(4, 3, 4, 3);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(88, 27);
            btnBrowse.TabIndex = 0;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnGenerate
            // 
            btnGenerate.Enabled = false;
            btnGenerate.Location = new Point(545, 607);
            btnGenerate.Margin = new Padding(4, 3, 4, 3);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(88, 27);
            btnGenerate.TabIndex = 2;
            btnGenerate.Text = "Generate";
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // txtPath
            // 
            txtPath.Location = new Point(14, 29);
            txtPath.Margin = new Padding(4, 3, 4, 3);
            txtPath.Name = "txtPath";
            txtPath.Size = new Size(523, 23);
            txtPath.TabIndex = 3;
            // 
            // lblPath
            // 
            lblPath.AutoSize = true;
            lblPath.Location = new Point(14, 10);
            lblPath.Margin = new Padding(4, 0, 4, 0);
            lblPath.Name = "lblPath";
            lblPath.Size = new Size(493, 15);
            lblPath.TabIndex = 4;
            lblPath.Text = "Select a folder with .NET assemblies. Ensure the XML documentation files are present as well.";
            // 
            // clbFiles
            // 
            clbFiles.CheckOnClick = true;
            clbFiles.FormattingEnabled = true;
            clbFiles.Location = new Point(14, 59);
            clbFiles.Margin = new Padding(4, 3, 4, 3);
            clbFiles.Name = "clbFiles";
            clbFiles.Size = new Size(618, 526);
            clbFiles.TabIndex = 5;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(645, 642);
            Controls.Add(clbFiles);
            Controls.Add(lblPath);
            Controls.Add(txtPath);
            Controls.Add(btnGenerate);
            Controls.Add(btnBrowse);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Doxie Help File Generator";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.FolderBrowserDialog dlgFolderBrowser;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.CheckedListBox clbFiles;
    }
}

