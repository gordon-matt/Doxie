#nullable enable
using Doxie.Core.Services;
using Doxie.Core.Solutions;

namespace Doxie.HelpFileGenerator.GUI;

public sealed class MainForm : Form
{
    private readonly RadioButton solutionMode = new() { Text = "Solution", Checked = true, AutoSize = true };
    private readonly RadioButton folderMode = new() { Text = "Assembly folder", AutoSize = true };
    private readonly TextBox sourcePath = new() { Anchor = AnchorStyles.Left | AnchorStyles.Right };
    private readonly TextBox outputPath = new() { Anchor = AnchorStyles.Left | AnchorStyles.Right };
    private readonly ListView files = new()
    {
        CheckBoxes = true,
        FullRowSelect = true,
        GridLines = false,
        View = View.Details,
        HeaderStyle = ColumnHeaderStyle.Nonclickable,
        Dock = DockStyle.Fill
    };
    private readonly Label status = new() { AutoSize = true, ForeColor = Color.FromArgb(80, 90, 105), Text = "Choose a solution or a folder." };
    private readonly Button generate = new() { Text = "Generate", Enabled = false, AutoSize = false, Size = new Size(120, 34) };
    private readonly Label hint = new() { AutoSize = true, ForeColor = Color.FromArgb(80, 90, 105) };

    private bool loading;

    public MainForm()
    {
        Text = "Doxie";
        Font = new Font("Segoe UI", 9f);
        BackColor = Color.FromArgb(246, 247, 249);
        MinimumSize = new Size(860, 560);
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(920, 640);
        BuildLayout();
        solutionMode.CheckedChanged += (_, _) => ModeChanged();
        hint.Text = "Libraries in the solution are selected. Tests and applications are listed so you can add them.";
    }

    private void BuildLayout()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = Color.FromArgb(23, 32, 51), Padding = new Padding(20, 14, 20, 12) };
        header.Controls.Add(new Label
        {
            Text = "Create assemblies.json from your projects, or from assemblies you already built.",
            ForeColor = Color.FromArgb(186, 198, 216),
            AutoSize = true,
            Location = new Point(20, 42)
        });
        header.Controls.Add(new Label
        {
            Text = "Doxie",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(18, 10)
        });

        var source = new Panel { Dock = DockStyle.Top, Height = 132, Padding = new Padding(20, 12, 20, 8), BackColor = Color.FromArgb(246, 247, 249) };
        var sourceLabel = new Label { Text = "Source", AutoSize = true, Location = new Point(20, 12), Font = new Font(Font, FontStyle.Bold) };
        solutionMode.Location = new Point(20, 34);
        folderMode.Location = new Point(130, 34);
        sourcePath.Location = new Point(20, 64);
        sourcePath.Width = 760;
        var browseSource = new Button { Text = "Browse", Size = new Size(96, 27), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        browseSource.Click += (_, _) => BrowseSource();
        hint.Location = new Point(20, 96);
        source.Controls.Add(sourceLabel);
        source.Controls.Add(solutionMode);
        source.Controls.Add(folderMode);
        source.Controls.Add(sourcePath);
        source.Controls.Add(browseSource);
        source.Controls.Add(hint);
        source.Resize += (_, _) =>
        {
            browseSource.Left = source.ClientSize.Width - browseSource.Width - 20;
            sourcePath.Width = browseSource.Left - sourcePath.Left - 8;
        };

        var output = new Panel { Dock = DockStyle.Top, Height = 64, Padding = new Padding(20, 4, 20, 8), BackColor = Color.FromArgb(246, 247, 249) };
        output.Controls.Add(new Label { Text = "Output folder", AutoSize = true, Location = new Point(20, 4), Font = new Font(Font, FontStyle.Bold) });
        outputPath.Location = new Point(20, 26);
        outputPath.Width = 760;
        var browseOutput = new Button { Text = "Browse", Size = new Size(96, 27), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        browseOutput.Click += (_, _) => BrowseOutput();
        output.Controls.Add(outputPath);
        output.Controls.Add(browseOutput);
        output.Resize += (_, _) =>
        {
            browseOutput.Left = output.ClientSize.Width - browseOutput.Width - 20;
            outputPath.Width = browseOutput.Left - outputPath.Left - 8;
        };

        files.Columns.Add("Name", 280);
        files.Columns.Add("Kind", 110);
        files.Columns.Add("Framework", 110);
        files.Columns.Add("Documentation", 280);
        files.ItemChecked += (_, _) =>
        {
            if (!loading)
            {
                UpdateStatus();
            }
        };

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(20, 12, 20, 12), BackColor = Color.White };
        var selectAll = new Button { Text = "Select all", Size = new Size(96, 30), Location = new Point(20, 16) };
        var selectNone = new Button { Text = "Select none", Size = new Size(104, 30), Location = new Point(124, 16) };
        selectAll.Click += (_, _) => SetAll(true);
        selectNone.Click += (_, _) => SetAll(false);
        generate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        generate.FlatStyle = FlatStyle.Flat;
        generate.FlatAppearance.BorderSize = 0;
        generate.BackColor = Color.FromArgb(27, 79, 158);
        generate.ForeColor = Color.White;
        generate.Click += async (_, _) => await GenerateAsync();
        status.Location = new Point(244, 22);
        footer.Controls.Add(selectAll);
        footer.Controls.Add(selectNone);
        footer.Controls.Add(status);
        footer.Controls.Add(generate);
        footer.Resize += (_, _) => generate.Left = footer.ClientSize.Width - generate.Width - 20;

        var listHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 8), BackColor = Color.FromArgb(246, 247, 249) };
        listHost.Controls.Add(files);

        Controls.Add(listHost);
        Controls.Add(footer);
        Controls.Add(output);
        Controls.Add(source);
        Controls.Add(header);
    }

    private void ModeChanged()
    {
        hint.Text = solutionMode.Checked
            ? "Libraries in the solution are selected. Tests and applications are listed so you can add them."
            : "Every assembly in the folder is listed. Ones with XML documentation are selected.";
        sourcePath.Clear();
        outputPath.Clear();
        files.Items.Clear();
        UpdateStatus();
    }

    private void BrowseSource()
    {
        if (solutionMode.Checked)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Solution files (*.sln;*.slnx)|*.sln;*.slnx|All files (*.*)|*.*",
                Title = "Open a solution"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                LoadSolution(dialog.FileName);
            }

            return;
        }

        using var folder = new FolderBrowserDialog { Description = "Folder that contains the assemblies", UseDescriptionForTitle = true };
        if (folder.ShowDialog(this) == DialogResult.OK)
        {
            LoadFolder(folder.SelectedPath);
        }
    }

    private void BrowseOutput()
    {
        using var folder = new FolderBrowserDialog { Description = "Where assemblies.json should be written", UseDescriptionForTitle = true };
        if (!string.IsNullOrWhiteSpace(outputPath.Text) && Directory.Exists(outputPath.Text))
        {
            folder.SelectedPath = outputPath.Text;
        }

        if (folder.ShowDialog(this) == DialogResult.OK)
        {
            outputPath.Text = folder.SelectedPath;
        }
    }

    private void LoadSolution(string path)
    {
        sourcePath.Text = path;
        outputPath.Text = Path.GetDirectoryName(path) ?? string.Empty;
        try
        {
            List<SourceItem> items = ProjectInspector.InspectSolution(path)
                .Select(SourceItem.FromProject)
                .ToList();
            ShowItems(items);
            if (items.Count == 0)
            {
                status.Text = "That solution does not list any projects.";
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Could not read the solution", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadFolder(string path)
    {
        sourcePath.Text = path;
        outputPath.Text = path;
        List<SourceItem> items = Directory.EnumerateFiles(path, "*.dll")
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            .Select(SourceItem.FromAssembly)
            .ToList();
        ShowItems(items);
    }

    private void ShowItems(IReadOnlyList<SourceItem> items)
    {
        loading = true;
        files.BeginUpdate();
        files.Items.Clear();
        foreach (SourceItem item in items)
        {
            var row = new ListViewItem(item.Name) { Tag = item };
            row.SubItems.Add(item.Kind);
            row.SubItems.Add(item.Framework);
            row.SubItems.Add(item.Status);
            files.Items.Add(row);
            row.Checked = item.SuggestInclude;
        }

        files.EndUpdate();
        loading = false;
        UpdateStatus();
        generate.Enabled = files.Items.Count > 0;
    }

    private void SetAll(bool selected)
    {
        loading = true;
        foreach (ListViewItem item in files.Items)
        {
            item.Checked = selected;
        }

        loading = false;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        int selected = files.CheckedItems.Count;
        status.Text = selected == 1 ? "1 item selected." : $"{selected} items selected.";
        generate.Enabled = selected > 0 && !string.IsNullOrWhiteSpace(outputPath.Text);
    }

    private async Task GenerateAsync()
    {
        List<SourceItem> selected = files.CheckedItems.Cast<ListViewItem>().Select(item => (SourceItem)item.Tag!).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show(this, "Select at least one project or assembly.", "Doxie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string output = outputPath.Text.Trim();
        if (string.IsNullOrEmpty(output))
        {
            MessageBox.Show(this, "Choose a folder for assemblies.json.", "Doxie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        generate.Enabled = false;
        UseWaitCursor = true;
        try
        {
            if (solutionMode.Checked && selected.Any(item => item.AssemblyPath == null || !item.HasXmlDocumentation))
            {
                string detail = string.Join(Environment.NewLine, selected
                    .Where(item => item.AssemblyPath == null || !item.HasXmlDocumentation)
                    .Select(item => $"{item.Name}: {item.Status}"));
                DialogResult build = MessageBox.Show(
                    this,
                    "Some selected projects are not built, or have no XML documentation." + Environment.NewLine + Environment.NewLine + detail + Environment.NewLine + Environment.NewLine + "Build the solution in Release and turn on XML documentation?",
                    "Doxie",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                if (build == DialogResult.Cancel)
                {
                    return;
                }

                if (build == DialogResult.Yes)
                {
                    status.Text = "Building...";
                    string keepOutput = output;
                    BuildResult result = await Task.Run(() => DotNetBuilder.Build(sourcePath.Text));
                    if (!result.Succeeded)
                    {
                        string log = result.Log.Length > 2500 ? result.Log[^2500..] : result.Log;
                        MessageBox.Show(this, "The build failed." + Environment.NewLine + Environment.NewLine + log, "Doxie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    HashSet<string> keep = selected.Select(item => item.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    LoadSolution(sourcePath.Text);
                    outputPath.Text = keepOutput;
                    foreach (ListViewItem row in files.Items)
                    {
                        var item = (SourceItem)row.Tag!;
                        row.Checked = keep.Contains(item.Key);
                    }

                    selected = files.CheckedItems.Cast<ListViewItem>().Select(row => (SourceItem)row.Tag!).ToList();
                }
            }

            List<SourceItem> ready = selected.Where(item => !string.IsNullOrEmpty(item.AssemblyPath) && File.Exists(item.AssemblyPath)).ToList();
            if (ready.Count == 0)
            {
                MessageBox.Show(this, "None of the selected items have a built assembly yet.", "Doxie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Directory.CreateDirectory(output);
            status.Text = "Writing assemblies.json...";
            IReadOnlyList<string> probes = ready.SelectMany(item => item.ProbeDirectories).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            await Task.Run(() => JsonHelpFileGenerator.Generate(ready.Select(item => item.AssemblyPath!), output, probes));
            status.Text = $"Wrote {Path.Combine(output, "assemblies.json")}";
            MessageBox.Show(this, "assemblies.json has been written to the output folder.", "Doxie", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Doxie", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            UpdateStatus();
        }
    }

    private sealed class SourceItem
    {
        public required string Key { get; init; }

        public string? AssemblyPath { get; init; }

        public required string Name { get; init; }

        public required string Kind { get; init; }

        public string Framework { get; init; } = string.Empty;

        public required string Status { get; init; }

        public bool SuggestInclude { get; init; }

        public bool HasXmlDocumentation { get; init; }

        public IReadOnlyList<string> ProbeDirectories { get; init; } = [];

        public static SourceItem FromProject(DocumentationTarget project) => new()
        {
            Key = project.ProjectPath,
            AssemblyPath = project.AssemblyPath,
            Name = project.Name,
            Kind = project.IsTestProject ? "Test" : project.OutputType.Equals("Library", StringComparison.OrdinalIgnoreCase) ? "Library" : "Application",
            Framework = project.TargetFramework,
            Status = project.Status,
            SuggestInclude = project.SuggestInclude,
            HasXmlDocumentation = project.HasXmlDocumentation,
            ProbeDirectories = project.ProbeDirectories
        };

        public static SourceItem FromAssembly(string path)
        {
            bool hasXml = File.Exists(Path.ChangeExtension(path, ".xml"));
            return new SourceItem
            {
                Key = path,
                AssemblyPath = path,
                Name = Path.GetFileName(path),
                Kind = "Assembly",
                Status = hasXml ? "XML documentation found" : "No XML documentation",
                SuggestInclude = hasXml,
                HasXmlDocumentation = hasXml,
                ProbeDirectories = [Path.GetDirectoryName(path) ?? string.Empty]
            };
        }
    }
}
