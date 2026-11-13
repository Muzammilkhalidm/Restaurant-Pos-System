using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantPOS.Data;
using RestaurantPOS.Models;

namespace RestaurantPOS.Forms
{
    public class MenuManagementForm : Form
    {
        private DataGridView grid;
        private TextBox txtName;
        private ComboBox cmbCategory;
        private TextBox txtPrice;
        private TextBox txtSearch;
        private ComboBox cmbFilterCategory;
        private int selectedId = 0;

        public MenuManagementForm()
        {
            InitializeComponent();
            LoadCategories();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Restaurant by Muzammil - Manage Menu";
            this.MinimumSize = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            var lblSearch = new Label { Text = "Search:", Location = new Point(20, 20), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(80, 17), Size = new Size(220, 25) };
            txtSearch.TextChanged += (s, e) => LoadGrid();

            var lblFilter = new Label { Text = "Category:", Location = new Point(320, 20), AutoSize = true };
            cmbFilterCategory = new ComboBox { Location = new Point(390, 17), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterCategory.SelectedIndexChanged += (s, e) => LoadGrid();

            var lblCount = new Label { Text = "", Location = new Point(600, 20), AutoSize = true, ForeColor = Color.Gray };
            lblCount.Name = "lblCount";

            grid = new DataGridView
            {
                Location = new Point(20, 55),
                Size = new Size(890, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.SelectionChanged += Grid_SelectionChanged;

            var lblName = new Label { Text = "Item Name", Location = new Point(20, 470), AutoSize = true, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            txtName = new TextBox { Location = new Point(20, 492), Size = new Size(280, 26), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            var lblCat = new Label { Text = "Category", Location = new Point(320, 470), AutoSize = true, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            cmbCategory = new ComboBox { Location = new Point(320, 492), Size = new Size(220, 26), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            var lblPrice = new Label { Text = "Price (Rs.)", Location = new Point(560, 470), AutoSize = true, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            txtPrice = new TextBox { Location = new Point(560, 492), Size = new Size(120, 26), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            var btnAdd = new Button { Text = "Add Item", Location = new Point(20, 540), Size = new Size(120, 35), BackColor = Color.FromArgb(25, 135, 84), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnAdd.Click += BtnAdd_Click;

            var btnUpdate = new Button { Text = "Update Selected", Location = new Point(150, 540), Size = new Size(140, 35), BackColor = Color.FromArgb(13, 110, 253), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnUpdate.Click += BtnUpdate_Click;

            var btnDelete = new Button { Text = "Delete Selected", Location = new Point(300, 540), Size = new Size(140, 35), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnDelete.Click += BtnDelete_Click;

            var btnClear = new Button { Text = "Clear Form", Location = new Point(450, 540), Size = new Size(110, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnClear.Click += (s, e) => ClearForm();

            this.Controls.AddRange(new Control[]
            {
                lblSearch, txtSearch, lblFilter, cmbFilterCategory, lblCount, grid,
                lblName, txtName, lblCat, cmbCategory, lblPrice, txtPrice,
                btnAdd, btnUpdate, btnDelete, btnClear
            });
        }

        private void LoadCategories()
        {
            var categories = DatabaseHelper.GetCategories();
            cmbFilterCategory.Items.Clear();
            cmbFilterCategory.Items.AddRange(categories.ToArray());
            cmbFilterCategory.SelectedIndex = 0;

            cmbCategory.Items.Clear();
            cmbCategory.Items.AddRange(categories.Where(c => c != "All").ToArray());
        }

        private void LoadGrid()
        {
            var items = DatabaseHelper.GetAllMenuItems();

            if (cmbFilterCategory.SelectedItem != null && cmbFilterCategory.SelectedItem.ToString() != "All")
                items = items.Where(i => i.Category == cmbFilterCategory.SelectedItem.ToString()).ToList();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                items = items.Where(i => i.Name.ToLower().Contains(txtSearch.Text.ToLower())).ToList();

            grid.DataSource = null;
            grid.DataSource = items.Select(i => new { i.Id, i.Name, i.Category, Price = i.Price.ToString("N0") }).ToList();

            var lblCount = this.Controls.Find("lblCount", true).FirstOrDefault() as Label;
            if (lblCount != null) lblCount.Text = $"{items.Count} item(s)";
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;
            var row = grid.SelectedRows[0];
            selectedId = Convert.ToInt32(row.Cells["Id"].Value);
            txtName.Text = row.Cells["Name"].Value.ToString();
            cmbCategory.Text = row.Cells["Category"].Value.ToString();
            txtPrice.Text = row.Cells["Price"].Value.ToString().Replace(",", "");
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(cmbCategory.Text) || !decimal.TryParse(txtPrice.Text, out _))
            {
                MessageBox.Show("Please enter a valid name, category, and numeric price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;
            DatabaseHelper.AddMenuItem(new MenuItem { Name = txtName.Text.Trim(), Category = cmbCategory.Text.Trim(), Price = decimal.Parse(txtPrice.Text) });
            LoadCategories();
            LoadGrid();
            ClearForm();
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedId == 0) { MessageBox.Show("Select an item from the grid first."); return; }
            if (!ValidateForm()) return;
            DatabaseHelper.UpdateMenuItem(new MenuItem { Id = selectedId, Name = txtName.Text.Trim(), Category = cmbCategory.Text.Trim(), Price = decimal.Parse(txtPrice.Text) });
            LoadCategories();
            LoadGrid();
            ClearForm();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedId == 0) { MessageBox.Show("Select an item from the grid first."); return; }
            if (MessageBox.Show("Delete this item?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DatabaseHelper.DeleteMenuItem(selectedId);
                LoadGrid();
                ClearForm();
            }
        }

        private void ClearForm()
        {
            selectedId = 0;
            txtName.Clear();
            cmbCategory.SelectedIndex = -1;
            txtPrice.Clear();
        }
    }
}
