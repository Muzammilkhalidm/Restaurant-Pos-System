using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantPOS.Data;

namespace RestaurantPOS.Forms
{
    /// <summary>
    /// Read-only, date-by-date sales report. Anyone logged into the POS can view this —
    /// no admin credentials needed just to look at numbers. Deleting/clearing sales is
    /// only reachable from here via the protected Admin Panel button.
    /// </summary>
    public class DailySalesReportForm : Form
    {
        private DataGridView gridDaily;
        private Label lblTodayValue;
        private Label lblAllTimeValue;

        public DailySalesReportForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Restaurant by Muzammil - Daily Sales Report";
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 246, 248);

            var lblHeader = new Label
            {
                Text = "Daily Sales Report",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(24, 20),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = "Sales grouped by calendar day, most recent first.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.Gray,
                Location = new Point(24, 54),
                AutoSize = true
            };

            // ---- Summary cards ----
            var todayCard = MakeSummaryCard("TODAY'S SALES", out lblTodayValue, Color.FromArgb(25, 135, 84), new Point(24, 90));
            var allTimeCard = MakeSummaryCard("ALL-TIME SALES", out lblAllTimeValue, Color.FromArgb(13, 110, 253), new Point(254, 90));

            // ---- Daily breakdown grid ----
            var lblGridHeader = new Label
            {
                Text = "Sales by Date",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(24, 210),
                AutoSize = true
            };

            gridDaily = new DataGridView
            {
                Location = new Point(24, 240),
                Size = new Size(700, 330),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(24, 585),
                Size = new Size(120, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadData();

            var btnAdminPanel = new Button
            {
                Text = "Open Admin Panel (Clear / Delete Sales)",
                Location = new Point(154, 585),
                Size = new Size(300, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.FromArgb(33, 37, 41),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnAdminPanel.FlatAppearance.BorderSize = 0;
            btnAdminPanel.Click += (s, e) =>
            {
                using var admin = new AdminPanelForm();
                admin.ShowDialog();
                LoadData(); // sales may have changed while the admin panel was open
            };

            var btnClose = new Button
            {
                Text = "Close",
                Size = new Size(110, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                lblHeader, lblSub, todayCard, allTimeCard,
                lblGridHeader, gridDaily, btnRefresh, btnAdminPanel, btnClose
            });

            this.Resize += (s, e) =>
            {
                gridDaily.Width = Math.Max(400, this.ClientSize.Width - 48);
                gridDaily.Height = Math.Max(200, this.ClientSize.Height - gridDaily.Top - 60);
                btnClose.Location = new Point(this.ClientSize.Width - 24 - btnClose.Width, this.ClientSize.Height - 51);
            };
        }

        private Panel MakeSummaryCard(string title, out Label valueLabel, Color accent, Point location)
        {
            var panel = new Panel
            {
                Location = location,
                Size = new Size(210, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var bar = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = accent };
            var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(14, 22), AutoSize = true };
            var lblValue = new Label { Text = "Rs. 0", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(30, 30, 30), Location = new Point(14, 46), AutoSize = true };

            panel.Controls.AddRange(new Control[] { bar, lblTitle, lblValue });
            valueLabel = lblValue;
            return panel;
        }

        private void LoadData()
        {
            var (todayOrders, todaySales) = DatabaseHelper.GetTodayStats();
            lblTodayValue.Text = $"Rs. {todaySales:N0}";

            var daily = DatabaseHelper.GetDailySales();
            decimal allTime = daily.Sum(d => d.TotalSales);
            lblAllTimeValue.Text = $"Rs. {allTime:N0}";

            gridDaily.DataSource = null;
            gridDaily.DataSource = daily.Select(d => new
            {
                Date = DateTime.Parse(d.Date).ToString("dd-MMM-yyyy (dddd)"),
                Orders = d.OrderCount,
                TotalSales = $"Rs. {d.TotalSales:N0}"
            }).ToList();
        }
    }
}
