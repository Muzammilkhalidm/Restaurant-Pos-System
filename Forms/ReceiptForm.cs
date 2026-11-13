using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using RestaurantPOS.Models;

namespace RestaurantPOS.Forms
{
    public class ReceiptForm : Form
    {
        private readonly int orderId;
        private readonly string customerName;
        private readonly string cashier;
        private readonly List<OrderItem> items;
        private readonly decimal subtotal, tax, total;
        private RichTextBox rtbReceipt;
        private PrintDocument printDoc;

        public ReceiptForm(int orderId, string customerName, string cashier, List<OrderItem> items,
            decimal subtotal, decimal tax, decimal total)
        {
            this.orderId = orderId;
            this.customerName = customerName;
            this.cashier = cashier;
            this.items = items;
            this.subtotal = subtotal;
            this.tax = tax;
            this.total = total;

            InitializeComponent();
            BuildReceiptText();
        }

        private void InitializeComponent()
        {
            this.Text = $"Receipt - Order #{orderId}";
            this.Size = new Size(420, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            rtbReceipt = new RichTextBox
            {
                Location = new Point(15, 15),
                Size = new Size(380, 500),
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var btnPrint = new Button { Text = "Print", Location = new Point(15, 530), Size = new Size(120, 36), BackColor = Color.FromArgb(13, 110, 253), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnPrint.Click += (s, e) => PrintReceipt();

            var btnSave = new Button { Text = "Save as Text", Location = new Point(145, 530), Size = new Size(120, 36) };
            btnSave.Click += (s, e) => SaveReceipt();

            var btnClose = new Button { Text = "Close", Location = new Point(275, 530), Size = new Size(120, 36) };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { rtbReceipt, btnPrint, btnSave, btnClose });

            printDoc = new PrintDocument();
            printDoc.PrintPage += PrintDoc_PrintPage;
        }

        private void BuildReceiptText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("     RESTAURANT BY MUZAMMIL");
            sb.AppendLine("      123 Main Boulevard, Lahore");
            sb.AppendLine("        Tel: 042-111-222-333");
            sb.AppendLine(new string('-', 38));
            sb.AppendLine($"Order #  : {orderId}");
            sb.AppendLine($"Date     : {DateTime.Now:dd-MMM-yyyy hh:mm tt}");
            sb.AppendLine($"Customer : {customerName}");
            sb.AppendLine($"Served by: {cashier}");
            sb.AppendLine(new string('-', 38));
            sb.AppendLine(string.Format("{0,-18}{1,4}{2,7}{3,9}", "Item", "Qty", "Rate", "Total"));
            sb.AppendLine(new string('-', 38));

            foreach (var item in items)
            {
                string name = item.Name.Length > 16 ? item.Name.Substring(0, 16) : item.Name;
                sb.AppendLine(string.Format("{0,-18}{1,4}{2,7}{3,9}",
                    name, item.Quantity, item.Price.ToString("N0"), item.Total.ToString("N0")));
            }

            sb.AppendLine(new string('-', 38));
            sb.AppendLine(string.Format("{0,-26}{1,12}", "Subtotal:", $"Rs. {subtotal:N0}"));
            sb.AppendLine(string.Format("{0,-26}{1,12}", "Tax (5%):", $"Rs. {tax:N0}"));
            sb.AppendLine(new string('-', 38));
            sb.AppendLine(string.Format("{0,-26}{1,12}", "TOTAL:", $"Rs. {total:N0}"));
            sb.AppendLine(new string('-', 38));
            sb.AppendLine();
            sb.AppendLine("      Thank you for dining with us!");
            sb.AppendLine("          Please visit again");

            rtbReceipt.Text = sb.ToString();
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString(rtbReceipt.Text, new Font("Consolas", 10), Brushes.Black, 10, 10);
        }

        private void PrintReceipt()
        {
            try
            {
                using var dialog = new PrintDialog { Document = printDoc };
                if (dialog.ShowDialog() == DialogResult.OK)
                    printDoc.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing failed: {ex.Message}\n\nMake sure a printer is installed.", "Print Error");
            }
        }

        private void SaveReceipt()
        {
            using var sfd = new SaveFileDialog { Filter = "Text File|*.txt", FileName = $"Receipt_{orderId}.txt" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(sfd.FileName, rtbReceipt.Text);
                MessageBox.Show("Receipt saved successfully.");
            }
        }
    }
}
