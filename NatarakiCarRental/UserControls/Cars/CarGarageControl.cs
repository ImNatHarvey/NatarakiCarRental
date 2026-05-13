using FontAwesome.Sharp;
using NatarakiCarRental.Helpers;
using NatarakiCarRental.Models;
using NatarakiCarRental.Services;
using NatarakiCarRental.UserControls.Cards;

namespace NatarakiCarRental.UserControls.Cars;

public sealed class CarGarageControl : UserControl
{
    private readonly CarService _carService = new();
    private readonly MetricCardControl _totalCarsCard = new();
    private readonly MetricCardControl _availableCarsCard = new();
    private readonly MetricCardControl _rentedCarsCard = new();
    private readonly MetricCardControl _archivedCarsCard = new();
    private readonly TextBox _searchTextBox = ControlFactory.CreateTextBox(340);
    private readonly Button _activeCarsButton = new();
    private readonly Button _archivedCarsButton = new();
    private readonly DataGridView _carsGrid = new();
    private readonly Label _emptyStateLabel = new();

    private bool _showArchived;

    public CarGarageControl()
    {
        InitializeControl();
        _ = LoadCarsAsync();
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
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        mainLayout.Controls.Add(CreateHeaderPanel(), 0, 0);
        mainLayout.Controls.Add(CreateMetricGrid(), 0, 1);
        mainLayout.Controls.Add(CreateToolbarPanel(), 0, 2);
        mainLayout.Controls.Add(CreateTabsPanel(), 0, 3);
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

        IconPictureBox icon = new()
        {
            IconChar = IconChar.Car,
            IconColor = ThemeHelper.Primary,
            IconSize = 28,
            BackColor = ThemeHelper.ContentBackground,
            Location = new Point(0, 2),
            Size = new Size(34, 34)
        };

        Label titleLabel = new()
        {
            Text = "Car Garage",
            AutoSize = false,
            Location = new Point(42, 0),
            Size = new Size(260, 34),
            Font = FontHelper.Title(22F),
            ForeColor = ThemeHelper.TextPrimary
        };

        Label subtitleLabel = new()
        {
            Text = "Manage vehicle records, availability, and archived cars.",
            AutoSize = false,
            Location = new Point(2, 42),
            Size = new Size(520, 24),
            Font = FontHelper.Regular(10.5F),
            ForeColor = ThemeHelper.TextSecondary
        };

        panel.Controls.Add(icon);
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

        AddMetricCard(grid, _totalCarsCard, IconChar.Car, "Total Cars", 0, "All active vehicles");
        AddMetricCard(grid, _availableCarsCard, IconChar.CircleCheck, "Available Cars", 1, "Ready for rental");
        AddMetricCard(grid, _rentedCarsCard, IconChar.Key, "Rented Cars", 2, "Currently rented");
        AddMetricCard(grid, _archivedCarsCard, IconChar.BoxArchive, "Archived Cars", 3, "Hidden from active list");

        return grid;
    }

    private static void AddMetricCard(TableLayoutPanel grid, MetricCardControl card, IconChar icon, string title, int column, string helperText)
    {
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, column == 3 ? 0 : 14, 0);
        card.SetMetric(icon, title, "0", helperText);
        grid.Controls.Add(card, column, 0);
    }

    private Panel CreateToolbarPanel()
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeHelper.ContentBackground
        };

        _searchTextBox.PlaceholderText = "Search by car name, model, or plate number";
        _searchTextBox.Location = new Point(0, 12);
        _searchTextBox.Width = 380;
        _searchTextBox.TextChanged += async (_, _) => await LoadCarsAsync();

        Button addCarButton = ControlFactory.CreatePrimaryButton("Add Car", 118, 36);
        addCarButton.Location = new Point(394, 10);
        addCarButton.TextImageRelation = TextImageRelation.ImageBeforeText;
        addCarButton.Click += (_, _) => MessageBoxHelper.ShowInfo("The Add Car form will be implemented next.");

        panel.Controls.Add(_searchTextBox);
        panel.Controls.Add(addCarButton);

        return panel;
    }

    private Panel CreateTabsPanel()
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeHelper.ContentBackground
        };

        ConfigureTabButton(_activeCarsButton, "Active Cars", new Point(0, 6));
        ConfigureTabButton(_archivedCarsButton, "Archived Cars", new Point(116, 6));

        _activeCarsButton.Click += async (_, _) =>
        {
            _showArchived = false;
            await LoadCarsAsync();
        };

        _archivedCarsButton.Click += async (_, _) =>
        {
            _showArchived = true;
            await LoadCarsAsync();
        };

        panel.Controls.Add(_activeCarsButton);
        panel.Controls.Add(_archivedCarsButton);

        return panel;
    }

    private static void ConfigureTabButton(Button button, string text, Point location)
    {
        button.Text = text;
        button.Location = location;
        button.Size = new Size(104, 34);
        button.FlatStyle = FlatStyle.Flat;
        button.Cursor = Cursors.Hand;
        button.Font = FontHelper.SemiBold(9F);
        button.FlatAppearance.BorderSize = 0;
    }

    private Panel CreateTablePanel()
    {
        Panel panel = ControlFactory.CreateCardPanel(new Size(0, 0));
        panel.Dock = DockStyle.Fill;
        panel.Padding = new Padding(18);

        _carsGrid.Dock = DockStyle.Fill;
        _carsGrid.AllowUserToAddRows = false;
        _carsGrid.AllowUserToDeleteRows = false;
        _carsGrid.AllowUserToResizeRows = false;
        _carsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _carsGrid.BackgroundColor = ThemeHelper.Surface;
        _carsGrid.BorderStyle = BorderStyle.None;
        _carsGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _carsGrid.ColumnHeadersHeight = 38;
        _carsGrid.EnableHeadersVisualStyles = false;
        _carsGrid.GridColor = ThemeHelper.Border;
        _carsGrid.ReadOnly = true;
        _carsGrid.RowHeadersVisible = false;
        _carsGrid.RowTemplate.Height = 38;
        _carsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _carsGrid.CellContentClick += CarsGrid_CellContentClick;

        AddGridColumns();

        _emptyStateLabel.Text = "No car records found.";
        _emptyStateLabel.Dock = DockStyle.Bottom;
        _emptyStateLabel.Height = 42;
        _emptyStateLabel.Font = FontHelper.Regular(10F);
        _emptyStateLabel.ForeColor = ThemeHelper.TextSecondary;
        _emptyStateLabel.TextAlign = ContentAlignment.MiddleCenter;
        _emptyStateLabel.Visible = false;

        panel.Controls.Add(_carsGrid);
        panel.Controls.Add(_emptyStateLabel);

        return panel;
    }

    private void AddGridColumns()
    {
        _carsGrid.Columns.Clear();
        _carsGrid.Columns.Add("CarId", "Car ID");
        _carsGrid.Columns.Add("CarName", "Car Name");
        _carsGrid.Columns.Add("Model", "Model");
        _carsGrid.Columns.Add("PlateNumber", "Plate Number");
        _carsGrid.Columns.Add("RatePerDay", "Rate/Day");
        _carsGrid.Columns.Add("Status", "Status");

        AddActionColumn("ViewAction", "View", "View");
        AddActionColumn("EditAction", "Edit", "Edit");
        AddActionColumn("ArchiveAction", "Archive", "Archive");
    }

    private void AddActionColumn(string name, string headerText, string buttonText)
    {
        DataGridViewButtonColumn column = new()
        {
            Name = name,
            HeaderText = headerText,
            Text = buttonText,
            UseColumnTextForButtonValue = true,
            Width = 70
        };

        _carsGrid.Columns.Add(column);
    }

    private async Task LoadCarsAsync()
    {
        try
        {
            UpdateTabStyles();

            CarCounts counts = await _carService.GetCarCountsAsync();
            UpdateMetricCards(counts);

            IReadOnlyList<Car> cars = await _carService.SearchCarsAsync(_searchTextBox.Text, _showArchived);
            PopulateGrid(cars);
        }
        catch (Exception exception)
        {
            MessageBoxHelper.ShowError($"Unable to load car records.\n\n{exception.Message}");
        }
    }

    private void UpdateMetricCards(CarCounts counts)
    {
        _totalCarsCard.SetMetric(IconChar.Car, "Total Cars", counts.TotalCars.ToString(), "All active vehicles");
        _availableCarsCard.SetMetric(IconChar.CircleCheck, "Available Cars", counts.AvailableCars.ToString(), "Ready for rental");
        _rentedCarsCard.SetMetric(IconChar.Key, "Rented Cars", counts.RentedCars.ToString(), "Currently rented");
        _archivedCarsCard.SetMetric(IconChar.BoxArchive, "Archived Cars", counts.ArchivedCars.ToString(), "Hidden from active list");
    }

    private void PopulateGrid(IReadOnlyList<Car> cars)
    {
        _carsGrid.Rows.Clear();

        foreach (Car car in cars)
        {
            _carsGrid.Rows.Add(
                car.CarId,
                car.CarName,
                car.Model,
                car.PlateNumber,
                car.RatePerDay.ToString("C"),
                car.Status);
        }

        _emptyStateLabel.Visible = cars.Count == 0;
    }

    private void UpdateTabStyles()
    {
        ApplyTabStyle(_activeCarsButton, !_showArchived);
        ApplyTabStyle(_archivedCarsButton, _showArchived);
    }

    private static void ApplyTabStyle(Button button, bool isActive)
    {
        button.BackColor = isActive ? ThemeHelper.Primary : ThemeHelper.Surface;
        button.ForeColor = isActive ? Color.White : ThemeHelper.TextPrimary;
    }

    private void CarsGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        string columnName = _carsGrid.Columns[e.ColumnIndex].Name;

        if (columnName is "ViewAction" or "EditAction" or "ArchiveAction")
        {
            string actionName = columnName.Replace("Action", string.Empty);
            MessageBoxHelper.ShowInfo($"{actionName} car feature is coming next.");
        }
    }
}
