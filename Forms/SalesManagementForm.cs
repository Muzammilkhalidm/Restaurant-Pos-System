using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantPOS.Data;

namespace RestaurantPOS.Forms
{
    /// <summary>
    /// Reached only through AdminPanelForm. Lets the admin either delete one specific
    /// order, or wipe all sales history entirely. Menu items are never affected here.
    /// </summary>
    public class SalesManagementForm : Form
    {
        private DataGridView grid;
        private Label lblSummary;
        private Button btnDeleteOne;
        private Button btnRefresh;
        private Button btnClearAll;
        private Button btnClose;

        public SalesManagementForm()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void InitializeComponent()
        {
            this.Text = "Admin Panel - Sales Management";
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 246, 248);

            var lblHeader = new Label
            {
                Text = "Sales Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(24, 20),
                AutoSize = true
            };

            lblSummary = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(24, 58),
                AutoSize = true
            };

            var lblHint = new Label
            {
                Text = "Select a row and click \"Delete Selected Sale\" to remove just that one order — every other sale stays untouched.",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(24, 82),
                AutoSize = true
            };

            grid = new DataGridView
            {
                Location = new Point(24, 112),
                Size = new Size(900, 420),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false
            };

            btnDeleteOne = new Button
            {
                Text = "Delete Selected Sale",
                Size = new Size(200, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnDeleteOne.FlatAppearance.BorderSize = 0;
            btnDeleteOne.Click += BtnDeleteOne_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Size = new Size(110, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadOrders();

            btnClearAll = new Button
            {
                Text = "Clear ALL Sales Data",
                Size = new Size(220, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnClearAll.FlatAppearance.BorderSize = 0;
            btnClearAll.Click += BtnClearAll_Click;

            btnClose = new Button
            {
                Text = "Close",
                Size = new Size(120, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblHeader, lblSummary, lblHint, grid, btnDeleteOne, btnRefresh, btnClearAll, btnClose });

            this.Resize += (s, e) => LayoutBottomBar();
            this.Load += (s, e) => LayoutBottomBar();
        }

        /// <summary>
        /// Keeps the button row pinned to the bottom of whatever size the window is,
        /// so this screen fills the full window just like the rest of the app.
        /// </summary>
        private void LayoutBottomBar()
        {
            int bottomY = this.ClientSize.Height - 56;
            btnDeleteOne.Location = new Point(24, bottomY);
            btnRefresh.Location = new Point(24 + btnDeleteOne.Width + 12, bottomY);

            btnClose.Location = new Point(this.ClientSize.Width - 24 - btnClose.Width, bottomY);
            btnClearAll.Location = new Point(btnClose.Left - 12 - btnClearAll.Width, bottomY);

            grid.Height = Math.Max(200, bottomY - grid.Top - 16);
            grid.Width = Math.Max(400, this.ClientSize.Width - 48);
        }

        private void LoadOrders()
        {
            var orders = DatabaseHelper.GetAllOrders();

            grid.DataSource = null;
            grid.DataSource = orders.Select(o => new
            {
                o.Id,
                Date = o.OrderDate.ToString("dd-MMM-yyyy hh:mm tt"),
                Customer = o.CustomerName,
                Cashier = o.CashierUsername,
                Total = $"Rs. {o.TotalAmount:N0}"
            }).ToList();

            decimal totalSales = orders.Sum(o => o.TotalAmount);
            lblSummary.Text = $"{orders.Count} order(s) on file   —   Rs. {totalSales:N0} total sales";
        }

        private void BtnDeleteOne_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a sale from the list first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = grid.SelectedRows[0];
            int orderId = Convert.ToInt32(row.Cells["Id"].Value);
            string customer = row.Cells["Customer"].Value.ToString();
            string total = row.Cells["Total"].Value.ToString();

            var confirm = MessageBox.Show(
                $"Delete sale #{orderId} for \"{customer}\" ({total})?\n\nOnly this one sale will be removed — all other sales stay untouched.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                DatabaseHelper.DeleteOrder(orderId);
                LoadOrders();
                MessageBox.Show("That sale has been deleted.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            var firstConfirm = MessageBox.Show(
                "This will permanently delete EVERY sale on record (all order history).\n\nMenu items are not affected. This cannot be undone.\n\nContinue?",
                "Clear All Sales Data",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (firstConfirm != DialogResult.Yes) return;

            var typed = Microsoft.VisualBasic.Interaction.InputBox(
                "Type CLEAR to confirm wiping all sales data:",
                "Final Confirmation",
                "");

            if (typed.Trim().Equals("CLEAR", StringComparison.OrdinalIgnoreCase))
            {
                DatabaseHelper.ClearAllSales();
                LoadOrders();
                MessageBox.Show("All sales data has been cleared.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Confirmation text did not match. Nothing was deleted.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
