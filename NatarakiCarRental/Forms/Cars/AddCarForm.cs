using FluentValidation;
using NatarakiCarRental.Helpers;
using NatarakiCarRental.Models;
using NatarakiCarRental.Services;

namespace NatarakiCarRental.Forms.Cars;

public sealed class AddCarForm : Form
{
    private readonly CarService _carService = new();
    private readonly TextBox _carNameTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _brandTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _modelTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _plateNumberTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _yearTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _colorTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _transmissionTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _fuelTypeTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _seatingCapacityTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _ratePerDayTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _imagePathTextBox = ControlFactory.CreateTextBox(230);
    private readonly TextBox _orCrPathTextBox = ControlFactory.CreateTextBox(230);
    private readonly ComboBox _statusComboBox = new();
    private Button? _saveButton;

    public AddCarForm()
    {
        InitializeForm();
    }

    private void InitializeForm()
    {
        Text = "Add Car";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(720, 610);
        BackColor = ThemeHelper.Surface;
        Font = FontHelper.Regular();
        ShowInTaskbar = false;

        Label titleLabel = new()
        {
            Text = "Add Car",
            AutoSize = false,
            Location = new Point(28, 22),
            Size = new Size(220, 34),
            Font = FontHelper.Title(18F),
            ForeColor = ThemeHelper.TextPrimary
        };

        Label subtitleLabel = new()
        {
            Text = "Create a new vehicle record for the garage.",
            AutoSize = false,
            Location = new Point(30, 58),
            Size = new Size(420, 24),
            Font = FontHelper.Regular(9.5F),
            ForeColor = ThemeHelper.TextSecondary
        };

        TableLayoutPanel fieldsLayout = new()
        {
            Location = new Point(30, 104),
            Size = new Size(660, 390),
            ColumnCount = 2,
            RowCount = 6
        };
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        for (int row = 0; row < 6; row++)
        {
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 65F));
        }

        ConfigureInputs();

        fieldsLayout.Controls.Add(CreateInputPanel("Car Name *", _carNameTextBox), 0, 0);
        fieldsLayout.Controls.Add(CreateInputPanel("Brand", _brandTextBox), 1, 0);
        fieldsLayout.Controls.Add(CreateInputPanel("Model *", _modelTextBox), 0, 1);
        fieldsLayout.Controls.Add(CreateInputPanel("Plate Number *", _plateNumberTextBox), 1, 1);
        fieldsLayout.Controls.Add(CreateInputPanel("Year", _yearTextBox), 0, 2);
        fieldsLayout.Controls.Add(CreateInputPanel("Color", _colorTextBox), 1, 2);
        fieldsLayout.Controls.Add(CreateInputPanel("Transmission", _transmissionTextBox), 0, 3);
        fieldsLayout.Controls.Add(CreateInputPanel("Fuel Type", _fuelTypeTextBox), 1, 3);
        fieldsLayout.Controls.Add(CreateInputPanel("Seating Capacity", _seatingCapacityTextBox), 0, 4);
        fieldsLayout.Controls.Add(CreateInputPanel("Rate Per Day *", _ratePerDayTextBox), 1, 4);
        fieldsLayout.Controls.Add(CreateInputPanel("Image Path", _imagePathTextBox), 0, 5);
        fieldsLayout.Controls.Add(CreateInputPanel("OR/CR Path", _orCrPathTextBox), 1, 5);

        Panel statusPanel = CreateInputPanel("Status *", _statusComboBox);
        statusPanel.Location = new Point(30, 502);
        statusPanel.Size = new Size(310, 58);

        _saveButton = ControlFactory.CreatePrimaryButton("Save Car", 120, 38);
        _saveButton.Location = new Point(438, 546);
        _saveButton.Click += SaveButton_Click;

        Button cancelButton = new()
        {
            Text = "Cancel",
            Size = new Size(120, 38),
            Location = new Point(570, 546),
            BackColor = ThemeHelper.Surface,
            ForeColor = ThemeHelper.TextPrimary,
            Font = FontHelper.SemiBold(),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.Cancel
        };
        cancelButton.FlatAppearance.BorderColor = ThemeHelper.Border;

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(fieldsLayout);
        Controls.Add(statusPanel);
        Controls.Add(_saveButton);
        Controls.Add(cancelButton);

        AcceptButton = _saveButton;
        CancelButton = cancelButton;
    }

    private void ConfigureInputs()
    {
        _plateNumberTextBox.CharacterCasing = CharacterCasing.Upper;
        _yearTextBox.MaxLength = 4;
        _yearTextBox.KeyPress += AllowDigitsOnly;
        _seatingCapacityTextBox.KeyPress += AllowDigitsOnly;
        _ratePerDayTextBox.KeyPress += AllowDecimalOnly;

        _statusComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _statusComboBox.Font = FontHelper.Regular(10F);
        _statusComboBox.ForeColor = ThemeHelper.TextPrimary;
        _statusComboBox.Width = 230;
        _statusComboBox.Height = 30;
        _statusComboBox.Items.AddRange(["Available", "Rented", "Under Maintenance"]);
        _statusComboBox.SelectedItem = "Available";
    }

    private static Panel CreateInputPanel(string labelText, Control inputControl)
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 20, 0),
            BackColor = ThemeHelper.Surface
        };

        Label label = ControlFactory.CreateInputLabel(labelText);
        label.Location = new Point(0, 0);

        inputControl.Location = new Point(0, 24);

        panel.Controls.Add(label);
        panel.Controls.Add(inputControl);

        return panel;
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        if (_saveButton is null)
        {
            return;
        }

        try
        {
            _saveButton.Enabled = false;
            Car car = BuildCarFromInputs();
            await _carService.AddCarAsync(car);

            MessageBoxHelper.ShowSuccess("Car record added successfully.");
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ValidationException exception)
        {
            string message = string.Join(Environment.NewLine, exception.Errors.Select(error => error.ErrorMessage));

            if (string.IsNullOrWhiteSpace(message))
            {
                message = exception.Message;
            }

            if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                MessageBoxHelper.ShowWarning(message);
            }
            else
            {
                MessageBoxHelper.ShowError(message, "Validation Error");
            }
        }
        catch (Exception exception)
        {
            MessageBoxHelper.ShowError($"Unable to add car record.\n\n{exception.Message}");
        }
        finally
        {
            _saveButton.Enabled = true;
        }
    }

    private Car BuildCarFromInputs()
    {
        return new Car
        {
            CarName = _carNameTextBox.Text,
            Brand = _brandTextBox.Text,
            Model = _modelTextBox.Text,
            PlateNumber = _plateNumberTextBox.Text,
            Year = TryParseInt(_yearTextBox.Text),
            Color = NullIfWhiteSpace(_colorTextBox.Text),
            Transmission = NullIfWhiteSpace(_transmissionTextBox.Text),
            FuelType = NullIfWhiteSpace(_fuelTypeTextBox.Text),
            SeatingCapacity = TryParseInt(_seatingCapacityTextBox.Text),
            RatePerDay = TryParseDecimal(_ratePerDayTextBox.Text),
            Status = _statusComboBox.SelectedItem?.ToString() ?? "Available",
            ImagePath = NullIfWhiteSpace(_imagePathTextBox.Text),
            OrCrPath = NullIfWhiteSpace(_orCrPathTextBox.Text)
        };
    }

    private static int? TryParseInt(string value)
    {
        return int.TryParse(value.Trim(), out int result) ? result : null;
    }

    private static decimal TryParseDecimal(string value)
    {
        return decimal.TryParse(value.Trim(), out decimal result) ? result : 0M;
    }

    private static string? NullIfWhiteSpace(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void AllowDigitsOnly(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private static void AllowDecimalOnly(object? sender, KeyPressEventArgs e)
    {
        TextBox? textBox = sender as TextBox;
        bool isDecimalPoint = e.KeyChar == '.';

        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !isDecimalPoint)
        {
            e.Handled = true;
            return;
        }

        if (isDecimalPoint && textBox is not null && textBox.Text.Contains('.'))
        {
            e.Handled = true;
        }
    }
}
