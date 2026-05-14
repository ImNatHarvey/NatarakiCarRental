using FontAwesome.Sharp;
using System.Globalization;
using NatarakiCarRental.Forms.Cars;
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
    private static readonly CultureInfo PhilippineCulture = new("en-PH");

    private readonly TextBox _searchTextBox = ControlFactory.CreateTextBox(300);
    private readonly IconButton _activeCarsButton = new();
    private readonly IconButton _archivedCarsButton = new();
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
        mainLayout.Controls.Add(CreateActionBarPanel(), 0, 2);
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
            Text = "Car Garage",
            AutoSize = false,
            Location = new Point(0, 0),
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

        AddMetricCard(grid, _totalCarsCard, IconChar.Car, "Total Cars", 0, "All active vehicles", ThemeHelper.Primary);
        AddMetricCard(grid, _availableCarsCard, IconChar.CircleCheck, "Available Cars", 1, "Ready for rental", ThemeHelper.Success);
        AddMetricCard(grid, _rentedCarsCard, IconChar.Key, "Rented Cars", 2, "Currently rented", ThemeHelper.Warning);
        AddMetricCard(grid, _archivedCarsCard, IconChar.BoxArchive, "Archived Cars", 3, "Hidden from active list", ThemeHelper.GrayIcon);

        return grid;
    }

    private static void AddMetricCard(TableLayoutPanel grid, MetricCardControl card, IconChar icon, string title, int column, string helperText, Color iconColor)
    {
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, column == 3 ? 0 : 14, 0);
        card.SetMetric(icon, title, "0", helperText, iconColor);
        grid.Controls.Add(card, column, 0);
    }

    private Panel CreateActionBarPanel()
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeHelper.ContentBackground
        };

        ConfigureTabButton(_activeCarsButton, "Active Cars", IconChar.CircleCheck, new Point(0, 10));
        ConfigureTabButton(_archivedCarsButton, "Archived Cars", IconChar.BoxArchive, new Point(128, 10));

        IconButton addCarButton = new()
        {
            Text = "Add Car",
            IconChar = IconChar.Plus,
            IconColor = Color.White,
            IconSize = 14,
            Size = new Size(116, 36),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(Width - 116, 10),
            BackColor = ThemeHelper.Primary,
            ForeColor = Color.White,
            Font = FontHelper.SemiBold(9F),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        addCarButton.FlatAppearance.BorderSize = 0;
        addCarButton.TextImageRelation = TextImageRelation.ImageBeforeText;
        addCarButton.Click += AddCarButton_Click;

        panel.Resize += (_, _) => addCarButton.Left = panel.Width - addCarButton.Width;
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
        panel.Controls.Add(addCarButton);

        return panel;
    }

    private Panel CreateSearchPanel()
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeHelper.ContentBackground
        };

        IconPictureBox searchIcon = new()
        {
            IconChar = IconChar.MagnifyingGlass,
            IconColor = ThemeHelper.TextSecondary,
            IconSize = 16,
            BackColor = ThemeHelper.ContentBackground,
            Location = new Point(2, 15),
            Size = new Size(20, 20)
        };

        _searchTextBox.PlaceholderText = "Search cars";
        _searchTextBox.Location = new Point(28, 8);
        _searchTextBox.Width = 300;
        _searchTextBox.TextChanged += async (_, _) => await LoadCarsAsync();

        panel.Controls.Add(searchIcon);
        panel.Controls.Add(_searchTextBox);

        return panel;
    }

    private static void ConfigureTabButton(IconButton button, string text, IconChar icon, Point location)
    {
        button.Text = text;
        button.IconChar = icon;
        button.IconSize = 16;
        button.TextImageRelation = TextImageRelation.ImageBeforeText;
        button.Location = location;
        button.Size = new Size(120, 34);
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
        _carsGrid.Columns.Add("CodingDay", "Coding Day");
        _carsGrid.Columns.Add("Status", "Status");

        AddActionColumn("ViewAction", "View", "View");

        if (_showArchived)
        {
            AddActionColumn("RestoreAction", "Restore", "Restore");
        }
        else
        {
            AddActionColumn("EditAction", "Edit", "Edit");
            AddActionColumn("ArchiveAction", "Archive", "Archive");
        }

        if (_carsGrid.Columns["CarId"] is DataGridViewColumn carIdColumn)
        {
            carIdColumn.Visible = false;
        }

        if (_carsGrid.Columns["RatePerDay"] is DataGridViewColumn rateColumn)
        {
            rateColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        if (_carsGrid.Columns["ViewAction"] is DataGridViewColumn viewColumn)
        {
            viewColumn.FillWeight = 55;
        }

        if (_carsGrid.Columns["EditAction"] is DataGridViewColumn editColumn)
        {
            editColumn.FillWeight = 55;
        }

        if (_carsGrid.Columns["ArchiveAction"] is DataGridViewColumn archiveColumn)
        {
            archiveColumn.FillWeight = 65;
        }

        if (_carsGrid.Columns["RestoreAction"] is DataGridViewColumn restoreColumn)
        {
            restoreColumn.FillWeight = 70;
        }
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
        _totalCarsCard.SetMetric(IconChar.Car, "Total Cars", counts.TotalCars.ToString(), "All active vehicles", ThemeHelper.Primary);
        _availableCarsCard.SetMetric(IconChar.CircleCheck, "Available Cars", counts.AvailableCars.ToString(), "Ready for rental", ThemeHelper.Success);
        _rentedCarsCard.SetMetric(IconChar.Key, "Rented Cars", counts.RentedCars.ToString(), "Currently rented", ThemeHelper.Warning);
        _archivedCarsCard.SetMetric(IconChar.BoxArchive, "Archived Cars", counts.ArchivedCars.ToString(), "Hidden from active list", ThemeHelper.GrayIcon);
    }

    private void PopulateGrid(IReadOnlyList<Car> cars)
    {
        AddGridColumns();
        _carsGrid.Rows.Clear();

        foreach (Car car in cars)
        {
            _carsGrid.Rows.Add(
                car.CarId,
                car.CarName,
                car.Model,
                car.PlateNumber,
                car.RatePerDay.ToString("C", PhilippineCulture),
                string.IsNullOrWhiteSpace(car.CodingDay) ? "-" : car.CodingDay,
                car.Status);
        }

        _emptyStateLabel.Visible = cars.Count == 0;
    }

    private void UpdateTabStyles()
    {
        ApplyTabStyle(_activeCarsButton, !_showArchived);
        ApplyTabStyle(_archivedCarsButton, _showArchived);
    }

    private static void ApplyTabStyle(IconButton button, bool isActive)
    {
        button.BackColor = isActive ? ThemeHelper.Primary : ThemeHelper.Surface;
        button.ForeColor = isActive ? Color.White : ThemeHelper.TextPrimary;
        button.IconColor = isActive ? Color.White : ThemeHelper.TextSecondary;
    }

    private async void CarsGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        string columnName = _carsGrid.Columns[e.ColumnIndex].Name;

        int carId = Convert.ToInt32(_carsGrid.Rows[e.RowIndex].Cells["CarId"].Value);

        if (columnName == "ViewAction")
        {
            await ViewCarAsync(carId);
            return;
        }

        if (columnName == "EditAction")
        {
            await EditCarAsync(carId);
            return;
        }

        if (columnName == "ArchiveAction")
        {
            await ArchiveCarAsync(carId);
            return;
        }

        if (columnName == "RestoreAction")
        {
            await RestoreCarAsync(carId);
        }
    }

    private async void AddCarButton_Click(object? sender, EventArgs e)
    {
        using CarDetailsForm addCarForm = new(CarFormMode.Add);

        if (addCarForm.ShowDialog(this) == DialogResult.OK)
        {
            _showArchived = false;
            await LoadCarsAsync();
        }
    }

    private async Task ViewCarAsync(int carId)
    {
        Car? car = await _carService.GetCarByIdAsync(carId);

        if (car is null)
        {
            MessageBoxHelper.ShowWarning("The selected car record no longer exists.");
            await LoadCarsAsync();
            return;
        }

        using CarDetailsForm form = new(CarFormMode.View, car);
        form.ShowDialog(this);
    }

    private async Task EditCarAsync(int carId)
    {
        Car? car = await _carService.GetCarByIdAsync(carId);

        if (car is null)
        {
            MessageBoxHelper.ShowWarning("The selected car record no longer exists.");
            await LoadCarsAsync();
            return;
        }

        using CarDetailsForm form = new(CarFormMode.Edit, car);

        if (form.ShowDialog(this) == DialogResult.OK)
        {
            await LoadCarsAsync();
        }
    }

    private async Task ArchiveCarAsync(int carId)
    {
        Car? car = await _carService.GetCarByIdAsync(carId);

        if (car is null)
        {
            MessageBoxHelper.ShowWarning("The selected car record no longer exists.");
            await LoadCarsAsync();
            return;
        }

        bool confirmed = MessageBoxHelper.ShowConfirmDanger(
            $"Archive {car.CarName} ({car.PlateNumber})? This will hide it from active car lists.",
            "Archive Car");

        if (!confirmed)
        {
            return;
        }

        await _carService.ArchiveCarAsync(carId);
        await LoadCarsAsync();
    }

    private async Task RestoreCarAsync(int carId)
    {
        Car? car = await _carService.GetCarByIdAsync(carId);

        if (car is null)
        {
            MessageBoxHelper.ShowWarning("The selected car record no longer exists.");
            await LoadCarsAsync();
            return;
        }

        bool confirmed = MessageBoxHelper.ShowConfirmWarning(
            $"Restore {car.CarName} ({car.PlateNumber}) to the active car list?",
            "Restore Car");

        if (!confirmed)
        {
            return;
        }

        await _carService.RestoreCarAsync(carId);
        await LoadCarsAsync();
    }
}
