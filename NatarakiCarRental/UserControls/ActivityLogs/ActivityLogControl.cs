using FontAwesome.Sharp;
using NatarakiCarRental.Helpers;
using NatarakiCarRental.Models;
using NatarakiCarRental.Services;
using NatarakiCarRental.UserControls.Cards;
using NatarakiCarRental.UserControls.Common;

namespace NatarakiCarRental.UserControls.ActivityLogs;

public sealed class ActivityLogControl : UserControl
{
    private const int MaxRows = 100;
    private readonly ActivityLogService _activityLogService = new();
    private readonly MetricCardControl _totalLogsCard = new();
    private readonly MetricCardControl _todaysLogsCard = new();
    private readonly MetricCardControl _carActionsCard = new();
    private readonly MetricCardControl _customerActionsCard = new();
    private readonly TextBox _searchTextBox = new();
    private readonly ComboBox _actionTypeComboBox = new();
    private readonly ComboBox _entityTypeComboBox = new();
    private readonly DataGridView _logsGrid = new();
    private readonly Label _emptyStateLabel = new();
    private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 350 };
    private bool _isInitializingFilters;

    public ActivityLogControl()
    {
        InitializeControl();
        Load += ActivityLogControl_Load;
    }

    private void InitializeControl()
    {
        BackColor = ThemeHelper.ContentBackground;
        Dock = DockStyle.Fill;
        Padding = new Padding(32);

        TableLayoutPanel mainLayout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 148F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        mainLayout.Controls.Add(CreateHeaderPanel(), 0, 0);
        mainLayout.Controls.Add(CreateMetricGrid(), 0, 1);
        mainLayout.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = ThemeHelper.ContentBackground }, 0, 2);
        mainLayout.Controls.Add(CreateSearchPanel(), 0, 3);
        mainLayout.Controls.Add(CreateTablePanel(), 0, 4);

        Controls.Add(mainLayout);
    }

    private static Panel CreateHeaderPanel()
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeHelper.ContentBackground
        };

        Label titleLabel = new()
        {
            Text = "Activity Log",
            AutoSize = false,
            Location = new Point(0, 0),
            Size = new Size(260, 34),
            Font = FontHelper.Title(22F),
            ForeColor = ThemeHelper.TextPrimary
        };

        Label subtitleLabel = new()
        {
            Text = "Monitor recent system actions and record changes.",
            AutoSize = false,
            Location = new Point(2, 42),
            Size = new Size(560, 24),
            Font = FontHelper.Regular(10.5F),
            ForeColor = ThemeHelper.TextSecondary
        };

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(subtitleLabel);
        return panel;
    }

    private TableLayoutPanel CreateMetricGrid()
    {
        TableLayoutPanel grid = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            Padding = new Padding(0, 12, 0, 8)
        };

        for (int i = 0; i < 4; i++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        }

        AddMetricCard(grid, _totalLogsCard, IconChar.ClipboardList, "Total Logs", 0, "All recorded actions", ThemeHelper.Primary);
        AddMetricCard(grid, _todaysLogsCard, IconChar.CalendarDay, "Today's Logs", 1, "Recorded today", ThemeHelper.Success);
        AddMetricCard(grid, _carActionsCard, IconChar.Car, "Car Actions", 2, "Vehicle record changes", ThemeHelper.Warning);
        AddMetricCard(grid, _customerActionsCard, IconChar.Users, "Customer Actions", 3, "Customer record changes", ThemeHelper.Purple);
        return grid;
    }

    private static void AddMetricCard(TableLayoutPanel grid, MetricCardControl card, IconChar icon, string title, int column, string helperText, Color iconColor)
    {
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, column == 3 ? 0 : 14, 0);
        card.SetMetric(icon, title, "0", helperText, iconColor);
        grid.Controls.Add(card, column, 0);
    }

    private Panel CreateSearchPanel()
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeHelper.ContentBackground
        };

        BorderedPanel searchContainer = new()
        {
            Size = new Size(360, 32),
            Location = new Point(0, 8),
            BackColor = ThemeHelper.Surface,
            BorderColor = ThemeHelper.Border,
            Cursor = Cursors.IBeam
        };

        IconPictureBox searchIcon = new()
        {
            IconChar = IconChar.MagnifyingGlass,
            IconColor = ThemeHelper.TextSecondary,
            IconSize = 18,
            BackColor = ThemeHelper.Surface,
            Location = new Point(8, 7),
            Size = new Size(20, 20)
        };

        _searchTextBox.BorderStyle = BorderStyle.None;
        _searchTextBox.PlaceholderText = "Search by user, action, entity, or details...";
        _searchTextBox.BackColor = ThemeHelper.Surface;
        _searchTextBox.Font = FontHelper.Regular(10F);
        _searchTextBox.ForeColor = ThemeHelper.TextPrimary;
        _searchTextBox.Location = new Point(34, 7);
        _searchTextBox.Width = 316;
        _searchTextBox.TextChanged += (_, _) =>
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        };

        searchContainer.Controls.Add(searchIcon);
        searchContainer.Controls.Add(_searchTextBox);
        searchContainer.Click += (_, _) => _searchTextBox.Focus();

        ConfigureFilterComboBox(_actionTypeComboBox, new Point(376, 8));
        ConfigureFilterComboBox(_entityTypeComboBox, new Point(568, 8));
        _actionTypeComboBox.SelectedIndexChanged += async (_, _) =>
        {
            if (!_isInitializingFilters)
            {
                await LoadLogsAsync();
            }
        };
        _entityTypeComboBox.SelectedIndexChanged += async (_, _) =>
        {
            if (!_isInitializingFilters)
            {
                await LoadLogsAsync();
            }
        };
        _searchTimer.Tick += async (_, _) =>
        {
            _searchTimer.Stop();
            await LoadLogsAsync();
        };

        panel.Controls.Add(searchContainer);
        panel.Controls.Add(_actionTypeComboBox);
        panel.Controls.Add(_entityTypeComboBox);
        return panel;
    }

    private static void ConfigureFilterComboBox(ComboBox comboBox, Point location)
    {
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Font = FontHelper.Regular(10F);
        comboBox.ForeColor = ThemeHelper.TextPrimary;
        comboBox.Size = new Size(176, 30);
        comboBox.Location = location;
    }

    private Panel CreateTablePanel()
    {
        Panel panel = ControlFactory.CreateCardPanel(new Size(0, 0));
        panel.Dock = DockStyle.Fill;
        panel.Padding = new Padding(18);

        _logsGrid.Dock = DockStyle.Fill;
        _logsGrid.AllowUserToAddRows = false;
        _logsGrid.AllowUserToDeleteRows = false;
        _logsGrid.AllowUserToResizeRows = false;
        _logsGrid.AllowUserToResizeColumns = false;
        _logsGrid.ScrollBars = ScrollBars.Vertical;
        _logsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _logsGrid.BackgroundColor = ThemeHelper.Surface;
        _logsGrid.BorderStyle = BorderStyle.None;
        _logsGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _logsGrid.ColumnHeadersHeight = 38;
        _logsGrid.EnableHeadersVisualStyles = false;
        _logsGrid.GridColor = ThemeHelper.Border;
        _logsGrid.ReadOnly = true;
        _logsGrid.RowHeadersVisible = false;
        _logsGrid.RowTemplate.Height = 38;
        _logsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _logsGrid.DefaultCellStyle.SelectionBackColor = ThemeHelper.Surface;
        _logsGrid.DefaultCellStyle.SelectionForeColor = ThemeHelper.TextPrimary;
        _logsGrid.ColumnHeadersDefaultCellStyle.BackColor = ThemeHelper.Primary;
        _logsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _logsGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeHelper.Primary;
        _logsGrid.ColumnHeadersDefaultCellStyle.Font = FontHelper.SemiBold(9F);
        _logsGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

        _emptyStateLabel.Text = "No activity logs found.";
        _emptyStateLabel.Dock = DockStyle.Bottom;
        _emptyStateLabel.Height = 42;
        _emptyStateLabel.Font = FontHelper.Regular(10F);
        _emptyStateLabel.ForeColor = ThemeHelper.TextSecondary;
        _emptyStateLabel.TextAlign = ContentAlignment.MiddleCenter;
        _emptyStateLabel.Visible = false;

        panel.Controls.Add(_logsGrid);
        panel.Controls.Add(_emptyStateLabel);
        return panel;
    }

    private async void ActivityLogControl_Load(object? sender, EventArgs e)
    {
        Load -= ActivityLogControl_Load;
        await InitializeFiltersAsync();
        await LoadLogsAsync();
    }

    private async Task InitializeFiltersAsync()
    {
        try
        {
            _isInitializingFilters = true;
            IReadOnlyList<string> actionTypes = await _activityLogService.GetActionTypesAsync();
            IReadOnlyList<string> entityNames = await _activityLogService.GetEntityNamesAsync();
            SetFilterItems(_actionTypeComboBox, "All Actions", actionTypes);
            SetFilterItems(_entityTypeComboBox, "All Entities", entityNames);
        }
        catch (Exception exception)
        {
            MessageBoxHelper.ShowWarning($"Unable to load activity log filters.\n\n{exception.Message}", "Activity Log");
            SetFilterItems(_actionTypeComboBox, "All Actions", []);
            SetFilterItems(_entityTypeComboBox, "All Entities", []);
        }
        finally
        {
            _isInitializingFilters = false;
        }
    }

    private async Task LoadLogsAsync()
    {
        try
        {
            ActivityLogMetrics metrics = await _activityLogService.GetMetricsAsync();
            UpdateMetricCards(metrics);

            IReadOnlyList<ActivityLog> logs = await _activityLogService.SearchLogsAsync(
                _searchTextBox.Text,
                GetSelectedFilter(_actionTypeComboBox),
                GetSelectedFilter(_entityTypeComboBox),
                MaxRows);
            PopulateGrid(logs);
        }
        catch (Exception exception)
        {
            MessageBoxHelper.ShowError($"Unable to load activity logs.\n\n{exception.Message}", "Activity Log");
        }
    }

    private void UpdateMetricCards(ActivityLogMetrics metrics)
    {
        _totalLogsCard.SetMetric(IconChar.ClipboardList, "Total Logs", metrics.TotalLogs.ToString(), "All recorded actions", ThemeHelper.Primary);
        _todaysLogsCard.SetMetric(IconChar.CalendarDay, "Today's Logs", metrics.TodaysLogs.ToString(), "Recorded today", ThemeHelper.Success);
        _carActionsCard.SetMetric(IconChar.Car, "Car Actions", metrics.CarActions.ToString(), "Vehicle record changes", ThemeHelper.Warning);
        _customerActionsCard.SetMetric(IconChar.Users, "Customer Actions", metrics.CustomerActions.ToString(), "Customer record changes", ThemeHelper.Purple);
    }

    private void PopulateGrid(IReadOnlyList<ActivityLog> logs)
    {
        AddGridColumns();
        _logsGrid.Rows.Clear();

        foreach (ActivityLog log in logs)
        {
            _logsGrid.Rows.Add(
                log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                log.UserDisplayName,
                log.ActionType,
                string.IsNullOrWhiteSpace(log.EntityName) ? "-" : log.EntityName,
                log.EntityId?.ToString() ?? "-",
                log.Description);
        }

        _emptyStateLabel.Visible = logs.Count == 0;
    }

    private void AddGridColumns()
    {
        _logsGrid.Columns.Clear();
        _logsGrid.Columns.Add("CreatedAt", "Date/Time");
        _logsGrid.Columns.Add("User", "User");
        _logsGrid.Columns.Add("Action", "Action");
        _logsGrid.Columns.Add("EntityName", "Entity Type");
        _logsGrid.Columns.Add("EntityId", "Entity ID");
        _logsGrid.Columns.Add("Description", "Details");

        SetFillWeight("CreatedAt", 84);
        SetFillWeight("User", 84);
        SetFillWeight("Action", 92);
        SetFillWeight("EntityName", 70);
        SetFillWeight("EntityId", 54);
        SetFillWeight("Description", 180);
    }

    private void SetFillWeight(string columnName, float weight)
    {
        if (_logsGrid.Columns[columnName] is DataGridViewColumn column)
        {
            column.FillWeight = weight;
        }
    }

    private static void SetFilterItems(ComboBox comboBox, string placeholder, IEnumerable<string> values)
    {
        comboBox.BeginUpdate();
        comboBox.Items.Clear();
        comboBox.Items.Add(placeholder);
        comboBox.Items.AddRange(values.Cast<object>().ToArray());
        comboBox.SelectedIndex = 0;
        comboBox.EndUpdate();
    }

    private static string? GetSelectedFilter(ComboBox comboBox)
    {
        return comboBox.SelectedIndex <= 0 ? null : comboBox.SelectedItem?.ToString();
    }
}
