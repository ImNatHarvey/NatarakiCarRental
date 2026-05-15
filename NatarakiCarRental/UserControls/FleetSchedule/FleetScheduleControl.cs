using System.Drawing.Drawing2D;
using FontAwesome.Sharp;
using NatarakiCarRental.Forms.FleetSchedule;
using NatarakiCarRental.Helpers;
using NatarakiCarRental.Models;
using NatarakiCarRental.Services;

namespace NatarakiCarRental.UserControls.FleetSchedule;

public sealed class FleetScheduleControl : UserControl
{
    private const int CarColumnWidth = 210;
    private const int HeaderHeight = 46;
    private const int RowHeight = 52;
    private const int DayWidth = 42;
    private readonly int _currentUserId;
    private readonly CarService _carService;
    private readonly FleetScheduleService _scheduleService;
    private readonly Label _monthLabel = new();
    private readonly TimelineCanvas _timelineCanvas;
    private readonly ToolTip _toolTip = new();
    private int? _hoveredScheduleId;
    private DateTime _selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private IReadOnlyList<Car> _cars = [];
    private IReadOnlyList<Models.FleetSchedule> _schedules = [];
    private DateTime SelectedMonth => _selectedMonth;

    public FleetScheduleControl(int currentUserId)
    {
        _currentUserId = currentUserId;
        _carService = new CarService(currentUserId);
        _scheduleService = new FleetScheduleService(currentUserId);
        _timelineCanvas = new TimelineCanvas(this);
        InitializeControl();
        Load += FleetScheduleControl_Load;
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
            RowCount = 3
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        mainLayout.Controls.Add(CreateHeaderPanel(), 0, 0);
        mainLayout.Controls.Add(CreateToolbarPanel(), 0, 1);
        mainLayout.Controls.Add(CreateTimelineHost(), 0, 2);
        Controls.Add(mainLayout);
    }

    private static Panel CreateHeaderPanel()
    {
        Panel panel = new() { Dock = DockStyle.Fill, BackColor = ThemeHelper.ContentBackground };
        panel.Controls.Add(new Label
        {
            Text = "Fleet Schedule",
            AutoSize = false,
            Location = new Point(0, 0),
            Size = new Size(280, 34),
            Font = FontHelper.Title(22F),
            ForeColor = ThemeHelper.TextPrimary
        });
        panel.Controls.Add(new Label
        {
            Text = "Visual monthly planning board for reservations, rentals, and maintenance.",
            AutoSize = false,
            Location = new Point(2, 42),
            Size = new Size(680, 24),
            Font = FontHelper.Regular(10.5F),
            ForeColor = ThemeHelper.TextSecondary
        });
        return panel;
    }

    private Panel CreateToolbarPanel()
    {
        Panel panel = new() { Dock = DockStyle.Fill, BackColor = ThemeHelper.ContentBackground };
        Button previousButton = CreateSecondaryButton("<", 38, 34);
        previousButton.Location = new Point(0, 10);
        previousButton.Click += async (_, _) => await ChangeMonthAsync(-1);

        _monthLabel.Location = new Point(48, 10);
        _monthLabel.Size = new Size(180, 34);
        _monthLabel.Font = FontHelper.Title(12F);
        _monthLabel.ForeColor = ThemeHelper.TextPrimary;
        _monthLabel.TextAlign = ContentAlignment.MiddleCenter;

        Button nextButton = CreateSecondaryButton(">", 38, 34);
        nextButton.Location = new Point(238, 10);
        nextButton.Click += async (_, _) => await ChangeMonthAsync(1);

        Button todayButton = CreateSecondaryButton("Today", 84, 34);
        todayButton.Location = new Point(288, 10);
        todayButton.Click += async (_, _) =>
        {
            _selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            await LoadBoardAsync();
        };

        Button addButton = ControlFactory.CreatePrimaryButton("Add Schedule", 132, 36);
        addButton.Location = new Point(390, 9);
        addButton.Click += async (_, _) => await OpenAddFormAsync(null, null);

        panel.Controls.Add(previousButton);
        panel.Controls.Add(_monthLabel);
        panel.Controls.Add(nextButton);
        panel.Controls.Add(todayButton);
        panel.Controls.Add(addButton);
        panel.Controls.Add(CreateLegendPanel());
        return panel;
    }

    private Panel CreateLegendPanel()
    {
        FlowLayoutPanel panel = new()
        {
            Location = new Point(544, 8),
            Size = new Size(510, 38),
            BackColor = ThemeHelper.ContentBackground,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 7, 0, 0)
        };

        AddLegendItem(panel, "Pending", FleetScheduleVisualHelper.GetColor(FleetScheduleConstants.Status.Pending));
        AddLegendItem(panel, "Reserved", FleetScheduleVisualHelper.GetColor(FleetScheduleConstants.Status.Confirmed));
        AddLegendItem(panel, "Rented", FleetScheduleVisualHelper.GetColor(FleetScheduleConstants.Status.Active));
        AddLegendItem(panel, "Maintenance", FleetScheduleVisualHelper.GetColor(FleetScheduleConstants.Status.Active, FleetScheduleConstants.Type.Maintenance));
        AddLegendItem(panel, "Cancelled", FleetScheduleVisualHelper.GetColor(FleetScheduleConstants.Status.Cancelled));
        return panel;
    }

    private static void AddLegendItem(FlowLayoutPanel panel, string text, Color color)
    {
        Panel itemPanel = new()
        {
            Size = new Size(text == "Maintenance" ? 112 : 88, 24),
            Margin = new Padding(0, 0, 8, 0),
            BackColor = ThemeHelper.ContentBackground
        };
        RoundedLegendMarker swatch = new(color)
        {
            Location = new Point(0, 5),
            Size = new Size(14, 14)
        };
        Label label = new()
        {
            Text = text,
            Location = new Point(22, 1),
            Size = new Size(itemPanel.Width - 22, 22),
            Font = FontHelper.Regular(9F),
            ForeColor = ThemeHelper.TextSecondary
        };
        itemPanel.Controls.Add(swatch);
        itemPanel.Controls.Add(label);
        panel.Controls.Add(itemPanel);
    }

    private Control CreateTimelineHost()
    {
        Panel card = ControlFactory.CreateCardPanel(new Size(0, 0));
        card.Dock = DockStyle.Fill;
        card.Padding = new Padding(12);

        _timelineCanvas.Dock = DockStyle.Fill;
        _timelineCanvas.AutoScroll = true;
        _timelineCanvas.BackColor = ThemeHelper.Surface;
        _timelineCanvas.MouseMove += TimelineCanvas_MouseMove;
        _timelineCanvas.MouseClick += TimelineCanvas_MouseClick;
        card.Controls.Add(_timelineCanvas);
        return card;
    }

    private async void FleetScheduleControl_Load(object? sender, EventArgs e)
    {
        Load -= FleetScheduleControl_Load;
        await LoadBoardAsync();
    }

    private async Task ChangeMonthAsync(int months)
    {
        _selectedMonth = _selectedMonth.AddMonths(months);
        await LoadBoardAsync();
    }

    private async Task LoadBoardAsync()
    {
        try
        {
            _monthLabel.Text = _selectedMonth.ToString("MMMM yyyy");
            _cars = await _carService.GetActiveCarsAsync();
            _schedules = await _scheduleService.GetSchedulesForMonthAsync(_selectedMonth.Year, _selectedMonth.Month);
            _timelineCanvas.UpdateVirtualSize();
            _timelineCanvas.Invalidate();
        }
        catch (Exception exception)
        {
            MessageBoxHelper.ShowError($"Unable to load fleet schedule.\n\n{exception.Message}", "Fleet Schedule");
        }
    }

    private async Task OpenAddFormAsync(int? carId, DateTime? date)
    {
        using FleetScheduleDetailsForm form = new(FleetScheduleFormMode.Add, _currentUserId, prefilledCarId: carId, prefilledDate: date);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            await LoadBoardAsync();
        }
    }

    private async Task OpenEditFormAsync(Models.FleetSchedule schedule)
    {
        using FleetScheduleDetailsForm form = new(FleetScheduleFormMode.Edit, _currentUserId, schedule);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            await LoadBoardAsync();
        }
    }

    private void TimelineCanvas_MouseMove(object? sender, MouseEventArgs e)
    {
        Models.FleetSchedule? schedule = _timelineCanvas.GetScheduleAt(e.Location);
        _timelineCanvas.Cursor = schedule is null ? Cursors.Default : Cursors.Hand;
        int? nextScheduleId = schedule?.ScheduleId;

        if (_hoveredScheduleId != nextScheduleId)
        {
            _hoveredScheduleId = nextScheduleId;
            _toolTip.SetToolTip(
                _timelineCanvas,
                schedule is null
                    ? null
                    : $"{schedule.CarName} ({schedule.PlateNumber})\n{schedule.Title}\n{schedule.StartDate:MMM d} - {schedule.EndDate:MMM d}\n{FleetScheduleVisualHelper.GetDisplayStatus(schedule.Status, schedule.ScheduleType)}");
        }
    }

    private async void TimelineCanvas_MouseClick(object? sender, MouseEventArgs e)
    {
        Models.FleetSchedule? schedule = _timelineCanvas.GetScheduleAt(e.Location);
        if (schedule is not null)
        {
            await OpenEditFormAsync(schedule);
            return;
        }

        (Car? car, DateTime? date) = _timelineCanvas.GetCellAt(e.Location);
        if (car is not null && date.HasValue)
        {
            await OpenAddFormAsync(car.CarId, date.Value);
        }
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

    private sealed class TimelineCanvas : Panel
    {
        private readonly FleetScheduleControl _owner;
        private readonly Dictionary<int, Rectangle> _scheduleBounds = [];

        public TimelineCanvas(FleetScheduleControl owner)
        {
            _owner = owner;
            DoubleBuffered = true;
        }

        public void UpdateVirtualSize()
        {
            int days = DateTime.DaysInMonth(_owner.SelectedMonth.Year, _owner.SelectedMonth.Month);
            int width = CarColumnWidth + days * DayWidth;
            int height = HeaderHeight + Math.Max(_owner._cars.Count, 1) * RowHeight;
            AutoScrollMinSize = new Size(width, height);
        }

        public Models.FleetSchedule? GetScheduleAt(Point point)
        {
            Point translated = TranslatePoint(point);
            int? scheduleId = _scheduleBounds.FirstOrDefault(pair => pair.Value.Contains(translated)).Key;
            return scheduleId is null or 0
                ? null
                : _owner._schedules.FirstOrDefault(schedule => schedule.ScheduleId == scheduleId);
        }

        public (Car? Car, DateTime? Date) GetCellAt(Point point)
        {
            Point translated = TranslatePoint(point);
            if (translated.X < CarColumnWidth || translated.Y < HeaderHeight)
            {
                return (null, null);
            }

            int carIndex = (translated.Y - HeaderHeight) / RowHeight;
            int dayIndex = (translated.X - CarColumnWidth) / DayWidth;
            if (carIndex < 0 || carIndex >= _owner._cars.Count)
            {
                return (null, null);
            }

            int days = DateTime.DaysInMonth(_owner.SelectedMonth.Year, _owner.SelectedMonth.Month);
            if (dayIndex < 0 || dayIndex >= days)
            {
                return (null, null);
            }

            return (_owner._cars[carIndex], _owner.SelectedMonth.AddDays(dayIndex));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);
            _scheduleBounds.Clear();

            int days = DateTime.DaysInMonth(_owner.SelectedMonth.Year, _owner.SelectedMonth.Month);
            using Pen gridPen = new(Color.FromArgb(226, 232, 240));
            using SolidBrush headerBrush = new(ThemeHelper.ContentBackground);
            using SolidBrush surfaceBrush = new(ThemeHelper.Surface);
            using SolidBrush textBrush = new(ThemeHelper.TextPrimary);
            using SolidBrush mutedBrush = new(ThemeHelper.TextSecondary);
            using SolidBrush weekendBrush = new(Color.FromArgb(248, 250, 252));
            using SolidBrush todayBrush = new(Color.FromArgb(239, 246, 255));

            graphics.FillRectangle(surfaceBrush, 0, 0, AutoScrollMinSize.Width, AutoScrollMinSize.Height);
            graphics.FillRectangle(headerBrush, 0, 0, AutoScrollMinSize.Width, HeaderHeight);
            graphics.FillRectangle(headerBrush, 0, 0, CarColumnWidth, AutoScrollMinSize.Height);

            graphics.DrawString("Car", FontHelper.SemiBold(9F), textBrush, new PointF(14, 15));
            for (int day = 0; day < days; day++)
            {
                int x = CarColumnWidth + day * DayWidth;
                DateTime date = _owner.SelectedMonth.AddDays(day);
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    graphics.FillRectangle(weekendBrush, x, HeaderHeight, DayWidth, AutoScrollMinSize.Height - HeaderHeight);
                }

                if (date.Date == DateTime.Today)
                {
                    graphics.FillRectangle(todayBrush, x, 0, DayWidth, AutoScrollMinSize.Height);
                }

                graphics.DrawLine(gridPen, x, 0, x, AutoScrollMinSize.Height);
                graphics.DrawString(date.Day.ToString(), FontHelper.SemiBold(9F), textBrush, new PointF(x + 13, 8));
                graphics.DrawString(date.ToString("ddd"), FontHelper.Regular(8F), mutedBrush, new PointF(x + 10, 24));
            }

            graphics.DrawLine(gridPen, 0, HeaderHeight, AutoScrollMinSize.Width, HeaderHeight);
            if (_owner._cars.Count == 0)
            {
                const string message = "No active cars available. Add cars in Car Garage first.";
                SizeF messageSize = graphics.MeasureString(message, FontHelper.Regular(11F));
                float x = Math.Max((ClientSize.Width - messageSize.Width) / 2 - AutoScrollPosition.X, 24);
                float y = HeaderHeight + Math.Max((ClientSize.Height - HeaderHeight - messageSize.Height) / 2 - AutoScrollPosition.Y, 28);
                graphics.DrawString(message, FontHelper.Regular(11F), mutedBrush, new PointF(x, y));
                return;
            }

            for (int row = 0; row < _owner._cars.Count; row++)
            {
                int y = HeaderHeight + row * RowHeight;
                Car car = _owner._cars[row];
                graphics.DrawLine(gridPen, 0, y, AutoScrollMinSize.Width, y);
                graphics.DrawString(car.CarName, FontHelper.SemiBold(9F), textBrush, new PointF(14, y + 10));
                graphics.DrawString(car.PlateNumber, FontHelper.Regular(8.5F), mutedBrush, new PointF(14, y + 28));
            }

            foreach (Models.FleetSchedule schedule in _owner._schedules)
            {
                int row = _owner._cars.ToList().FindIndex(car => car.CarId == schedule.CarId);
                if (row < 0)
                {
                    continue;
                }

                DateTime visibleStart = schedule.StartDate < _owner.SelectedMonth ? _owner.SelectedMonth : schedule.StartDate;
                DateTime monthEnd = _owner.SelectedMonth.AddMonths(1).AddDays(-1);
                DateTime visibleEnd = schedule.EndDate > monthEnd ? monthEnd : schedule.EndDate;
                int startDay = visibleStart.Day - 1;
                int durationDays = (visibleEnd - visibleStart).Days + 1;
                Rectangle rect = new(
                    CarColumnWidth + startDay * DayWidth + 4,
                    HeaderHeight + row * RowHeight + 10,
                    Math.Max(durationDays * DayWidth - 8, 20),
                    30);
                _scheduleBounds[schedule.ScheduleId] = rect;

                using GraphicsPath path = GetRoundedRect(rect, 12);
                using SolidBrush fillBrush = new(FleetScheduleVisualHelper.GetColor(schedule.Status, schedule.ScheduleType));
                graphics.FillPath(fillBrush, path);

                string displayStatus = FleetScheduleVisualHelper.GetDisplayStatus(schedule.Status, schedule.ScheduleType);
                string text = rect.Width >= 80 ? $"{schedule.Title} ({displayStatus})" : displayStatus;
                using StringFormat format = new()
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                graphics.DrawString(text, FontHelper.SemiBold(8.5F), Brushes.White, new RectangleF(rect.X + 8, rect.Y, rect.Width - 12, rect.Height), format);
            }
        }

        private Point TranslatePoint(Point point)
        {
            return new Point(point.X - AutoScrollPosition.X, point.Y - AutoScrollPosition.Y);
        }

        private static GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    private sealed class RoundedLegendMarker : Control
    {
        private readonly Color _fillColor;

        public RoundedLegendMarker(Color fillColor)
        {
            _fillColor = fillColor;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using GraphicsPath path = new();
            path.AddArc(0, 0, Width, Height, 180, 90);
            path.AddArc(Width - Height, 0, Height, Height, 270, 90);
            path.AddArc(Width - Height, Height - Height, Height, Height, 0, 90);
            path.AddArc(0, Height - Height, Height, Height, 90, 90);
            path.CloseFigure();
            using SolidBrush brush = new(_fillColor);
            e.Graphics.FillPath(brush, path);
        }
    }
}
