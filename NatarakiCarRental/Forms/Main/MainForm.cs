using FontAwesome.Sharp;
using NatarakiCarRental.Helpers;
using NatarakiCarRental.Models;

namespace NatarakiCarRental.Forms.Main;

public sealed class MainForm : Form
{
    private readonly Label _pageTitleLabel = new();
    private readonly Label _pagePlaceholderLabel = new();
    private readonly List<IconButton> _navigationButtons = [];

    public event EventHandler? LoggedOut;

    public MainForm(User currentUser)
    {
        CurrentUser = currentUser;
        InitializeMainForm();
        ShowPlaceholder("Overview");
    }

    private User CurrentUser { get; }

    private void InitializeMainForm()
    {
        Text = AppConstants.ApplicationName;
        ThemeHelper.ApplyStandardMainFormSettings(this);

        Panel sidebarPanel = new()
        {
            BackColor = ThemeHelper.Surface,
            Dock = DockStyle.Left,
            Width = 240,
            Padding = new Padding(16, 22, 16, 16)
        };

        Panel brandPanel = new()
        {
            Dock = DockStyle.Top,
            Height = 64
        };

        IconPictureBox brandIcon = new()
        {
            IconChar = IconChar.Car,
            IconColor = ThemeHelper.Primary,
            IconSize = 30,
            BackColor = ThemeHelper.Surface,
            Location = new Point(0, 8),
            Size = new Size(34, 34)
        };

        Label brandLabel = new()
        {
            Text = AppConstants.ApplicationName,
            AutoSize = false,
            Location = new Point(42, 4),
            Size = new Size(166, 44),
            Font = FontHelper.Title(13F),
            ForeColor = ThemeHelper.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };

        Label userLabel = new()
        {
            Text = $"Signed in: {CurrentUser.FirstName}",
            AutoSize = false,
            Height = 34,
            Dock = DockStyle.Top,
            ForeColor = ThemeHelper.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        };

        FlowLayoutPanel menuPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 28, 0, 0)
        };

        NavigationItem[] menuItems =
        [
            new("Overview", IconChar.ChartLine),
            new("Car Garage", IconChar.Car),
            new("Customers", IconChar.Users),
            new("Transactions", IconChar.Receipt),
            new("Offsite", IconChar.LocationDot),
            new("Activity Log", IconChar.ClipboardList),
            new("Manage System", IconChar.Gear)
        ];

        foreach (NavigationItem menuItem in menuItems)
        {
            IconButton button = ControlFactory.CreateSidebarButton(menuItem.Text, menuItem.Icon);
            button.Click += (_, _) => ShowPlaceholder(menuItem.Text);
            _navigationButtons.Add(button);
            menuPanel.Controls.Add(button);
        }

        IconButton logoutButton = ControlFactory.CreateSidebarButton("Logout", IconChar.RightFromBracket, isDanger: true);
        logoutButton.Dock = DockStyle.Bottom;
        logoutButton.Width = 208;
        logoutButton.Click += LogoutButton_Click;

        brandPanel.Controls.Add(brandIcon);
        brandPanel.Controls.Add(brandLabel);

        sidebarPanel.Controls.Add(menuPanel);
        sidebarPanel.Controls.Add(userLabel);
        sidebarPanel.Controls.Add(brandPanel);
        sidebarPanel.Controls.Add(logoutButton);

        Panel contentPanel = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32)
        };

        _pageTitleLabel.AutoSize = false;
        _pageTitleLabel.Dock = DockStyle.Top;
        _pageTitleLabel.Height = 48;
        _pageTitleLabel.Font = FontHelper.Title(20F);
        _pageTitleLabel.ForeColor = ThemeHelper.TextPrimary;

        Panel placeholderCard = ControlFactory.CreateCardPanel(new Size(0, 160));
        placeholderCard.Dock = DockStyle.Top;
        placeholderCard.Margin = new Padding(0, 12, 0, 0);
        ControlFactory.ApplyRoundedPanel(placeholderCard);

        _pagePlaceholderLabel.AutoSize = false;
        _pagePlaceholderLabel.Dock = DockStyle.Fill;
        _pagePlaceholderLabel.Font = FontHelper.Regular(12F);
        _pagePlaceholderLabel.ForeColor = ThemeHelper.TextSecondary;
        _pagePlaceholderLabel.TextAlign = ContentAlignment.MiddleLeft;

        placeholderCard.Controls.Add(_pagePlaceholderLabel);
        contentPanel.Controls.Add(placeholderCard);
        contentPanel.Controls.Add(_pageTitleLabel);

        Controls.Add(contentPanel);
        Controls.Add(sidebarPanel);
    }

    private void ShowPlaceholder(string pageName)
    {
        _pageTitleLabel.Text = pageName;
        _pagePlaceholderLabel.Text = $"{pageName} module placeholder. This section will be built in a later step.";

        foreach (IconButton button in _navigationButtons)
        {
            bool isActive = button.Text == pageName;
            button.BackColor = isActive ? ThemeHelper.Secondary : Color.Transparent;
            button.IconColor = isActive ? ThemeHelper.Primary : ThemeHelper.TextSecondary;
            button.ForeColor = isActive ? ThemeHelper.Primary : ThemeHelper.TextPrimary;
        }
    }

    private void LogoutButton_Click(object? sender, EventArgs e)
    {
        if (!MessageBoxHelper.Confirm("Are you sure you want to log out?"))
        {
            return;
        }

        LoggedOut?.Invoke(this, EventArgs.Empty);
        Close();
    }

    private sealed record NavigationItem(string Text, IconChar Icon);
}
