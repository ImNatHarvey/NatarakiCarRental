using FontAwesome.Sharp;
using NatarakiCarRental.Helpers;
using NatarakiCarRental.UserControls.Cards;

namespace NatarakiCarRental.UserControls.Dashboard;

public sealed class OverviewControl : UserControl
{
    public OverviewControl()
    {
        InitializeControl();
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

    private static TableLayoutPanel CreateMetricGrid()
    {
        TableLayoutPanel grid = new()
        {
            Dock = DockStyle.Top,
            Height = 148,
            ColumnCount = 4,
            Padding = new Padding(0, 18, 0, 6)
        };

        for (int i = 0; i < 4; i++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        }

        grid.Controls.Add(CreateMetricCard(IconChar.Car, "Total Cars", "0", "Registered vehicles"), 0, 0);
        grid.Controls.Add(CreateMetricCard(IconChar.CircleCheck, "Available Cars", "0", "Ready for rental"), 1, 0);
        grid.Controls.Add(CreateMetricCard(IconChar.Key, "Active Rentals", "0", "Currently booked"), 2, 0);
        grid.Controls.Add(CreateMetricCard(IconChar.PesoSign, "Monthly Revenue", "\u20b10.00", "Current month"), 3, 0);

        return grid;
    }

    private static MetricCardControl CreateMetricCard(IconChar icon, string title, string value, string helperText)
    {
        MetricCardControl card = new()
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 14, 8)
        };

        card.SetMetric(icon, title, value, helperText);

        return card;
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
            "No recent rentals yet.");

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
            Text = "Charts will appear here once transaction data is available.",
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
}
