using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantPOS.Data;
using RestaurantPOS.Models;

namespace RestaurantPOS.Forms
{
    public class BillingForm : Form
    {
        private readonly string _cashier;
        private List<MenuItem> allItems;
        private readonly List<OrderItem> cart = new List<OrderItem>();

        // Top bar
        private Panel customerPanel;
        private Label lblCustomer;
        private TextBox txtCustomer;
        private TextBox txtSearch;
        private ComboBox cmbCategory;

        // Main content
        private Label lblMenuHeader;
        private ListBox lstMenu;
        private Label lblCartHeader;
        private DataGridView gridCart;

        // Bottom bar
        private Button btnRemove;
        private Button btnClearCart;
        private Label lblSubtotal, lblTax, lblTotal;
        private Button btnPay;

        private const int EdgeGap = 20;

        public BillingForm(string cashier)
        {
            _cashier = cashier;
            InitializeComponent();
            allItems = DatabaseHelper.GetAllMenuItems();
            LoadCategoryFilter();
            PopulateMenuList();
        }

        private void InitializeComponent()
        {
            this.Text = "Restaurant by Muzammil - New Order";
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 246, 248);

            // ---------------- Customer name (prominent) ----------------
            customerPanel = new Panel
            {
                BackColor = Color.FromArgb(255, 243, 205),   // warm highlight so it stands out
                BorderStyle = BorderStyle.None
            };

            lblCustomer = new Label
            {
                Text = "CUSTOMER NAME  *",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(133, 100, 4),
                AutoSize = true,
                Location = new Point(14, 6)
            };

            txtCustomer = new TextBox
            {
                Location = new Point(14, 26),
                Size = new Size(360, 34),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle
            };

            customerPanel.Controls.AddRange(new Control[] { lblCustomer, txtCustomer });
            customerPanel.Size = new Size(390, 74);

            // ---------------- Search / category ----------------
            var lblSearch = new Label { Text = "Search Item:", AutoSize = true, Font = new Font("Segoe UI", 9.5f) };
            txtSearch = new TextBox { Size = new Size(220, 28), Font = new Font("Segoe UI", 10) };
            txtSearch.TextChanged += (s, e) => PopulateMenuList();

            var lblCat = new Label { Text = "Category:", AutoSize = true, Font = new Font("Segoe UI", 9.5f) };
            cmbCategory = new ComboBox { Size = new Size(210, 28), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.SelectedIndexChanged += (s, e) => PopulateMenuList();

            // ---------------- Menu list ----------------
            lblMenuHeader = new Label { Text = "Menu Items  (double-click to add)", AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            lstMenu = new ListBox
            {
                Font = new Font("Segoe UI", 10.5f),
                ItemHeight = TextRenderer.MeasureText("Ag", new Font("Segoe UI", 10.5f)).Height + 10,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.FixedSingle
            };
            lstMenu.DrawItem += LstMenu_DrawItem;
            lstMenu.DoubleClick += LstMenu_DoubleClick;

            // ---------------- Cart ----------------
            lblCartHeader = new Label { Text = "Current Order  (double-click a line to change qty)", AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            gridCart = new DataGridView
            {
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            gridCart.Columns.Add("Name", "Item");
            gridCart.Columns.Add("Price", "Price");
            gridCart.Columns.Add("Qty", "Qty");
            gridCart.Columns.Add("Total", "Total");
            gridCart.CellDoubleClick += GridCart_CellDoubleClick;

            // ---------------- Bottom action bar ----------------
            btnRemove = new Button { Text = "Remove Selected Line", Size = new Size(190, 34), FlatStyle = FlatStyle.Flat };
            btnRemove.Click += BtnRemove_Click;

            btnClearCart = new Button { Text = "Clear Order", Size = new Size(130, 34), FlatStyle = FlatStyle.Flat };
            btnClearCart.Click += (s, e) => { cart.Clear(); RefreshCart(); };

            lblSubtotal = new Label { Text = "Subtotal: Rs. 0", AutoSize = true, Font = new Font("Segoe UI", 10.5f) };
            lblTax = new Label { Text = "Tax (5%): Rs. 0", AutoSize = true, Font = new Font("Segoe UI", 10.5f) };
            lblTotal = new Label { Text = "Total: Rs. 0", AutoSize = true, Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Color.FromArgb(220, 53, 69) };

            btnPay = new Button
            {
                Text = "Pay & Print Receipt",
                Size = new Size(230, 48),
                BackColor = Color.FromArgb(25, 135, 84),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += BtnPay_Click;

            this.Controls.AddRange(new Control[]
            {
                customerPanel, lblSearch, txtSearch, lblCat, cmbCategory,
                lblMenuHeader, lstMenu, lblCartHeader, gridCart,
                btnRemove, btnClearCart, lblSubtotal, lblTax, lblTotal, btnPay
            });

            this.Resize += (s, e) => LayoutControls();
            this.Load += (s, e) => LayoutControls();
            this.Shown += (s, e) => LayoutControls();
        }

        /// <summary>
        /// Recomputes every control's position/size from the form's current ClientSize
        /// so the screen always fills the full window, at any resolution or window size.
        /// </summary>
        private void LayoutControls()
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            if (w <= 0 || h <= 0) return;

            // ---- Top bar ----
            customerPanel.Location = new Point(EdgeGap, 12);

            int topRowY = 34;
            int searchX = customerPanel.Right + 30;
            var lblSearch = this.Controls.OfType<Label>().First(l => l.Text == "Search Item:");
            lblSearch.Location = new Point(searchX, topRowY);
            txtSearch.Location = new Point(searchX + 90, topRowY - 3);

            int catX = txtSearch.Right + 30;
            var lblCat = this.Controls.OfType<Label>().First(l => l.Text == "Category:");
            lblCat.Location = new Point(catX, topRowY);
            cmbCategory.Location = new Point(catX + 75, topRowY - 3);

            int contentTop = customerPanel.Bottom + 15;
            int bottomBarHeight = 110;
            int listAreaHeight = h - contentTop - bottomBarHeight - EdgeGap;
            if (listAreaHeight < 150) listAreaHeight = 150;

            int halfWidth = (w - EdgeGap * 3) / 2;

            // ---- Menu list (left) ----
            lblMenuHeader.Location = new Point(EdgeGap, contentTop);
            lstMenu.Location = new Point(EdgeGap, contentTop + 25);
            lstMenu.Size = new Size(halfWidth, listAreaHeight - 25);

            // ---- Cart grid (right) ----
            int cartX = EdgeGap * 2 + halfWidth;
            lblCartHeader.Location = new Point(cartX, contentTop);
            gridCart.Location = new Point(cartX, contentTop + 25);
            gridCart.Size = new Size(w - cartX - EdgeGap, listAreaHeight - 25);

            // ---- Bottom bar ----
            int bottomY = h - bottomBarHeight + 10;
            btnRemove.Location = new Point(cartX, bottomY);
            btnClearCart.Location = new Point(cartX + btnRemove.Width + 10, bottomY);

            int totalsY = bottomY + 44;
            lblSubtotal.Location = new Point(cartX, totalsY);
            lblTax.Location = new Point(cartX, totalsY + 24);
            lblTotal.Location = new Point(cartX, totalsY + 48);

            btnPay.Location = new Point(w - EdgeGap - btnPay.Width, totalsY + 8);
        }

        private void LoadCategoryFilter()
        {
            var categories = DatabaseHelper.GetCategories();
            cmbCategory.Items.Clear();
            cmbCategory.Items.AddRange(categories.ToArray());
            cmbCategory.SelectedIndex = 0;
        }

        private void PopulateMenuList()
        {
            var filtered = allItems.AsEnumerable();

            if (cmbCategory.SelectedItem != null && cmbCategory.SelectedItem.ToString() != "All")
                filtered = filtered.Where(i => i.Category == cmbCategory.SelectedItem.ToString());

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                filtered = filtered.Where(i => i.Name.ToLower().Contains(txtSearch.Text.ToLower()));

            lstMenu.Items.Clear();
            foreach (var item in filtered.OrderBy(i => i.Name))
                lstMenu.Items.Add(item);
        }

        // Custom draw so each row shows "Name  —  Rs. price"
        private void LstMenu_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            var item = (MenuItem)lstMenu.Items[e.Index];
            string text = $"{item.Name}   —   Rs. {item.Price:N0}";
            int textHeight = TextRenderer.MeasureText(text, e.Font).Height;
            int yOffset = e.Bounds.Top + Math.Max(0, (e.Bounds.Height - textHeight) / 2);
            using var brush = new SolidBrush(e.ForeColor);
            e.Graphics.DrawString(text, e.Font, brush, e.Bounds.Left + 6, yOffset);
            e.DrawFocusRectangle();
        }

        private void LstMenu_DoubleClick(object sender, EventArgs e)
        {
            if (lstMenu.SelectedItem is MenuItem selected)
            {
                var existing = cart.FirstOrDefault(c => c.MenuItemId == selected.Id);
                if (existing != null)
                    existing.Quantity++;
                else
                    cart.Add(new OrderItem { MenuItemId = selected.Id, Name = selected.Name, Price = selected.Price, Quantity = 1 });

                RefreshCart();
            }
        }

        private void GridCart_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= cart.Count) return;
            var line = cart[e.RowIndex];

            using var inputForm = new Form
            {
                Text = "Update Quantity",
                Size = new Size(280, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var lbl = new Label { Text = $"Quantity for {line.Name}:", Location = new Point(20, 20), AutoSize = true };
            var num = new NumericUpDown { Minimum = 1, Maximum = 100, Value = line.Quantity, Location = new Point(20, 50), Size = new Size(100, 26) };
            var btnOk = new Button { Text = "Update", Location = new Point(140, 50), Size = new Size(100, 26), DialogResult = DialogResult.OK };
            inputForm.Controls.AddRange(new Control[] { lbl, num, btnOk });
            inputForm.AcceptButton = btnOk;

            if (inputForm.ShowDialog() == DialogResult.OK)
            {
                line.Quantity = (int)num.Value;
                RefreshCart();
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (gridCart.SelectedRows.Count == 0) return;
            int index = gridCart.SelectedRows[0].Index;
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                RefreshCart();
            }
        }

        private void RefreshCart()
        {
            gridCart.Rows.Clear();
            foreach (var line in cart)
                gridCart.Rows.Add(line.Name, $"Rs. {line.Price:N0}", line.Quantity, $"Rs. {line.Total:N0}");

            decimal subtotal = cart.Sum(c => c.Total);
            decimal tax = Math.Round(subtotal * 0.05m, 0);
            decimal total = subtotal + tax;

            lblSubtotal.Text = $"Subtotal: Rs. {subtotal:N0}";
            lblTax.Text = $"Tax (5%): Rs. {tax:N0}";
            lblTotal.Text = $"Total: Rs. {total:N0}";
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomer.Text))
            {
                MessageBox.Show("Please enter customer name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomer.Focus();
                return;
            }
            if (cart.Count == 0)
            {
                MessageBox.Show("Cart is empty. Add at least one item.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal subtotal = cart.Sum(c => c.Total);
            decimal tax = Math.Round(subtotal * 0.05m, 0);
            decimal total = subtotal + tax;

            int orderId = DatabaseHelper.SaveOrder(txtCustomer.Text.Trim(), _cashier, total, cart);

            using var receipt = new ReceiptForm(orderId, txtCustomer.Text.Trim(), _cashier, cart.ToList(), subtotal, tax, total);
            receipt.ShowDialog();

            cart.Clear();
            txtCustomer.Clear();
            RefreshCart();
        }
    }
}
