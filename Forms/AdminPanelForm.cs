using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantPOS.Data;

namespace RestaurantPOS.Forms
{
    /// <summary>
    /// A protected gate in front of the sales-management screen. Requires the
    /// dedicated admin account (separate from regular cashier/manager logins).
    /// </summary>
    public class AdminPanelForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Label lblError;
        private Button btnLogin;

        public AdminPanelForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Admin Panel - Restricted Access";
            this.Size = new Size(420, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            var lblLock = new Label
            {
                Text = "🔒",
                Font = new Font("Segoe UI Emoji", 28),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 25),
                Size = new Size(360, 45)
            };

            var lblTitle = new Label
            {
                Text = "Admin Panel",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 72),
                Size = new Size(360, 30)
            };

            var lblSubtitle = new Label
            {
                Text = "Sales data can only be cleared from here.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 100),
                Size = new Size(360, 20)
            };

            var lblUser = new Label { Text = "Admin Username", Location = new Point(50, 140), Size = new Size(200, 20), Font = new Font("Segoe UI", 10) };
            txtUsername = new TextBox { Location = new Point(50, 162), Size = new Size(300, 28), Font = new Font("Segoe UI", 11) };

            var lblPass = new Label { Text = "Admin Password", Location = new Point(50, 202), Size = new Size(200, 20), Font = new Font("Segoe UI", 10) };
            txtPassword = new TextBox { Location = new Point(50, 224), Size = new Size(300, 28), Font = new Font("Segoe UI", 11), UseSystemPasswordChar = true };

            btnLogin = new Button
            {
                Text = "Unlock Admin Panel",
                Location = new Point(50, 274),
                Size = new Size(300, 38),
                BackColor = Color.FromArgb(33, 37, 41),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            lblError = new Label
            {
                Text = "",
                ForeColor = Color.Red,
                Location = new Point(50, 320),
                Size = new Size(300, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9)
            };

            this.Controls.AddRange(new Control[] { lblLock, lblTitle, lblSubtitle, lblUser, txtUsername, lblPass, txtPassword, btnLogin, lblError });
            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblError.Text = "Please enter admin username and password.";
                return;
            }

            // Must match the dedicated admin account AND carry the SuperAdmin role -
            // a normal manager/cashier login is not enough to reach this screen.
            if (DatabaseHelper.ValidateUser(username, password, out string role) && role == "SuperAdmin")
            {
                lblError.Text = "";
                this.Hide();
                using var salesForm = new SalesManagementForm();
                salesForm.ShowDialog();
                this.Close();
            }
            else
            {
                lblError.Text = "Access denied. Admin credentials required.";
                txtPassword.Clear();
            }
        }
    }
}
