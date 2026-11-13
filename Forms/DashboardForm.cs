using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantPOS.Data;

namespace RestaurantPOS.Forms
{
    public class DashboardForm : Form
    {
        private readonly string _username;
        private readonly string _role;

        private Panel mainPanel;
        private Panel statsPanel;
        private Label lblWelcome;
        private Label lblSub;
        private Label lblQuickStart;
        private Button btnQuickOrder;
        private Button btnQuickMenu;
        private Button btnQuickReport;

        private const int SidebarWidth = 240;
        private const int ContentMaxWidth = 940;

        public DashboardForm(string username, string role)
        {
            _username = username;
            _role = role;
            InitializeComponent();
            LoadStats();
        }

        private void InitializeComponent()
        {
            this.Text = "Restaurant by Muzammil - Dashboard";
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(238, 241, 245);

            // ---------------- Sidebar ----------------
            var sidebar = new Panel { Dock = DockStyle.Left, Width = SidebarWidth };
            sidebar.Paint += Sidebar_Paint;

            // Every element below is stacked using its *measured* height, not a guessed
            // pixel gap, so nothing overlaps regardless of font/DPI on the target machine.
            int y = 26;

            var lblLogo = new Label
            {
                Text = "Muzammil POS",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                Location = new Point(22, y),
                AutoSize = true
            };
            y += TextRenderer.MeasureText(lblLogo.Text, lblLogo.Font).Height + 4;

            var lblTagline = new Label
            {
                Text = "Restaurant Management",
                ForeColor = Color.FromArgb(173, 181, 189),
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(22, y),
                AutoSize = true
            };
            y += TextRenderer.MeasureText(lblTagline.Text, lblTagline.Font).Height + 22;

            var lblUserText = new Label
            {
                Text = $"{_username}\n{_role}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Location = new Point(12, 8),
                AutoSize = true
            };
            var userTextSize = TextRenderer.MeasureText(lblUserText.Text, lblUserText.Font);

            var lblUser = new Panel
            {
                Location = new Point(18, y),
                Size = new Size(SidebarWidth - 36, (int)userTextSize.Height + 20),
                BackColor = Color.FromArgb(52, 58, 64)
            };
            RoundCorners(lblUser, 8);
            lblUser.Controls.Add(lblUserText);
            y += lblUser.Height + 30;

            var navButtons = new[]
            {
                ("New Order / Billing", (Action)(() => { new BillingForm(_username).ShowDialog(); LoadStats(); })),
                ("Manage Menu", (Action)(() => { new MenuManagementForm().ShowDialog(); LoadStats(); })),
                ("Daily Sales Report", (Action)(() => { new DailySalesReportForm().ShowDialog(); LoadStats(); })),
                ("Refresh Stats", (Action)(() => LoadStats())),
                ("Admin Panel", (Action)(() => { new AdminPanelForm().ShowDialog(); LoadStats(); })),
            };

            var navButtonControls = new System.Collections.Generic.List<Control>();
            foreach (var (text, action) in navButtons)
            {
                var btn = MakeSidebarButton(text, y);
                btn.Click += (s, e) => action();
                navButtonControls.Add(btn);
                y += btn.Height + 6;
            }

            var btnLogout = MakeSidebarButton("Logout", y + 120);
            btnLogout.Click += (s, e) => this.Close();

            sidebar.Controls.Add(lblLogo);
            sidebar.Controls.Add(lblTagline);
            sidebar.Controls.Add(lblUser);
            sidebar.Controls.AddRange(navButtonControls.ToArray());
            sidebar.Controls.Add(btnLogout);

            // ---------------- Main content (gradient background) ----------------
            mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(238, 241, 245) };
            mainPanel.Paint += MainPanel_Paint;

            lblWelcome = new Label
            {
                Text = $"Welcome back, {_username}!",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 30, 33),
                AutoSize = true
            };

            lblSub = new Label
            {
                Text = "Here's what's happening at your restaurant today.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = true
            };

            statsPanel = new Panel { Size = new Size(ContentMaxWidth, 140) };

            lblQuickStart = new Label
            {
                Text = "Quick actions",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 30, 33),
                AutoSize = true
            };

            btnQuickOrder = MakeQuickActionButton("Start New Order", Color.FromArgb(25, 135, 84));
            btnQuickOrder.Click += (s, e) => { new BillingForm(_username).ShowDialog(); LoadStats(); };

            btnQuickMenu = MakeQuickActionButton("Manage Menu Items", Color.FromArgb(13, 110, 253));
            btnQuickMenu.Click += (s, e) => { new MenuManagementForm().ShowDialog(); LoadStats(); };

            btnQuickReport = MakeQuickActionButton("Daily Sales Report", Color.FromArgb(111, 66, 193));
            btnQuickReport.Click += (s, e) => { new DailySalesReportForm().ShowDialog(); LoadStats(); };

            mainPanel.Controls.AddRange(new Control[]
            {
                lblWelcome, lblSub, statsPanel, lblQuickStart, btnQuickOrder, btnQuickMenu, btnQuickReport
            });

            this.Controls.Add(mainPanel);
            this.Controls.Add(sidebar);

            mainPanel.Resize += (s, e) => LayoutMainContent();
            this.Load += (s, e) => LayoutMainContent();
        }

        /// <summary>
        /// Centers a fixed-width content column inside the main panel so the dashboard
        /// looks intentional (not stretched-thin) on very wide/maximized windows.
        /// </summary>
        private void LayoutMainContent()
        {
            int available = mainPanel.ClientSize.Width - 48;
            int contentWidth = Math.Min(ContentMaxWidth, Math.Max(500, available));
            int x = 24 + Math.Max(0, (available - contentWidth) / 2);

            lblWelcome.Location = new Point(x, 30);
            lblSub.Location = new Point(x, lblWelcome.Bottom + 6);
            statsPanel.Location = new Point(x, lblSub.Bottom + 24);
            statsPanel.Width = contentWidth;

            lblQuickStart.Location = new Point(x, statsPanel.Bottom + 30);
            int btnY = lblQuickStart.Bottom + 14;
            int gap = 16;
            btnQuickOrder.Location = new Point(x, btnY);
            btnQuickMenu.Location = new Point(x + btnQuickOrder.Width + gap, btnY);
            btnQuickReport.Location = new Point(x + btnQuickOrder.Width + btnQuickMenu.Width + gap * 2, btnY);
        }

        private void Sidebar_Paint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            using var brush = new LinearGradientBrush(
                panel.ClientRectangle,
                Color.FromArgb(33, 37, 41),
                Color.FromArgb(24, 27, 30),
                LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, panel.ClientRectangle);
        }

        private void MainPanel_Paint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            using var brush = new LinearGradientBrush(
                panel.ClientRectangle,
                Color.FromArgb(240, 243, 247),
                Color.FromArgb(226, 232, 240),
                LinearGradientMode.ForwardDiagonal);
            e.Graphics.FillRectangle(brush, panel.ClientRectangle);
        }

        private Button MakeSidebarButton(string text, int y)
        {
            var btn = new Button
            {
                Text = "  " + text,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(33, 37, 41),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10.5f),
                Location = new Point(0, y),
                Size = new Size(SidebarWidth, 42),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 58, 64);
            return btn;
        }

        private Button MakeQuickActionButton(string text, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(280, 48),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            RoundCorners(btn, 10);
            return btn;
        }

        private void LoadStats()
        {
            statsPanel.Controls.Clear();
            var (totalOrders, totalSales, totalItems) = DatabaseHelper.GetDashboardStats();
            var (todayOrders, todaySales) = DatabaseHelper.GetTodayStats();

            int cardWidth = 210;
            int gap = 20;

            AddStatCard("Today's Sales", $"Rs. {todaySales:N0}", $"{todayOrders} order(s) today", Color.FromArgb(255, 143, 0), 0 * (cardWidth + gap));
            AddStatCard("Total Orders", totalOrders.ToString(), "all-time", Color.FromArgb(13, 110, 253), 1 * (cardWidth + gap));
            AddStatCard("Total Sales", $"Rs. {totalSales:N0}", "all-time", Color.FromArgb(25, 135, 84), 2 * (cardWidth + gap));
            AddStatCard("Menu Items", totalItems.ToString(), "on the menu", Color.FromArgb(220, 53, 69), 3 * (cardWidth + gap));
        }

        /// <summary>
        /// Adds one stat card directly to statsPanel (shadow, then the white card on top
        /// of it, both added straight to the real parent — no transparent wrapper panel,
        /// which is what previously caused the cards to render blank).
        /// </summary>
        private void AddStatCard(string title, string value, string subtitle, Color accent, int x)
        {
            int width = 210;
            int height = 130;

            var shadow = new Panel
            {
                Location = new Point(x + 4, 6),
                Size = new Size(width, height),
                BackColor = Color.FromArgb(214, 217, 222) // solid "shadow" tone (real alpha isn't supported on Panel)
            };
            RoundCorners(shadow, 14);
            statsPanel.Controls.Add(shadow);

            var card = new Panel
            {
                Location = new Point(x, 0),
                Size = new Size(width, height),
                BackColor = Color.White
            };
            RoundCorners(card, 14);

            var bar = new Panel { Location = new Point(0, 0), Size = new Size(width, 6), BackColor = accent };
            var lblTitle = new Label { Text = title.ToUpper(), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(16, 24), AutoSize = true };
            var lblValue = new Label { Text = value, Font = new Font("Segoe UI", 21, FontStyle.Bold), ForeColor = Color.FromArgb(30, 30, 30), Location = new Point(16, 48), AutoSize = true };
            var lblSubtitle = new Label { Text = subtitle, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(150, 150, 150), Location = new Point(16, 96), AutoSize = true };
            card.Controls.AddRange(new Control[] { bar, lblTitle, lblValue, lblSubtitle });

            statsPanel.Controls.Add(card);
            card.BringToFront(); // guarantee the card renders above its shadow
        }

        /// <summary>
        /// Clips a control to a rounded-rectangle region for a softer, modern look.
        /// </summary>
        private static void RoundCorners(Control control, int radius)
        {
            var rect = new Rectangle(0, 0, control.Width, control.Height);
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            control.Region = new Region(path);
        }
    }
}
