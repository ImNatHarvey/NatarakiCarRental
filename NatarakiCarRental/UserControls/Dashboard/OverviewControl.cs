using FontAwesome.Sharp;
using NatarakiCarRental.Helpers;
using NatarakiCarRental.Models;
using NatarakiCarRental.Services;
using NatarakiCarRental.UserControls.Cards;

namespace NatarakiCarRental.UserControls.Dashboard;

public sealed class OverviewControl : UserControl
{
    private readonly CarService _carService = new();
    private readonly CustomerService _customerService = new();
    private readonly MetricCardControl _totalCarsCard = new();
    private readonly MetricCardControl _availableCarsCard = new();
    private readonly MetricCardControl _activeCustomersCard = new();
    private readonly MetricCardControl _archivedCustomersCard = new();

    public OverviewControl()
    {
        InitializeControl();
        Load += OverviewControl_Load;
    }

    private void InitializeControl()
    {
        BackColor = ThemeHelper.ContentBackground;
        Dock = DockStyle.Fill;
        AutoScroll = true;
        Padding = new Padding(32);

        TableLayoutPanel mainLayout = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 5
        };

        Label titleLabel = new()
        {
            Text = "Overview",
            AutoSize = false,
            Height = 38,
            Dock = DockStyle.Top,
            Font = FontHelper.Title(22F),
            ForeColor = ThemeHelper.TextPrimary
        };

        Label subtitleLabel = new()
        {
            Text = "Monitor rentals, vehicles, customers, and recent activity.",
            AutoSize = false,
            Height = 30,
            Dock = DockStyle.Top,
            Font = FontHelper.Regular(10.5F),
            ForeColor = ThemeHelper.TextSecondary
        };

        TableLayoutPanel metricGrid = CreateMetricGrid();
        TableLayoutPanel lowerGrid = CreateLowerGrid();

        mainLayout.Controls.Add(titleLabel);
        mainLayout.Controls.Add(subtitleLabel);
        mainLayout.Controls.Add(metricGrid);
        mainLayout.Controls.Add(lowerGrid);

        Controls.Add(mainLayout);
    }

    private TableLayoutPanel CreateMetricGrid()
    {
        TableLayoutPanel grid = new()
        {
            Dock = DockStyle.Top,
            Height = 152,
            ColumnCount = 4,
            Padding = new Padding(0, 18, 0, 10)
        };

        for (int i = 0; i < 4; i++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        }

        AddMetricCard(grid, _totalCarsCard, IconChar.Car, "Total Cars", "0", "Registered active vehicles", ThemeHelper.Primary, 0);
        AddMetricCard(grid, _availableCarsCard, IconChar.CircleCheck, "Available Cars", "0", "Ready for rental", ThemeHelper.Success, 1);
        AddMetricCard(grid, _activeCustomersCard, IconChar.Users, "Active Customers", "0", "Ready for booking", ThemeHelper.Warning, 2);
        AddMetricCard(grid, _archivedCustomersCard, IconChar.BoxArchive, "Archived Customers", "0", "Hidden records", ThemeHelper.GrayIcon, 3);

        return grid;
    }

    private static void AddMetricCard(TableLayoutPanel grid, MetricCardControl card, IconChar icon, string title, string value, string helperText, Color iconColor, int column)
    {
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, column == 3 ? 0 : 14, 0);

        card.SetMetric(icon, title, value, helperText, iconColor);

        grid.Controls.Add(card, column, 0);
    }

    private static TableLayoutPanel CreateLowerGrid()
    {
        TableLayoutPanel grid = new()
        {
            Dock = DockStyle.Top,
            Height = 390,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(0, 22, 0, 0)
        };

        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F));

        Panel rentalsPanel = CreateTablePanel(
            "Recent Rentals",
            "Booking ID     Customer     Car     Status",
            "Transactions module not implemented yet.");

        Panel customersPanel = CreateTablePanel(
            "Recent Customers",
            "Customer     Contact     Status",
            "No customers yet.");

        Panel chartPanel = CreateChartPlaceholderPanel();

        rentalsPanel.Margin = new Padding(0, 0, 14, 14);
        customersPanel.Margin = new Padding(0, 0, 0, 14);
        chartPanel.Margin = new Padding(0, 0, 0, 0);

        grid.Controls.Add(rentalsPanel, 0, 0);
        grid.Controls.Add(customersPanel, 1, 0);
        grid.SetColumnSpan(chartPanel, 2);
        grid.Controls.Add(chartPanel, 0, 1);

        return grid;
    }

    private static Panel CreateTablePanel(string title, string header, string emptyMessage)
    {
        Panel panel = ControlFactory.CreateCardPanel(new Size(0, 0));
        panel.Dock = DockStyle.Fill;
        panel.Padding = new Padding(22);

        Label titleLabel = CreatePanelTitle(title);
        Label headerLabel = new()
        {
            Text = header,
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 34,
            Padding = new Padding(0, 10, 0, 0),
            Font = FontHelper.SemiBold(9F),
            ForeColor = ThemeHelper.TextPrimary
        };

        Label emptyLabel = new()
        {
            Text = emptyMessage,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = FontHelper.Regular(10F),
            ForeColor = ThemeHelper.TextSecondary,
            TextAlign = ContentAlignment.MiddleCenter
        };

        panel.Controls.Add(emptyLabel);
        panel.Controls.Add(headerLabel);
        panel.Controls.Add(titleLabel);

        return panel;
    }

    private static Panel CreateChartPlaceholderPanel()
    {
        Panel panel = ControlFactory.CreateCardPanel(new Size(0, 0));
        panel.Dock = DockStyle.Fill;
        panel.Padding = new Padding(22);

        Label titleLabel = CreatePanelTitle("Rental Overview");
        Label placeholderLabel = new()
        {
            Text = "Rental charts will appear once the Transactions module is implemented.",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = FontHelper.Regular(10F),
            ForeColor = ThemeHelper.TextSecondary,
            TextAlign = ContentAlignment.MiddleCenter
        };

        panel.Controls.Add(placeholderLabel);
        panel.Controls.Add(titleLabel);

        return panel;
    }

    private static Label CreatePanelTitle(string title)
    {
        return new Label
        {
            Text = title,
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 28,
            Font = FontHelper.Title(12F),
            ForeColor = ThemeHelper.TextPrimary
        };
    }

    private async void OverviewControl_Load(object? sender, EventArgs e)
    {
        Load -= OverviewControl_Load;

        try
        {
            CarCounts carCounts = await _carService.GetCarCountsAsync();
            CustomerCounts customerCounts = await _customerService.GetCustomerCountsAsync();

            _totalCarsCard.SetMetric(IconChar.Car, "Total Cars", carCounts.TotalCars.ToString(), "Registered active vehicles", ThemeHelper.Primary);
            _availableCarsCard.SetMetric(IconChar.CircleCheck, "Available Cars", carCounts.AvailableCars.ToString(), "Ready for rental", ThemeHelper.Success);
            _activeCustomersCard.SetMetric(IconChar.Users, "Active Customers", customerCounts.ActiveCustomers.ToString(), "Ready for booking", ThemeHelper.Warning);
            _archivedCustomersCard.SetMetric(IconChar.BoxArchive, "Archived Customers", customerCounts.ArchivedCustomers.ToString(), "Hidden records", ThemeHelper.GrayIcon);
        }
        catch (Exception exception)
        {
            MessageBoxHelper.ShowWarning($"Unable to load overview counts.\n\n{exception.Message}", "Overview");
        }
    }
}
