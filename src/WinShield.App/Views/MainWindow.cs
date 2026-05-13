using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using WinShield.App.ViewModels;
using WinShield.Domain;

namespace WinShield.App.Views;

public sealed class MainWindow : Window
{
    private readonly ContentControl _content = new();
    private readonly TextBlock _status = new();
    private readonly TextBlock _score = new();
    private readonly TextBlock _progress = new();
    private MainWindowViewModel? _vm;

    public MainWindow()
    {
        Title = "WinShield - Clean. Protect. Accelerate.";
        Width = 1240;
        Height = 820;
        MinWidth = 980;
        MinHeight = 680;
        Background = Brush("#071016");
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_vm is not null) _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = DataContext as MainWindowViewModel;
        if (_vm is null) return;
        _vm.PropertyChanged += OnVmPropertyChanged;
        Content = BuildShell(_vm);
        RenderModule();
    }

    private Control BuildShell(MainWindowViewModel vm)
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("260,*"),
            RowDefinitions = new RowDefinitions("*")
        };

        var sidebar = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(18),
            Background = Brush("#0B1820")
        };
        sidebar.Children.Add(new TextBlock
        {
            Text = "WinShield",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(8, 16, 8, 0)
        });
        sidebar.Children.Add(new TextBlock
        {
            Text = "Clean. Protect. Accelerate.",
            FontSize = 13,
            Foreground = Brush("#8FA4B2"),
            Margin = new Thickness(8, 0, 8, 22)
        });

        foreach (var module in vm.Modules)
        {
            var button = new Button
            {
                Content = module,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(18, 12),
                Margin = new Thickness(0, 1),
                Background = Brush(module == vm.SelectedModule ? "#1A7F64" : "#10222D"),
                Foreground = Brushes.White,
                BorderBrush = Brush("#173342"),
                CornerRadius = new CornerRadius(8)
            };
            button.Click += (_, _) =>
            {
                vm.SelectModuleCommand.Execute(module);
                RefreshNavigation(sidebar, vm);
            };
            sidebar.Children.Add(button);
        }

        Grid.SetColumn(sidebar, 0);
        root.Children.Add(sidebar);

        var page = new Grid
        {
            RowDefinitions = new RowDefinitions("96,*"),
            Margin = new Thickness(22, 18, 24, 22)
        };

        var top = new Grid { ColumnDefinitions = new ColumnDefinitions("*,220,220") };
        _status.Text = vm.ProtectionStatus;
        _status.FontSize = 30;
        _status.FontWeight = FontWeight.Bold;
        _status.Foreground = Brushes.White;
        top.Children.Add(new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "Premium Windows care suite", Foreground = Brush("#7E94A3"), FontSize = 14 },
                _status
            }
        });

        _score.Text = $"{vm.HealthScore}";
        _score.HorizontalAlignment = HorizontalAlignment.Center;
        _score.VerticalAlignment = VerticalAlignment.Center;
        _score.FontSize = 42;
        _score.FontWeight = FontWeight.Bold;
        _score.Foreground = Brush("#62E0B0");
        var scoreCard = Card(new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                new TextBlock { Text = "Health Score", Foreground = Brush("#8FA4B2"), HorizontalAlignment = HorizontalAlignment.Center },
                _score
            }
        });
        Grid.SetColumn(scoreCard, 1);
        top.Children.Add(scoreCard);

        _progress.Text = vm.ScanProgress;
        _progress.TextWrapping = TextWrapping.Wrap;
        _progress.Foreground = Brush("#A8BAC7");
        var progressCard = Card(_progress);
        Grid.SetColumn(progressCard, 2);
        top.Children.Add(progressCard);

        Grid.SetRow(top, 0);
        page.Children.Add(top);
        Grid.SetRow(_content, 1);
        page.Children.Add(_content);
        Grid.SetColumn(page, 1);
        root.Children.Add(page);
        return root;
    }

    private static void RefreshNavigation(StackPanel sidebar, MainWindowViewModel vm)
    {
        foreach (var button in sidebar.Children.OfType<Button>())
        {
            var module = button.Content?.ToString();
            button.Background = Brush(module == vm.SelectedModule ? "#1A7F64" : "#10222D");
        }
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_vm is null) return;
        if (e.PropertyName is nameof(MainWindowViewModel.SelectedModule)
            or nameof(MainWindowViewModel.ThreatCount)
            or nameof(MainWindowViewModel.ReclaimableBytes)
            or nameof(MainWindowViewModel.StartupCount)
            or nameof(MainWindowViewModel.PrivacyTraceCount))
        {
            RenderModule();
        }
        _status.Text = _vm.ProtectionStatus;
        _score.Text = _vm.HealthScore.ToString();
        _progress.Text = _vm.ScanProgress;
    }

    private void RenderModule()
    {
        if (_vm is null) return;
        _content.Content = _vm.SelectedModule switch
        {
            "Cleanup" => CleanupPage(_vm),
            "Protection" => ProtectionPage(_vm),
            "Privacy" => PrivacyPage(_vm),
            "Performance" => PerformancePage(_vm),
            "Applications" => PlaceholderPage("Applications", "Review installed applications, vendor trust, and future uninstall leftovers."),
            "Files" => FilesPage(_vm),
            "Reports" => ReportsPage(_vm),
            "Settings" => SettingsPage(_vm),
            _ => SmartScanPage(_vm)
        };
    }

    private static Control SmartScanPage(MainWindowViewModel vm)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("1.2*,*"), RowDefinitions = new RowDefinitions("*,220"), Margin = new Thickness(0, 12, 0, 0) };
        var hero = Card(new StackPanel
        {
            Spacing = 18,
            Children =
            {
                new TextBlock { Text = "Smart Scan", FontSize = 38, FontWeight = FontWeight.Bold, Foreground = Brushes.White },
                new TextBlock { Text = "One coordinated pass across junk files, privacy traces, startup risk, and local threat indicators.", TextWrapping = TextWrapping.Wrap, Foreground = Brush("#A8BAC7"), FontSize = 16 },
                ActionButton("Run Smart Scan", () => vm.RunSmartScanCommand.Execute(null), "#24B483"),
                ActionButton("Full Malware Scan", () => vm.RunSecurityScanCommand.Execute(null), "#3059D8")
            }
        });
        grid.Children.Add(hero);

        var metrics = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions = new RowDefinitions("*,*"),
            Margin = new Thickness(16, 0, 0, 0)
        };
        AddMetric(metrics, Metric("Threats", vm.ThreatCount.ToString(), "#FF6B6B"), 0, 0);
        AddMetric(metrics, Metric("Reclaimable", MainWindowViewModel.FormatBytes(vm.ReclaimableBytes), "#62E0B0"), 1, 0);
        AddMetric(metrics, Metric("Startup Apps", vm.StartupCount.ToString(), "#C9A7FF"), 0, 1);
        AddMetric(metrics, Metric("Privacy Traces", vm.PrivacyTraceCount.ToString(), "#FFD166"), 1, 1);
        Grid.SetColumn(metrics, 1);
        grid.Children.Add(metrics);

        var quick = Card(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                ActionButton("Deep Clean", () => vm.RunCleanupScanCommand.Execute(null), "#1F8A70"),
                ActionButton("Defender Quick Scan", () => vm.RunDefenderQuickScanCommand.Execute(null), "#2F6FED"),
                ActionButton("Review Startup Apps", () => vm.SelectModuleCommand.Execute("Performance"), "#6B5DD3")
            }
        });
        Grid.SetRow(quick, 1);
        Grid.SetColumnSpan(quick, 2);
        grid.Children.Add(quick);
        return grid;
    }

    private static Control CleanupPage(MainWindowViewModel vm) => ListPage(
        "Cleanup",
        "Safe, review-first cleanup inventory. Nothing is deleted without explicit confirmation.",
        ActionButton("Scan Junk", () => vm.RunCleanupScanCommand.Execute(null), "#24B483"),
        vm.CleanupItems,
        item => $"{item.Category} | {MainWindowViewModel.FormatBytes(item.SizeBytes)} | {item.Path}");

    private static Control ProtectionPage(MainWindowViewModel vm) => ListPage(
        "Protection",
        "Local rules, heuristics, Defender handoff, quarantine, and scan evidence.",
        new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                ActionButton("Local Security Scan", () => vm.RunSecurityScanCommand.Execute(null), "#24B483"),
                ActionButton("Defender Scan", () => vm.RunDefenderQuickScanCommand.Execute(null), "#2F6FED"),
                ActionButton("Refresh Quarantine", () => vm.RefreshQuarantineCommand.Execute(null), "#6B5DD3")
            }
        },
        vm.ThreatFindings,
        item => $"{item.Severity} {item.Category} | {item.Title} | {item.TargetPath}");

    private static Control PrivacyPage(MainWindowViewModel vm) => ListPage(
        "Privacy",
        "Browser cache indicators, recent traces, and reviewable privacy cleanup suggestions.",
        ActionButton("Run Smart Scan", () => vm.RunSmartScanCommand.Execute(null), "#24B483"),
        vm.PrivacyTraces,
        item => $"{item.Category} | {MainWindowViewModel.FormatBytes(item.SizeBytes)} | {item.Path}");

    private static Control PerformancePage(MainWindowViewModel vm) => ListPage(
        "Performance",
        "Startup entries with risk hints. Disable actions should be explicit and audited on Windows.",
        ActionButton("Refresh Startup", () => vm.RefreshStartupCommand.Execute(null), "#24B483"),
        vm.StartupEntries,
        item => $"{item.Risk} | {item.Name} | {item.Path} | {item.Hint}");

    private static Control FilesPage(MainWindowViewModel vm) => Card(new StackPanel
    {
        Spacing = 16,
        Children =
        {
            Header("Files", "Large file, duplicate file, old downloads, and disk usage workflows are implemented in the cleaning engine and ready for picker integration."),
            ActionButton("Run Cleanup Scan", () => vm.RunCleanupScanCommand.Execute(null), "#24B483"),
            new TextBlock { Text = "Duplicate detection uses size grouping followed by SHA-256 comparison and never auto-deletes.", Foreground = Brush("#A8BAC7"), TextWrapping = TextWrapping.Wrap }
        }
    });

    private static Control ReportsPage(MainWindowViewModel vm) => Card(new StackPanel
    {
        Spacing = 16,
        Children =
        {
            Header("Reports", "Export scan, cleanup, threat, and quarantine history for audit review."),
            ActionButton("Export JSON Report", () => vm.ExportReportCommand.Execute(null), "#24B483"),
            new ListBox
            {
                ItemsSource = vm.ScanHistory,
                ItemTemplate = new FuncDataTemplate<ScanSession>((item, _) => new TextBlock
                {
                    Text = item is null ? "" : $"{item.StartedAt:g} | {item.Type} | {item.Status} | {item.Findings.Count} findings",
                    Foreground = Brush("#D8E5EE")
                })
            }
        }
    });

    private static Control SettingsPage(MainWindowViewModel vm) => Card(new StackPanel
    {
        Spacing = 14,
        Children =
        {
            Header("Settings", "Product controls for theme, scan paths, real-time monitor, Defender visibility, cleanup safety, logs, and app data."),
            new CheckBox { Content = "Enable real-time Downloads monitor", Foreground = Brushes.White },
            new CheckBox { Content = "Show Microsoft Defender integration", Foreground = Brushes.White, IsChecked = true },
            new ComboBox { SelectedIndex = 0, ItemsSource = new[] { "Review-first cleanup", "Safe items only", "Manual selection" } },
            ActionButton("Export Logs", () => vm.ExportReportCommand.Execute(null), "#2F6FED")
        }
    });

    private static Control PlaceholderPage(string title, string body) => Card(new StackPanel
    {
        Children = { Header(title, body) }
    });

    private static Control ListPage<T>(string title, string subtitle, Control action, IEnumerable<T> source, Func<T, string> format) => Card(new DockPanel
    {
        Children =
        {
            DockTop(new StackPanel { Spacing = 14, Children = { Header(title, subtitle), action } }),
            new ListBox
            {
                ItemsSource = source,
                Margin = new Thickness(0, 18, 0, 0),
                ItemTemplate = new FuncDataTemplate<T>((item, _) => new TextBlock
                {
                    Text = item is null ? "" : format(item),
                    Foreground = Brush("#D8E5EE"),
                    TextWrapping = TextWrapping.NoWrap,
                    Margin = new Thickness(0, 6)
                })
            }
        }
    });

    private static Control DockTop(Control control)
    {
        DockPanel.SetDock(control, Dock.Top);
        return control;
    }

    private static TextBlock Header(string title, string subtitle) => new()
    {
        Text = $"{title}\n{subtitle}",
        FontSize = 22,
        FontWeight = FontWeight.SemiBold,
        Foreground = Brushes.White,
        TextWrapping = TextWrapping.Wrap
    };

    private static Button ActionButton(string text, Action action, string color)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(18, 12),
            Background = Brush(color),
            Foreground = Brushes.White,
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(8)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static Control Metric(string label, string value, string color) => Card(new StackPanel
    {
        Children =
        {
            new TextBlock { Text = label, Foreground = Brush("#8FA4B2"), FontSize = 14 },
            new TextBlock { Text = value, Foreground = Brush(color), FontSize = 28, FontWeight = FontWeight.Bold }
        }
    });

    private static void AddMetric(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }

    private static Border Card(Control child) => new()
    {
        Child = child,
        Padding = new Thickness(22),
        Margin = new Thickness(0, 0, 0, 16),
        Background = Brush("#0D1B24"),
        BorderBrush = Brush("#173342"),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(12)
    };

    private static IBrush Brush(string color) => new SolidColorBrush(Color.Parse(color));
}
