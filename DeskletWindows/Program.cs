using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Desklet;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            ToastNotificationManagerCompat.OnActivated += args =>
            {
                var arg = (args.Argument ?? "").Split('=', 2).Last();
                if (arg.StartsWith("desklet-yes:", StringComparison.Ordinal))
                    ApprovalBroker.Resolve(arg["desklet-yes:".Length..], true);
                else if (arg.StartsWith("desklet-no:", StringComparison.Ordinal))
                    ApprovalBroker.Resolve(arg["desklet-no:".Length..], false);
            };
        }
        catch { }
        Application.Run(new DeskletWindow());
    }
}

internal sealed class AppSettings
{
    public List<string> ApprovedFolders { get; set; } = new();
    public Dictionary<string, string> Policies { get; set; } = new()
    {
        ["Sort new files by type"] = "Ask before",
        ["Create category folders"] = "Always without asking",
        ["Rename files"] = "Ask before",
        ["Move files"] = "Ask before",
        ["Copy files"] = "Ask before",
        ["Delete files"] = "Cannot do",
        ["Overwrite existing files"] = "Cannot do",
        ["Convert file formats"] = "Ask before",
        ["Bulk rename"] = "Ask before",
        ["Find duplicate files"] = "Always without asking"
    };
    public bool SortNewFilesEnabled { get; set; } = true;
    public bool StartWithWindows { get; set; } = false;
    public bool ReducedMotion { get; set; } = false;
    public bool NotificationsEnabled { get; set; } = true;
}

internal static class Store
{
    private static readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Desklet");
    private static readonly string FilePath = Path.Combine(Dir, "settings.json");
    public static AppSettings Load()
    {
        try { if (File.Exists(FilePath)) return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings(); } catch { }
        return new AppSettings();
    }
    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Dir);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }
}

internal sealed class PendingApproval
{
    public required string Id { get; init; }
    public required string Source { get; init; }
    public required string Destination { get; init; }
    public required Action<bool> Finish { get; init; }
}

internal static class ApprovalBroker
{
    private static readonly ConcurrentDictionary<string, PendingApproval> Pending = new();
    public static event Action<string, bool>? Resolved;
    public static string Add(PendingApproval item) { Pending[item.Id] = item; return item.Id; }
    public static void Resolve(string id, bool yes)
    {
        if (Pending.TryRemove(id, out var item))
        {
            try { item.Finish(yes); } catch { }
            Resolved?.Invoke(id, yes);
        }
    }
    public static void Notify(PendingApproval item)
    {
        Add(item);
        try
        {
            new ToastContentBuilder()
                .AddText("Desklet needs your approval")
                .AddText($"Sort {Path.GetFileName(item.Source)} into {Path.GetFileName(Path.GetDirectoryName(item.Destination) ?? "")}?")
                .AddButton(new ToastButton().SetContent("Yes, allow").AddArgument("approval", "desklet-yes:" + item.Id).SetActivationType(ToastActivationType.Foreground))
                .AddButton(new ToastButton().SetContent("No, keep it").AddArgument("approval", "desklet-no:" + item.Id).SetActivationType(ToastActivationType.Foreground))
                .Show();
        }
        catch
        {
            var result = MessageBox.Show($"Desklet wants to move this file:\n\n{item.Source}\n\nTo:\n{item.Destination}\n\nAllow this one action?", "Desklet approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            Resolve(item.Id, result == DialogResult.Yes);
        }
    }
}

internal sealed class Mascot : Control
{
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 450 };
    private int frame;
    public string Mood { get; set; } = "idle";
    public Mascot()
    {
        DoubleBuffered = true;
        Size = new Size(76, 76);
        timer.Tick += (_, _) => { frame = (frame + 1) % 4; Invalidate(); };
        timer.Start();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.None;
        var g = e.Graphics;
        int bounce = Mood == "working" ? frame % 2 : 0;
        using var shadow = new SolidBrush(Color.FromArgb(24, 44, 79));
        using var paper = new SolidBrush(Color.FromArgb(229, 239, 255));
        using var edge = new SolidBrush(Color.FromArgb(113, 158, 244));
        using var blue = new SolidBrush(Color.FromArgb(47, 108, 232));
        using var face = new SolidBrush(Color.FromArgb(18, 31, 54));
        using var white = new SolidBrush(Color.White);
        using var green = new SolidBrush(Color.FromArgb(51, 190, 132));
        int x = 20, y = 9 + bounce;
        g.FillRectangle(shadow, x - 3, y + 3, 40, 48);
        g.FillRectangle(edge, x, y, 38, 46);
        g.FillRectangle(paper, x + 3, y + 2, 33, 41);
        g.FillRectangle(blue, x + 8, y + 9, 19, 3);
        g.FillRectangle(blue, x + 8, y + 16, 23, 3);
        g.FillRectangle(blue, x + 8, y + 23, 20, 3);
        g.FillEllipse(face, x + 7, y + 26, 6, 7);
        g.FillEllipse(face, x + 25, y + 26, 6, 7);
        g.FillRectangle(face, x + 13, y + 36, 13, 3);
        g.FillEllipse(edge, x - 8, y + 25, 11, 11);
        g.FillEllipse(white, x - 6, y + 27, 7, 7);
        g.FillEllipse(edge, x + 35, y + 25, 11, 11);
        g.FillEllipse(white, x + 37, y + 27, 7, 7);
        g.FillRectangle(face, x + 7, y + 45, 7, 5);
        g.FillRectangle(face, x + 25, y + 45, 7, 5);
        if (Mood == "working") { g.FillRectangle(green, x + 28, y + 1, 8, 8); g.FillRectangle(white, x + 30, y + 4, 4, 2); }
        else if (Mood == "asking") { g.FillEllipse(blue, x + 29, y - 5, 10, 10); using var f = new Font("Segoe UI", 7, FontStyle.Bold); g.DrawString("?", f, white, x + 31, y - 5); }
        else if (Mood == "paused") { g.FillRectangle(blue, x + 28, y + 2, 3, 7); g.FillRectangle(blue, x + 33, y + 2, 3, 7); }
    }
}

internal sealed class DeskletWindow : Form
{
    private readonly AppSettings settings = Store.Load();
    private readonly Dictionary<string, FileSystemWatcher> watchers = new();
    private readonly ListBox activity = new() { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, IntegralHeight = false };
    private readonly FlowLayoutPanel content = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(24), BackColor = Color.FromArgb(247, 249, 253) };
    private readonly Label status = new() { AutoSize = true, ForeColor = Color.FromArgb(83, 105, 137), Text = "Paused · Choose an approved folder to begin" };
    private readonly Button startButton = new() { Text = "▶  Start Desklet", Width = 170, Height = 42, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(46, 102, 222), ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 10), Cursor = Cursors.Hand };
    private readonly FlowLayoutPanel nav = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(14, 12, 14, 12), BackColor = Color.FromArgb(18, 31, 54) };
    private string currentPage = "Overview";
    private bool running;
    private int filesProcessed;

    private static readonly string[] CapabilityNames =
    {
        "Sort new files by type", "Create category folders", "Rename files", "Move files", "Copy files",
        "Delete files", "Overwrite existing files", "Convert file formats", "Bulk rename", "Find duplicate files"
    };
    private static readonly string[] PolicyChoices = { "Cannot do", "Ask before", "Preview first", "Always without asking" };

    private void BeginInvoke(Action action) => base.BeginInvoke(action);

    public DeskletWindow()
    {
        Text = "Desklet";
        MinimumSize = new Size(980, 680);
        Size = new Size(1180, 780);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(247, 249, 253);
        Font = new Font("Segoe UI", 9.5f);
        BuildShell();
        ApprovalBroker.Resolved += (_, yes) => BeginInvoke(() => Log(yes ? "Approval accepted; action completed." : "Approval declined; file left unchanged."));
        FormClosing += (_, _) => StopWatching();
        ShowPage("Overview");
    }

    private void BuildShell()
    {
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.White };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 224));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(shell);
        var side = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 31, 54), Padding = new Padding(14) };
        shell.Controls.Add(side, 0, 0);
        var brand = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 94, WrapContents = false, BackColor = Color.Transparent, Padding = new Padding(2, 10, 0, 0) };
        brand.Controls.Add(new Mascot { Margin = new Padding(0, 0, 6, 0), Size = new Size(66, 66) });
        var brandText = new Panel { Width = 110, Height = 55, Margin = new Padding(0, 14, 0, 0) };
        brandText.Controls.Add(new Label { Text = "Desklet", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 19), AutoSize = true, Location = new Point(0, 0) });
        brandText.Controls.Add(new Label { Text = "FILE ASSISTANT", ForeColor = Color.FromArgb(154, 177, 211), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(2, 34) });
        brand.Controls.Add(brandText);
        side.Controls.Add(brand);
        nav.Height = 330;
        side.Controls.Add(nav);
        foreach (var page in new[] { "Overview", "Permissions", "Automation", "Activity", "Settings" })
        {
            var b = new Button { Text = "   " + page, TextAlign = ContentAlignment.MiddleLeft, Width = 190, Height = 44, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(220, 230, 247), BackColor = Color.FromArgb(18, 31, 54), Font = new Font("Segoe UI", 10), Cursor = Cursors.Hand, Margin = new Padding(0, 2, 0, 2) };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (_, _) => ShowPage(page);
            nav.Controls.Add(b);
        }
        var bottom = new Label { Dock = DockStyle.Bottom, Height = 50, ForeColor = Color.FromArgb(154, 177, 211), Text = "LOCAL-FIRST\nApproved folders only", TextAlign = ContentAlignment.MiddleLeft };
        side.Controls.Add(bottom);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Color.FromArgb(247, 249, 253) };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.Controls.Add(main, 1, 0);
        var top = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(24, 16, 24, 12) };
        main.Controls.Add(top, 0, 0);
        top.Controls.Add(status);
        status.Location = new Point(26, 48);
        startButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        startButton.Location = new Point(Width - 430, 20);
        startButton.Click += (_, _) => { if (running) StopWatching(); else StartWatching(); };
        startButton.FlatAppearance.BorderSize = 0;
        top.Resize += (_, _) => startButton.Location = new Point(top.ClientSize.Width - startButton.Width - 20, 20);
        top.Controls.Add(startButton);
        main.Controls.Add(content, 0, 1);
    }

    private void ShowPage(string page)
    {
        currentPage = page;
        content.SuspendLayout();
        content.Controls.Clear();
        if (page == "Overview") BuildOverview();
        else if (page == "Permissions") BuildPermissions();
        else if (page == "Automation") BuildAutomation();
        else if (page == "Activity") BuildActivity();
        else BuildSettings();
        content.ResumeLayout();
    }

    private Label Heading(string text, string sub = "")
    {
        var p = new Panel { Width = 860, Height = string.IsNullOrEmpty(sub) ? 48 : 72, Margin = new Padding(0, 0, 0, 12) };
        p.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI Semibold", 23), ForeColor = Color.FromArgb(24, 42, 69), AutoSize = true, Location = new Point(0, 0) });
        if (!string.IsNullOrEmpty(sub)) p.Controls.Add(new Label { Text = sub, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(94, 111, 139), AutoSize = true, Location = new Point(2, 42) });
        content.Controls.Add(p);
        return (Label)p.Controls[0];
    }

    private Panel Card(int width = 850)
    {
        var p = new Panel { Width = width, BackColor = Color.White, Padding = new Padding(18), Margin = new Padding(0, 0, 0, 14), BorderStyle = BorderStyle.FixedSingle };
        content.Controls.Add(p);
        return p;
    }

    private Button ActionButton(string text, Action action, bool primary = false)
    {
        var b = new Button { Text = text, AutoSize = true, Height = 36, Padding = new Padding(12, 3, 12, 3), FlatStyle = FlatStyle.Flat, BackColor = primary ? Color.FromArgb(46, 102, 222) : Color.White, ForeColor = primary ? Color.White : Color.FromArgb(37, 62, 97), Font = new Font("Segoe UI Semibold", 9.5f), Cursor = Cursors.Hand, Margin = new Padding(0, 4, 8, 4) };
        b.FlatAppearance.BorderColor = Color.FromArgb(215, 225, 240);
        b.Click += (_, _) => action();
        return b;
    }

    private void BuildOverview()
    {
        Heading("Your workspace", "Desklet watches approved folders and runs only the rules your permissions allow.");
        var hero = Card();
        hero.Height = 170;
        var mascot = new Mascot { Location = new Point(18, 18), Size = new Size(88, 88), Mood = running ? "working" : "idle" };
        hero.Controls.Add(mascot);
        hero.Controls.Add(new Label { Text = running ? "Desklet is working" : "Ready when you are", Font = new Font("Segoe UI Semibold", 17), ForeColor = Color.FromArgb(24, 42, 69), AutoSize = true, Location = new Point(126, 22) });
        hero.Controls.Add(new Label { Text = "When you press Start, enabled automations scan approved folders and watch for new files.\\nActions marked Ask before trigger a Windows notification with Yes / No.", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(94, 111, 139), AutoSize = true, Location = new Point(128, 55) });
        hero.Controls.Add(ActionButton("＋  Approve a folder", AddFolder, true));
        hero.Controls[2].Location = new Point(128, 108);
        var folders = Card(); folders.Height = 100;
        folders.Controls.Add(new Label { Text = $"Approved folders ({settings.ApprovedFolders.Count})", Font = new Font("Segoe UI Semibold", 12), AutoSize = true, Location = new Point(16, 12) });
        folders.Controls.Add(new Label { Text = settings.ApprovedFolders.Count == 0 ? "No folders approved yet. Choose a folder to begin." : string.Join("\\n", settings.ApprovedFolders.Take(3)), AutoSize = true, ForeColor = Color.FromArgb(94, 111, 139), Location = new Point(18, 42), MaximumSize = new Size(800, 60) });
        var stats = Card(); stats.Height = 88;
        stats.Controls.Add(new Label { Text = "FILES PROCESSED", ForeColor = Color.FromArgb(94, 111, 139), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(16, 12) });
        stats.Controls.Add(new Label { Text = filesProcessed.ToString(), ForeColor = Color.FromArgb(24, 42, 69), Font = new Font("Segoe UI Semibold", 23), AutoSize = true, Location = new Point(16, 30) });
        stats.Controls.Add(new Label { Text = "Sort new files by type is enabled by default; change its permission in Permissions.", ForeColor = Color.FromArgb(94, 111, 139), AutoSize = true, Location = new Point(160, 34) });
        var buttons = new FlowLayoutPanel { Width = 850, Height = 48, Margin = new Padding(0) };
        buttons.Controls.Add(ActionButton("Permissions", () => ShowPage("Permissions")));
        buttons.Controls.Add(ActionButton("Automation", () => ShowPage("Automation")));
        buttons.Controls.Add(ActionButton("View activity", () => ShowPage("Activity")));
        content.Controls.Add(buttons);
    }

    private void BuildPermissions()
    {
        Heading("Permissions", "Choose what Desklet may do. Changes are saved on this computer.");
        var intro = Card(); intro.Height = 75;
        intro.Controls.Add(new Label { Text = "Folder boundary", Font = new Font("Segoe UI Semibold", 11), AutoSize = true, Location = new Point(16, 10) });
        intro.Controls.Add(new Label { Text = "Desklet never works outside folders you approve. Destructive actions stay blocked by default.", AutoSize = true, ForeColor = Color.FromArgb(94, 111, 139), Location = new Point(16, 36) });
        foreach (var name in CapabilityNames)
        {
            var row = Card(); row.Height = 66;
            row.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI Semibold", 10), AutoSize = true, Location = new Point(16, 11) });
            var note = new Label { Text = name == "Sort new files by type" ? "Implemented · watches new files in approved folders" : name == "Find duplicate files" ? "Implemented · hash comparison in approved folders" : "Policy saved · advanced action not yet available", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(105, 121, 145), AutoSize = true, Location = new Point(16, 35) };
            row.Controls.Add(note);
            var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 210, Location = new Point(600, 16), Font = new Font("Segoe UI", 9.5f) };
            combo.Items.AddRange(PolicyChoices);
            var value = settings.Policies.TryGetValue(name, out var p) ? p : "Ask before";
            combo.SelectedItem = value;
            combo.SelectedIndexChanged += (_, _) => { settings.Policies[name] = combo.SelectedItem?.ToString() ?? "Ask before"; Store.Save(settings); };
            row.Controls.Add(combo);
        }
        var folderCard = Card(); folderCard.Height = 90;
        folderCard.Controls.Add(new Label { Text = "Approved folders", Font = new Font("Segoe UI Semibold", 11), AutoSize = true, Location = new Point(16, 12) });
        folderCard.Controls.Add(ActionButton("＋ Add folder", AddFolder, true));
        folderCard.Controls[1].Location = new Point(16, 40);
        folderCard.Controls.Add(ActionButton("Manage folders", () => ShowFolderManager()));
        folderCard.Controls[2].Location = new Point(142, 40);
    }

    private void BuildAutomation()
    {
        Heading("Automation", "Start Desklet to run enabled rules. Every action still passes through its permission policy.");
        var rule = Card(); rule.Height = 145;
        rule.Controls.Add(new Label { Text = "Sort new files by type", Font = new Font("Segoe UI Semibold", 14), AutoSize = true, Location = new Point(16, 12) });
        rule.Controls.Add(new Label { Text = "Moves new files into Documents, Images, Video, Audio, Archives or Other folders based on their extension.", AutoSize = true, ForeColor = Color.FromArgb(94, 111, 139), Location = new Point(16, 43), MaximumSize = new Size(790, 40) });
        var check = new CheckBox { Text = "Enable this automation", Checked = settings.SortNewFilesEnabled, AutoSize = true, Location = new Point(16, 94), Font = new Font("Segoe UI Semibold", 10) };
        check.CheckedChanged += (_, _) => { settings.SortNewFilesEnabled = check.Checked; Store.Save(settings); Log("Automation " + (check.Checked ? "enabled" : "disabled") + "."); };
        rule.Controls.Add(check);
        var duplicates = Card(); duplicates.Height = 120;
        duplicates.Controls.Add(new Label { Text = "Duplicate finder", Font = new Font("Segoe UI Semibold", 13), AutoSize = true, Location = new Point(16, 12) });
        duplicates.Controls.Add(new Label { Text = "Calculates SHA-256 hashes to identify files with identical content. It does not delete anything.", AutoSize = true, ForeColor = Color.FromArgb(94, 111, 139), Location = new Point(16, 42), MaximumSize = new Size(780, 40) });
        duplicates.Controls.Add(ActionButton("Scan approved folders", ScanDuplicates, true));
        duplicates.Controls[2].Location = new Point(16, 78);
        var start = Card(); start.Height = 80;
        start.Controls.Add(ActionButton(running ? "Stop watching" : "Start Desklet now", () => { if (running) StopWatching(); else StartWatching(); }, true));
        start.Controls.Add(new Label { Text = "Starts monitoring all approved folders and applies the policies above.", AutoSize = true, ForeColor = Color.FromArgb(94, 111, 139), Location = new Point(190, 23) });
    }

    private void BuildActivity()
    {
        Heading("Activity", "A local log of actions, approvals, skipped files and errors.");
        var card = Card(); card.Height = 420; card.Padding = new Padding(8);
        activity.BackColor = Color.White; activity.Font = new Font("Consolas", 9); card.Controls.Add(activity);
        if (activity.Items.Count == 0) activity.Items.Add("No activity yet.");
        content.Controls.Add(ActionButton("Clear activity", () => activity.Items.Clear()));
    }

    private void BuildSettings()
    {
        Heading("Settings", "Local storage, notifications and appearance.");
        var card = Card(); card.Height = 170;
        var n = new CheckBox { Text = "Show Windows approval notifications", Checked = settings.NotificationsEnabled, AutoSize = true, Location = new Point(16, 18) };
        n.CheckedChanged += (_, _) => { settings.NotificationsEnabled = n.Checked; Store.Save(settings); };
        var m = new CheckBox { Text = "Use static mascot (reduced motion)", Checked = settings.ReducedMotion, AutoSize = true, Location = new Point(16, 52) };
        m.CheckedChanged += (_, _) => { settings.ReducedMotion = m.Checked; Store.Save(settings); };
        card.Controls.Add(n); card.Controls.Add(m);
        card.Controls.Add(ActionButton("Open settings folder", () => { Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Desklet")); Process.Start("explorer.exe", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Desklet")); }));
        card.Controls[2].Location = new Point(16, 88);
        var info = Card(); info.Height = 100;
        info.Controls.Add(new Label { Text = "Desklet safety", Font = new Font("Segoe UI Semibold", 12), AutoSize = true, Location = new Point(16, 12) });
        info.Controls.Add(new Label { Text = "No shell commands are run. File actions stay inside approved folders. Deletion and overwrite are not implemented in this build.", AutoSize = true, ForeColor = Color.FromArgb(94, 111, 139), Location = new Point(16, 42), MaximumSize = new Size(800, 45) });
    }

    private void AddFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = "Choose a folder Desklet is allowed to work inside", UseDescriptionForTitle = true, ShowNewFolderButton = false };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var path = Path.GetFullPath(dialog.SelectedPath);
            if (!settings.ApprovedFolders.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                settings.ApprovedFolders.Add(path);
                Store.Save(settings);
                Log("Approved folder: " + path);
                ShowPage(currentPage);
            }
        }
    }

    private void ShowFolderManager()
    {
        using var form = new Form { Text = "Approved folders", Size = new Size(650, 380), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White };
        var list = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), DataSource = settings.ApprovedFolders.ToList() };
        var remove = new Button { Text = "Remove selected folder", Dock = DockStyle.Bottom, Height = 42 };
        remove.Click += (_, _) =>
        {
            if (list.SelectedItem is string path)
            {
                if (watchers.TryGetValue(path, out var w)) { w.EnableRaisingEvents = false; w.Dispose(); watchers.Remove(path); }
                settings.ApprovedFolders.Remove(path); Store.Save(settings); list.DataSource = null; list.DataSource = settings.ApprovedFolders.ToList();
            }
        };
        form.Controls.Add(list); form.Controls.Add(remove); form.ShowDialog(this); ShowPage(currentPage);
    }

    private void StartWatching()
    {
        if (settings.ApprovedFolders.Count == 0) { MessageBox.Show(this, "Approve at least one folder first.", "Desklet", MessageBoxButtons.OK, MessageBoxIcon.Information); AddFolder(); return; }
        running = true;
        foreach (var root in settings.ApprovedFolders.ToArray())
        {
            if (!Directory.Exists(root) || watchers.ContainsKey(root)) continue;
            try
            {
                var watcher = new FileSystemWatcher(root) { IncludeSubdirectories = true, NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.CreationTime, EnableRaisingEvents = false };
                watcher.Created += (_, e) => QueueSort(root, e.FullPath);
                watcher.Renamed += (_, e) => QueueSort(root, e.FullPath);
                watcher.Error += (_, e) => BeginInvoke(() => Log("Watcher warning: " + e.GetException().Message));
                watchers[root] = watcher;
                watcher.EnableRaisingEvents = true;
                Log("Watching approved folder: " + root);
                if (settings.SortNewFilesEnabled)
                    _ = Task.Run(async () => { foreach (var file in SafeFiles(root).Take(500)) { await Task.Delay(20); QueueSort(root, file); } });
            }
            catch (Exception ex) { Log("Could not watch " + root + ": " + ex.Message); }
        }
        startButton.Text = "■  Stop Desklet";
        status.Text = "Running · watching " + watchers.Count + " approved folder(s)";
        ShowPage(currentPage);
    }

    private void StopWatching()
    {
        running = false;
        foreach (var watcher in watchers.Values) { try { watcher.EnableRaisingEvents = false; watcher.Dispose(); } catch { } }
        watchers.Clear();
        startButton.Text = "▶  Start Desklet";
        status.Text = "Paused · no background actions are running";
        Log("Desklet paused.");
        if (!IsDisposed) ShowPage(currentPage);
    }

    private IEnumerable<string> SafeFiles(string root)
    {
        var stack = new Stack<string>(); stack.Push(root);
        while (stack.Count > 0)
        {
            var dir = stack.Pop();
            string[] dirs = Array.Empty<string>(), files = Array.Empty<string>();
            try { dirs = Directory.GetDirectories(dir); files = Directory.GetFiles(dir); } catch { }
            foreach (var d in dirs)
            {
                var name = Path.GetFileName(d);
                if (name.StartsWith(".") || name.Equals("System Volume Information", StringComparison.OrdinalIgnoreCase) || name.Equals("$RECYCLE.BIN", StringComparison.OrdinalIgnoreCase)) continue;
                stack.Push(d);
            }
            foreach (var f in files) yield return f;
        }
    }

    private void QueueSort(string root, string source)
    {
        if (!running || !settings.SortNewFilesEnabled || !File.Exists(source)) return;
        var policy = settings.Policies.TryGetValue("Sort new files by type", out var p) ? p : "Ask before";
        if (policy == "Cannot do") { BeginInvoke(() => Log("Blocked by policy: " + source)); return; }
        if (!IsInsideApprovedRoot(root, source)) { BeginInvoke(() => Log("Blocked outside approved folder: " + source)); return; }
        if (Path.GetFileName(source).StartsWith(".desklet-", StringComparison.OrdinalIgnoreCase)) return;
        _ = Task.Run(async () =>
        {
            await Task.Delay(800);
            if (!File.Exists(source) || !running) return;
            var category = CategoryFor(source);
            var folder = Path.Combine(Path.GetDirectoryName(source)!, category);
            var destination = Path.Combine(folder, Path.GetFileName(source));
            if (string.Equals(Path.GetDirectoryName(source), folder, StringComparison.OrdinalIgnoreCase)) return;
            if (File.Exists(destination)) destination = Path.Combine(folder, Path.GetFileNameWithoutExtension(source) + " (" + DateTime.Now.ToString("HHmmss") + ")" + Path.GetExtension(source));
            Action<bool> finish = yes =>
            {
                if (!yes) { BeginInvoke(() => Log("Declined: " + source)); return; }
                try
                {
                    if (!running || !File.Exists(source) || !IsInsideApprovedRoot(root, source)) return;
                    Directory.CreateDirectory(folder);
                    File.Move(source, destination);
                    Interlocked.Increment(ref filesProcessed);
                    BeginInvoke(() => { Log("Sorted: " + source + " → " + destination); if (currentPage == "Overview") ShowPage(currentPage); });
                }
                catch (Exception ex) { BeginInvoke(() => Log("Failed: " + source + " · " + ex.Message)); }
            };
            if (policy == "Always without asking")
            {
                finish(true);
            }
            else if (policy == "Preview first")
            {
                BeginInvoke(() =>
                {
                    var answer = MessageBox.Show(this, $"Move this file?\n\n{source}\n\nTo:\n{destination}", "Review file action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    finish(answer == DialogResult.Yes);
                });
            }
            else if (policy == "Ask before")
            {
                var item = new PendingApproval { Id = Guid.NewGuid().ToString("N"), Source = source, Destination = destination, Finish = finish };
                if (settings.NotificationsEnabled) ApprovalBroker.Notify(item);
                else BeginInvoke(() => { var answer = MessageBox.Show(this, $"Move this file?\n\n{source}\n\nTo:\n{destination}", "Desklet approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question); ApprovalBroker.Resolve(item.Id, answer == DialogResult.Yes); });
            }
        });
    }

    private static bool IsInsideApprovedRoot(string root, string path)
    {
        try
        {
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static string CategoryFor(string file)
    {
        var ext = Path.GetExtension(file).ToLowerInvariant();
        if (new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp", ".svg", ".ico", ".heic" }.Contains(ext)) return "Images";
        if (new[] { ".mp4", ".mov", ".mkv", ".avi", ".webm" }.Contains(ext)) return "Video";
        if (new[] { ".mp3", ".wav", ".flac", ".m4a", ".aac", ".ogg" }.Contains(ext)) return "Audio";
        if (new[] { ".zip", ".7z", ".rar", ".tar", ".gz" }.Contains(ext)) return "Archives";
        if (new[] { ".doc", ".docx", ".pdf", ".txt", ".rtf", ".odt", ".xls", ".xlsx", ".ppt", ".pptx", ".csv" }.Contains(ext)) return "Documents";
        return "Other";
    }

    private void ScanDuplicates()
    {
        if (settings.ApprovedFolders.Count == 0) { MessageBox.Show(this, "Approve a folder first."); return; }
        var found = new List<string>();
        var seen = new Dictionary<string, string>();
        foreach (var root in settings.ApprovedFolders)
        {
            foreach (var file in SafeFiles(root).Take(3000))
            {
                try
                {
                    using var stream = File.OpenRead(file);
                    var hash = Convert.ToHexString(SHA256.HashData(stream));
                    if (seen.TryGetValue(hash, out var other)) found.Add($"{file}  =  {other}");
                    else seen[hash] = file;
                }
                catch { }
            }
        }
        Log($"Duplicate scan finished. {found.Count} duplicate match(es) found.");
        MessageBox.Show(this, found.Count == 0 ? "No duplicate files found in the scanned approved folders." : string.Join(Environment.NewLine, found.Take(20)) + (found.Count > 20 ? "\n…and more" : ""), "Desklet duplicate scan", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void Log(string text)
    {
        if (IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(() => Log(text)); return; }
        if (activity.Items.Count == 1 && activity.Items[0]?.ToString() == "No activity yet.") activity.Items.Clear();
        activity.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {text}");
        while (activity.Items.Count > 500) activity.Items.RemoveAt(activity.Items.Count - 1);
    }
}