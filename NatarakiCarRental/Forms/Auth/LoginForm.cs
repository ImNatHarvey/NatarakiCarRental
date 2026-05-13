using FontAwesome.Sharp;
using NatarakiCarRental.Forms.Main;
using NatarakiCarRental.Helpers;
using NatarakiCarRental.Models;
using NatarakiCarRental.Services;

namespace NatarakiCarRental.Forms.Auth;

public sealed class LoginForm : Form
{
    private readonly AuthService _authService = new();
    private readonly TextBox _usernameTextBox = ControlFactory.CreateTextBox();
    private readonly TextBox _passwordTextBox = ControlFactory.CreatePasswordTextBox();

    public LoginForm()
    {
        InitializeLoginForm();
    }

    private void InitializeLoginForm()
    {
        Text = AppConstants.ApplicationName;
        ThemeHelper.ApplyStandardFormSettings(this);

        TableLayoutPanel rootLayout = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeHelper.Background,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(80)
        };
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        Panel brandingPanel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeHelper.Secondary,
            Padding = new Padding(52)
        };

        IconPictureBox carIcon = new()
        {
            IconChar = IconChar.Car,
            IconColor = ThemeHelper.Primary,
            IconSize = 62,
            BackColor = ThemeHelper.Secondary,
            Location = new Point(52, 134),
            Size = new Size(70, 70)
        };

        Label titleLabel = new()
        {
            AutoSize = false,
            Text = AppConstants.ApplicationName,
            Font = FontHelper.Title(28F),
            ForeColor = ThemeHelper.Primary,
            Location = new Point(52, 218),
            Size = new Size(420, 52),
            TextAlign = ContentAlignment.MiddleLeft
        };

        Label descriptionLabel = new()
        {
            AutoSize = false,
            Text = "Internal scheduling and record management system",
            Font = FontHelper.Regular(12F),
            ForeColor = ThemeHelper.TextSecondary,
            Location = new Point(56, 278),
            Size = new Size(390, 58),
            TextAlign = ContentAlignment.MiddleLeft
        };

        Panel accentLine = new()
        {
            BackColor = ThemeHelper.Primary,
            Location = new Point(56, 354),
            Size = new Size(96, 4)
        };

        Panel formPanel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeHelper.Surface,
            Padding = new Padding(76, 96, 76, 96)
        };

        Label loginHeadingLabel = new()
        {
            AutoSize = false,
            Text = "Log In",
            Font = FontHelper.Title(24F),
            ForeColor = ThemeHelper.TextPrimary,
            Location = new Point(76, 132),
            Size = new Size(360, 44)
        };

        Label subtextLabel = new()
        {
            AutoSize = false,
            Text = "Sign in with your system account",
            Font = FontHelper.Regular(10.5F),
            ForeColor = ThemeHelper.TextSecondary,
            Location = new Point(78, 178),
            Size = new Size(360, 28)
        };

        Label usernameLabel = ControlFactory.CreateInputLabel("Username");
        usernameLabel.Location = new Point(78, 244);
        _usernameTextBox.Location = new Point(78, 270);
        _usernameTextBox.Width = 360;

        Label passwordLabel = ControlFactory.CreateInputLabel("Password");
        passwordLabel.Location = new Point(78, 326);
        _passwordTextBox.Location = new Point(78, 352);
        _passwordTextBox.Width = 360;

        Button loginButton = ControlFactory.CreatePrimaryButton("Login", 360, 42);
        loginButton.Location = new Point(78, 420);
        loginButton.Click += LoginButton_Click;

        brandingPanel.Controls.Add(carIcon);
        brandingPanel.Controls.Add(titleLabel);
        brandingPanel.Controls.Add(descriptionLabel);
        brandingPanel.Controls.Add(accentLine);

        formPanel.Controls.Add(loginHeadingLabel);
        formPanel.Controls.Add(subtextLabel);
        formPanel.Controls.Add(usernameLabel);
        formPanel.Controls.Add(_usernameTextBox);
        formPanel.Controls.Add(passwordLabel);
        formPanel.Controls.Add(_passwordTextBox);
        formPanel.Controls.Add(loginButton);

        rootLayout.Controls.Add(brandingPanel, 0, 0);
        rootLayout.Controls.Add(formPanel, 1, 0);

        Controls.Add(rootLayout);
        AcceptButton = loginButton;
    }

    private void LoginButton_Click(object? sender, EventArgs e)
    {
        try
        {
            User? user = _authService.Login(_usernameTextBox.Text, _passwordTextBox.Text);

            if (user is null)
            {
                MessageBoxHelper.ShowError("Invalid username or password.");
                return;
            }

            OpenMainForm(user);
        }
        catch (Exception exception)
        {
            MessageBoxHelper.ShowError($"Login failed.\n\n{exception.Message}");
        }
    }

    private void OpenMainForm(User user)
    {
        Hide();

        MainForm mainForm = new(user);
        mainForm.LoggedOut += (_, _) =>
        {
            _passwordTextBox.Clear();
            Show();
            _passwordTextBox.Focus();
        };
        mainForm.FormClosed += (_, _) =>
        {
            if (!Visible)
            {
                Close();
            }
        };

        mainForm.Show();
    }
}
