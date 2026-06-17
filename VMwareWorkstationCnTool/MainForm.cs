using System.Diagnostics;
using VMwareWorkstationCnTool.Core;

namespace VMwareWorkstationCnTool;

public sealed class MainForm : Form
{
    private readonly VMwareInstallationLocator locator = new();
    private readonly LocalizationService localizationService = LocalizationService.CreateDefault();
    private readonly VMwareReleaseService releaseService = new(new HttpClient());
    private readonly TextBox installPathTextBox = new();
    private readonly Label versionValueLabel = new();
    private readonly TextBox logTextBox = new();
    private readonly Button localizeButton = new();
    private readonly Button downloadLatestButton = new();
    private VMwareInstallationInfo currentInstallation = VMwareInstallationInfo.NotFound("尚未检测。");

    public MainForm()
    {
        Text = "VMware Workstation 一键汉化工具";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(680, 420);
        Size = new Size(760, 460);
        Font = new Font("Microsoft YaHei UI", 9F);

        BuildLayout();
        Load += (_, _) => DetectInstallation();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 1,
            RowCount = 5
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var titleLabel = new Label
        {
            Text = "VMware Workstation 一键汉化",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 16F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12)
        };

        var pathPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };
        pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var pathLabel = new Label
        {
            Text = "安装目录",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 10, 0)
        };
        installPathTextBox.Dock = DockStyle.Fill;
        installPathTextBox.ReadOnly = true;

        var chooseButton = new Button
        {
            Text = "选择...",
            AutoSize = true,
            Margin = new Padding(10, 0, 0, 0)
        };
        chooseButton.Click += (_, _) => ChooseInstallationPath();

        pathPanel.Controls.Add(pathLabel, 0, 0);
        pathPanel.Controls.Add(installPathTextBox, 1, 0);
        pathPanel.Controls.Add(chooseButton, 2, 0);

        var versionPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };
        versionPanel.Controls.Add(new Label { Text = "当前 VMware 版本：", AutoSize = true });
        versionValueLabel.AutoSize = true;
        versionValueLabel.Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold);
        versionPanel.Controls.Add(versionValueLabel);

        logTextBox.Dock = DockStyle.Fill;
        logTextBox.Multiline = true;
        logTextBox.ReadOnly = true;
        logTextBox.ScrollBars = ScrollBars.Vertical;
        logTextBox.BackColor = SystemColors.Window;
        logTextBox.Margin = new Padding(0, 0, 0, 14);

        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true
        };

        localizeButton.Text = "一键汉化";
        localizeButton.Width = 130;
        localizeButton.Height = 36;
        localizeButton.Click += (_, _) => Localize();

        downloadLatestButton.Text = "下载最新版";
        downloadLatestButton.Width = 120;
        downloadLatestButton.Height = 36;
        downloadLatestButton.Margin = new Padding(8, 0, 0, 0);
        downloadLatestButton.Click += async (_, _) => await DownloadLatestInstallerAsync();

        var openReleasesButton = new Button
        {
            Text = "打开下载页",
            Width = 120,
            Height = 36,
            Margin = new Padding(8, 0, 0, 0)
        };
        openReleasesButton.Click += (_, _) => OpenLatestDownloadPage();

        var refreshButton = new Button
        {
            Text = "重新检测",
            Width = 110,
            Height = 36,
            Margin = new Padding(8, 0, 0, 0)
        };
        refreshButton.Click += (_, _) => DetectInstallation();

        buttonPanel.Controls.Add(localizeButton);
        buttonPanel.Controls.Add(downloadLatestButton);
        buttonPanel.Controls.Add(openReleasesButton);
        buttonPanel.Controls.Add(refreshButton);

        root.Controls.Add(titleLabel);
        root.Controls.Add(pathPanel);
        root.Controls.Add(versionPanel);
        root.Controls.Add(logTextBox);
        root.Controls.Add(buttonPanel);
        Controls.Add(root);
    }

    private void DetectInstallation()
    {
        currentInstallation = locator.Find();
        ApplyInstallationInfo(currentInstallation);
    }

    private void ChooseInstallationPath()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "请选择 VMware Workstation 安装目录",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
            SelectedPath = Directory.Exists(installPathTextBox.Text) ? installPathTextBox.Text : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        currentInstallation = VMwareInstallationLocator.FromCustomPath(dialog.SelectedPath);
        ApplyInstallationInfo(currentInstallation);
    }

    private void ApplyInstallationInfo(VMwareInstallationInfo info)
    {
        installPathTextBox.Text = info.InstallPath;
        versionValueLabel.Text = info.IsFound ? info.Version : "未检测到";
        localizeButton.Enabled = info.IsFound;
        AppendLog(info.Message);
    }

    private void Localize()
    {
        if (!currentInstallation.IsFound)
        {
            MessageBox.Show(this, "请先选择有效的 VMware Workstation 安装目录。", "无法汉化", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            localizeButton.Enabled = false;
            AppendLog("正在写入语言文件和偏好设置...");
            var result = localizationService.Localize(currentInstallation.InstallPath);
            AppendLog(result.Message);

            var icon = result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
            MessageBox.Show(this, result.Message, result.Success ? "汉化完成" : "汉化失败", MessageBoxButtons.OK, icon);
        }
        catch (UnauthorizedAccessException)
        {
            var message = "没有写入安装目录的权限，请以管理员身份运行本工具。";
            AppendLog(message);
            MessageBox.Show(this, message, "权限不足", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            AppendLog(ex.Message);
            MessageBox.Show(this, ex.Message, "汉化失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            localizeButton.Enabled = currentInstallation.IsFound;
        }
    }

    private async Task DownloadLatestInstallerAsync()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "请选择最新版 VMware Workstation 安装包保存目录",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            SelectedPath = VMwareReleaseService.GetDefaultDownloadDirectory()
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            downloadLatestButton.Enabled = false;
            AppendLog("正在获取最新版 VMware Workstation 信息...");
            var result = await releaseService.DownloadLatestWindowsInstallerAsync(dialog.SelectedPath, CancellationToken.None);
            AppendLog($"已下载 {result.Release.TagName}：{result.FilePath}");

            var openFolder = MessageBox.Show(
                this,
                $"下载完成：{result.FilePath}{Environment.NewLine}是否打开所在文件夹？",
                "下载完成",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (openFolder == DialogResult.Yes)
            {
                OpenFileLocation(result.FilePath);
            }
        }
        catch (Exception ex)
        {
            AppendLog(ex.Message);
            MessageBox.Show(this, ex.Message, "下载失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            downloadLatestButton.Enabled = true;
        }
    }

    private void OpenLatestDownloadPage()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = VMwareReleaseService.ReleasesUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            AppendLog(ex.Message);
            MessageBox.Show(this, ex.Message, "无法打开下载页", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void OpenFileLocation(string filePath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{filePath}\"",
            UseShellExecute = true
        });
    }

    private void AppendLog(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
        logTextBox.AppendText(line);
        Debug.WriteLine(line);
    }
}
