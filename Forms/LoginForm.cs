using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantPOS.Data;

namespace RestaurantPOS.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblError;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Restaurant by Muzammil - Login";
            this.Size = new Size(420, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var lblTitle = new Label
            {
                Text = "Restaurant by Muzammil",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 30),
                Size = new Size(360, 40)
            };

            var lblSubtitle = new Label
            {
                Text = "Sign in to continue",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 72),
                Size = new Size(360, 20)
            };

            var lblUser = new Label { Text = "Username", Location = new Point(50, 120), Size = new Size(150, 20), Font = new Font("Segoe UI", 10) };
            txtUsername = new TextBox { Location = new Point(50, 142), Size = new Size(300, 28), Font = new Font("Segoe UI", 11) };

            var lblPass = new Label { Text = "Password", Location = new Point(50, 182), Size = new Size(150, 20), Font = new Font("Segoe UI", 10) };
            txtPassword = new TextBox { Location = new Point(50, 204), Size = new Size(300, 28), Font = new Font("Segoe UI", 11), UseSystemPasswordChar = true };

            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(50, 254),
                Size = new Size(300, 38),
                BackColor = Color.FromArgb(220, 53, 69),
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
                Location = new Point(50, 300),
                Size = new Size(300, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9)
            };

            var lblHint = new Label
            {
                Text = "Default login: admin / admin123",
                ForeColor = Color.Gray,
                Location = new Point(50, 325),
                Size = new Size(300, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8, FontStyle.Italic)
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblUser, txtUsername, lblPass, txtPassword, btnLogin, lblError, lblHint });
            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblError.Text = "Please enter username and password.";
                return;
            }

            if (DatabaseHelper.ValidateUser(username, password, out string role))
            {
                lblError.Text = "";
                this.Hide();
                var dashboard = new DashboardForm(username, role);
                dashboard.FormClosed += (s, args) => this.Close();
                dashboard.Show();
            }
            else
            {
                lblError.Text = "Invalid username or password.";
            }
        }
    }
}
