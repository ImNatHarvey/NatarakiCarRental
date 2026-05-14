using FluentValidation;
using FluentValidation.Results;
using NatarakiCarRental.Helpers;
using NatarakiCarRental.Models;
using NatarakiCarRental.Services;

namespace NatarakiCarRental.Forms.Customers;

public enum CustomerFormMode
{
    Add,
    Edit,
    View
}

public sealed class CustomerDetailsForm : Form
{
    private readonly CustomerService _customerService = new();
    private readonly CustomerFormMode _mode;
    private readonly Customer? _sourceCustomer;
    private readonly ErrorProvider _errorProvider = new();

    private readonly TextBox _firstNameTextBox = ControlFactory.CreateTextBox(260);
    private readonly TextBox _lastNameTextBox = ControlFactory.CreateTextBox(260);
    private readonly TextBox _phoneNumberTextBox = ControlFactory.CreateTextBox(260);
    private readonly TextBox _emailTextBox = ControlFactory.CreateTextBox(260);
    private readonly TextBox _addressTextBox = ControlFactory.CreateTextBox(760);
    private readonly Label _driverLicensePathLabel = new();
    private readonly Label _proofOfBillingPathLabel = new();
    private readonly Button _driverLicenseBrowseButton = CreateSecondaryButton("Browse", 90, 30);
    private readonly Button _proofOfBillingBrowseButton = CreateSecondaryButton("Browse", 90, 30);
    private readonly Label _validationLabel = new();

    private string? _selectedDriverLicenseSourcePath;
    private string? _selectedProofOfBillingSourcePath;

    public CustomerDetailsForm(CustomerFormMode mode, Customer? customer = null)
    {
        _mode = mode;
        _sourceCustomer = customer;
        InitializeForm();
        ConfigureInputs();

        if (customer is not null)
        {
            LoadCustomer(customer);
        }

        if (_mode == CustomerFormMode.View)
        {
            ApplyViewMode();
        }
    }

    private bool IsViewMode => _mode == CustomerFormMode.View;

    private void InitializeForm()
    {
        string title = _mode switch
        {
            CustomerFormMode.Edit => "Edit Customer",
            CustomerFormMode.View => "View Customer",
            _ => "Add Customer"
        };

        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(900, 610);
        BackColor = ThemeHelper.Surface;
        Font = FontHelper.Regular();
        ShowInTaskbar = false;

        _errorProvider.ContainerControl = this;
        _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        Label titleLabel = new()
        {
            Text = title,
            AutoSize = false,
            Location = new Point(32, 24),
            Size = new Size(280, 34),
            Font = FontHelper.Title(18F),
            ForeColor = ThemeHelper.TextPrimary
        };

        Label subtitleLabel = new()
        {
            Text = _mode switch
            {
                CustomerFormMode.Edit => "Update customer contact details and attached documents.",
                CustomerFormMode.View => "Review this customer record and saved documents.",
                _ => "Create a customer record for rentals and document tracking."
            },
            AutoSize = false,
            Location = new Point(34, 58),
            Size = new Size(650, 24),
            Font = FontHelper.Regular(9.5F),
            ForeColor = ThemeHelper.TextSecondary
        };

        _validationLabel.AutoSize = false;
        _validationLabel.Location = new Point(34, 86);
        _validationLabel.Size = new Size(820, 24);
        _validationLabel.Font = FontHelper.SemiBold(9F);
        _validationLabel.ForeColor = ThemeHelper.Danger;
        _validationLabel.Visible = false;

        Panel contentPanel = new()
        {
            Location = new Point(32, 116),
            Size = new Size(836, 408),
            BackColor = ThemeHelper.Surface
        };

        TableLayoutPanel contentLayout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));

        contentLayout.Controls.Add(CreateSection("Required Information", CreateRequiredLayout()), 0, 0);
        contentLayout.Controls.Add(CreateSection("Contact & Address", CreateContactLayout()), 0, 1);
        contentLayout.Controls.Add(CreateSection("Documents", CreateDocumentsLayout()), 0, 2);
        contentPanel.Controls.Add(contentLayout);

        Button cancelButton = CreateSecondaryButton(IsViewMode ? "Close" : "Cancel", 118, 38);
        cancelButton.Location = new Point(IsViewMode ? 750 : 610, 548);
        cancelButton.DialogResult = DialogResult.Cancel;

        Button? saveButton = null;
        if (!IsViewMode)
        {
            saveButton = ControlFactory.CreatePrimaryButton(_mode == CustomerFormMode.Edit ? "Save Changes" : "Add Customer", 140, 38);
            saveButton.Location = new Point(728, 548);
            saveButton.Click += SaveButton_Click;
            AcceptButton = saveButton;
        }

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(_validationLabel);
        Controls.Add(contentPanel);
        Controls.Add(cancelButton);

        if (saveButton is not null)
        {
            Controls.Add(saveButton);
        }

        CancelButton = cancelButton;
    }

    private TableLayoutPanel CreateRequiredLayout()
    {
        TableLayoutPanel layout = CreateGrid(3, 1);
        layout.Controls.Add(CreateInputPanel("First Name *", _firstNameTextBox), 0, 0);
        layout.Controls.Add(CreateInputPanel("Last Name *", _lastNameTextBox), 1, 0);
        layout.Controls.Add(CreateInputPanel("Phone Number *", _phoneNumberTextBox), 2, 0);
        return layout;
    }

    private TableLayoutPanel CreateContactLayout()
    {
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));

        Panel addressPanel = CreateInputPanel("Full Address", _addressTextBox);
        layout.Controls.Add(CreateInputPanel("Email", _emailTextBox), 0, 0);
        layout.Controls.Add(addressPanel, 0, 1);
        layout.SetColumnSpan(addressPanel, 3);
        return layout;
    }

    private TableLayoutPanel CreateDocumentsLayout()
    {
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.Controls.Add(CreateFilePickerPanel("Driver's License Document", _driverLicensePathLabel, _driverLicenseBrowseButton), 0, 0);
        layout.Controls.Add(CreateFilePickerPanel("Proof of Billing Document", _proofOfBillingPathLabel, _proofOfBillingBrowseButton), 1, 0);
        return layout;
    }

    private static GroupBox CreateSection(string title, Control content)
    {
        GroupBox section = new()
        {
            Text = title,
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 30, 16, 12),
            Font = FontHelper.SemiBold(9.5F),
            ForeColor = ThemeHelper.TextPrimary
        };
        section.Controls.Add(content);
        return section;
    }

    private static TableLayoutPanel CreateGrid(int columns, int rows)
    {
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = columns,
            RowCount = rows
        };

        for (int column = 0; column < columns; column++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));
        }

        for (int row = 0; row < rows; row++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        }

        return layout;
    }

    private static Panel CreateInputPanel(string labelText, Control inputControl)
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 12, 0),
            BackColor = ThemeHelper.Surface
        };

        Label label = ControlFactory.CreateInputLabel(labelText);
        label.Location = new Point(0, 0);
        inputControl.Location = new Point(0, 22);
        panel.Controls.Add(label);
        panel.Controls.Add(inputControl);
        return panel;
    }

    private static Panel CreateFilePickerPanel(string labelText, Label pathLabel, Button browseButton)
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeHelper.Surface
        };

        Label label = ControlFactory.CreateInputLabel(labelText);
        label.Location = new Point(0, 0);
        browseButton.Location = new Point(0, 28);

        pathLabel.AutoSize = false;
        pathLabel.Location = new Point(102, 33);
        pathLabel.Size = new Size(270, 20);
        pathLabel.Font = FontHelper.Regular(9F);
        pathLabel.ForeColor = ThemeHelper.TextSecondary;
        pathLabel.Text = "No file selected";

        panel.Controls.Add(label);
        panel.Controls.Add(browseButton);
        panel.Controls.Add(pathLabel);
        return panel;
    }

    private void ConfigureInputs()
    {
        _firstNameTextBox.PlaceholderText = "e.g. Juan";
        _lastNameTextBox.PlaceholderText = "e.g. Dela Cruz";
        _phoneNumberTextBox.PlaceholderText = "e.g. 09171234567";
        _emailTextBox.PlaceholderText = "e.g. customer@email.com";
        _addressTextBox.PlaceholderText = "Complete customer address";

        _driverLicenseBrowseButton.Click += (_, _) => BrowseFile(
            "Select driver's license document",
            "Supported files|*.jpg;*.jpeg;*.png;*.webp;*.pdf",
            path =>
            {
                _selectedDriverLicenseSourcePath = path;
                _driverLicensePathLabel.Text = Path.GetFileName(path);
                _driverLicensePathLabel.ForeColor = ThemeHelper.Primary;
            });

        _proofOfBillingBrowseButton.Click += (_, _) => BrowseFile(
            "Select proof of billing document",
            "Supported files|*.jpg;*.jpeg;*.png;*.webp;*.pdf",
            path =>
            {
                _selectedProofOfBillingSourcePath = path;
                _proofOfBillingPathLabel.Text = Path.GetFileName(path);
                _proofOfBillingPathLabel.ForeColor = ThemeHelper.Primary;
            });
    }

    private void LoadCustomer(Customer customer)
    {
        _firstNameTextBox.Text = customer.FirstName;
        _lastNameTextBox.Text = customer.LastName;
        _phoneNumberTextBox.Text = customer.PhoneNumber;
        _emailTextBox.Text = customer.Email ?? string.Empty;
        _addressTextBox.Text = customer.Address ?? string.Empty;
        _driverLicensePathLabel.Text = string.IsNullOrWhiteSpace(customer.DriverLicensePath)
            ? "No file attached"
            : Path.GetFileName(customer.DriverLicensePath);
        _proofOfBillingPathLabel.Text = string.IsNullOrWhiteSpace(customer.ProofOfBillingPath)
            ? "No file attached"
            : Path.GetFileName(customer.ProofOfBillingPath);
    }

    private void ApplyViewMode()
    {
        foreach (Control control in GetAllControls(this))
        {
            if (control is TextBox textBox)
            {
                textBox.ReadOnly = true;
                textBox.BackColor = Color.FromArgb(248, 250, 252);
            }
        }

        _driverLicenseBrowseButton.Enabled = false;
        _proofOfBillingBrowseButton.Enabled = false;
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        if (sender is not Button saveButton)
        {
            return;
        }

        try
        {
            saveButton.Enabled = false;
            ClearValidationState();

            Customer customer = BuildCustomerFromInputs();
            customer.DriverLicensePath = SaveUploadedFileIfSelected(_selectedDriverLicenseSourcePath, _sourceCustomer?.DriverLicensePath);
            customer.ProofOfBillingPath = SaveUploadedFileIfSelected(_selectedProofOfBillingSourcePath, _sourceCustomer?.ProofOfBillingPath);

            if (_mode == CustomerFormMode.Edit)
            {
                await _customerService.UpdateCustomerAsync(customer);
                MessageBoxHelper.ShowSuccess("Customer record updated successfully.");
            }
            else
            {
                await _customerService.AddCustomerAsync(customer);
                MessageBoxHelper.ShowSuccess("Customer record added successfully.");
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ValidationException exception)
        {
            ShowValidationErrors(exception.Errors.ToList(), exception.Message);
        }
        catch (Exception exception)
        {
            MessageBoxHelper.ShowError($"Unable to save customer record.\n\n{exception.Message}");
        }
        finally
        {
            saveButton.Enabled = true;
        }
    }

    private Customer BuildCustomerFromInputs()
    {
        return new Customer
        {
            CustomerId = _sourceCustomer?.CustomerId ?? 0,
            FirstName = _firstNameTextBox.Text,
            LastName = _lastNameTextBox.Text,
            PhoneNumber = _phoneNumberTextBox.Text,
            Email = NullIfWhiteSpace(_emailTextBox.Text),
            Address = NullIfWhiteSpace(_addressTextBox.Text),
            IsBlacklisted = _sourceCustomer?.IsBlacklisted ?? false,
            BlacklistReason = _sourceCustomer?.BlacklistReason,
            DriverLicensePath = _sourceCustomer?.DriverLicensePath,
            ProofOfBillingPath = _sourceCustomer?.ProofOfBillingPath,
            IsArchived = _sourceCustomer?.IsArchived ?? false
        };
    }

    private static string? SaveUploadedFileIfSelected(string? sourcePath, string? existingPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return existingPath;
        }

        string uploadDirectory = Path.Combine(AppContext.BaseDirectory, AppConstants.CustomersUploadFolder);
        Directory.CreateDirectory(uploadDirectory);

        string extension = Path.GetExtension(sourcePath);
        string fileName = $"{Guid.NewGuid():N}{extension}";
        string destinationPath = Path.Combine(uploadDirectory, fileName);
        File.Copy(sourcePath, destinationPath);

        return Path.Combine(AppConstants.CustomersUploadFolder, fileName);
    }

    private static void BrowseFile(string title, string filter, Action<string> selected)
    {
        using OpenFileDialog dialog = new()
        {
            Title = title,
            Filter = filter,
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            selected(dialog.FileName);
        }
    }

    private void ShowValidationErrors(IReadOnlyList<ValidationFailure> errors, string fallbackMessage)
    {
        string message = string.Join(Environment.NewLine, errors.Select(error => error.ErrorMessage));

        if (string.IsNullOrWhiteSpace(message))
        {
            message = fallbackMessage;
        }

        _validationLabel.Text = message.Split(Environment.NewLine).FirstOrDefault() ?? message;
        _validationLabel.Visible = true;

        foreach (ValidationFailure error in errors)
        {
            Control? control = GetControlForProperty(error.PropertyName);
            if (control is not null)
            {
                _errorProvider.SetError(control, error.ErrorMessage);
            }
        }

        if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            _errorProvider.SetError(_phoneNumberTextBox, message);
            MessageBoxHelper.ShowWarning(message);
            return;
        }

        MessageBoxHelper.ShowError(message, "Validation Error");
    }

    private void ClearValidationState()
    {
        _validationLabel.Visible = false;
        _errorProvider.Clear();
    }

    private Control? GetControlForProperty(string propertyName)
    {
        return propertyName switch
        {
            nameof(Customer.FirstName) => _firstNameTextBox,
            nameof(Customer.LastName) => _lastNameTextBox,
            nameof(Customer.PhoneNumber) => _phoneNumberTextBox,
            nameof(Customer.Email) => _emailTextBox,
            _ => null
        };
    }

    private static Button CreateSecondaryButton(string text, int width, int height)
    {
        Button button = new()
        {
            Text = text,
            Size = new Size(width, height),
            BackColor = ThemeHelper.Surface,
            ForeColor = ThemeHelper.TextPrimary,
            Font = FontHelper.SemiBold(),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = ThemeHelper.Border;
        return button;
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static IEnumerable<Control> GetAllControls(Control parent)
    {
        foreach (Control control in parent.Controls)
        {
            yield return control;

            foreach (Control child in GetAllControls(control))
            {
                yield return child;
            }
        }
    }
}
