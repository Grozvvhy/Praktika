using System;
using System.Drawing;
using System.Windows.Forms;
using SaverH.Models;

namespace SaverH.Forms
{
    public partial class VisitorForm : Form
    {
        public Visitor Visitor { get; private set; }

        public VisitorForm()
        {
            InitializeComponent();
            Visitor = new Visitor();
            this.Text = "Добавление посетителя";
            this.Size = new System.Drawing.Size(500, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            SetupForm();
        }

        private void SetupForm()
        {
            System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel() { Dock = DockStyle.Fill, AutoScroll = true };
            this.Controls.Add(panel);

            int yPos = 20;
            int labelWidth = 120;
            int controlWidth = 300;
            int leftMargin = 30;

            System.Windows.Forms.TextBox txtLastName = AddField(panel, "Фамилия:*", ref yPos, labelWidth, controlWidth, leftMargin);
            System.Windows.Forms.TextBox txtFirstName = AddField(panel, "Имя:*", ref yPos, labelWidth, controlWidth, leftMargin);
            System.Windows.Forms.TextBox txtMiddleName = AddField(panel, "Отчество:", ref yPos, labelWidth, controlWidth, leftMargin);
            System.Windows.Forms.TextBox txtPhone = AddField(panel, "Телефон:", ref yPos, labelWidth, controlWidth, leftMargin);
            System.Windows.Forms.TextBox txtEmail = AddField(panel, "Email:*", ref yPos, labelWidth, controlWidth, leftMargin);
            System.Windows.Forms.TextBox txtOrganization = AddField(panel, "Организация:", ref yPos, labelWidth, controlWidth, leftMargin);
            System.Windows.Forms.TextBox txtNote = AddField(panel, "Примечание:", ref yPos, labelWidth, controlWidth, leftMargin);

            System.Windows.Forms.Label lblBirthDate = new System.Windows.Forms.Label() { Text = "Дата рождения:*", Location = new System.Drawing.Point(leftMargin, yPos), Size = new System.Drawing.Size(labelWidth, 25) };
            DateTimePicker dtpBirthDate = new DateTimePicker()
            {
                Location = new System.Drawing.Point(leftMargin + labelWidth, yPos),
                Size = new System.Drawing.Size(controlWidth, 25),
                MaxDate = DateTime.Today.AddYears(-16),
                Format = DateTimePickerFormat.Short
            };
            panel.Controls.AddRange(new System.Windows.Forms.Control[] { lblBirthDate, dtpBirthDate });
            yPos += 35;

            System.Windows.Forms.TextBox txtPassportSeries = AddField(panel, "Серия паспорта:*", ref yPos, labelWidth, controlWidth, leftMargin);
            txtPassportSeries.MaxLength = 4;

            System.Windows.Forms.TextBox txtPassportNumber = AddField(panel, "Номер паспорта:*", ref yPos, labelWidth, controlWidth, leftMargin);
            txtPassportNumber.MaxLength = 6;

            System.Windows.Forms.Label lblScanPath = new System.Windows.Forms.Label() { Text = "Скан паспорта:", Location = new System.Drawing.Point(leftMargin, yPos), Size = new System.Drawing.Size(labelWidth, 25) };
            System.Windows.Forms.TextBox txtScanPath = new System.Windows.Forms.TextBox() { Location = new System.Drawing.Point(leftMargin + labelWidth, yPos), Size = new System.Drawing.Size(controlWidth - 80, 25), ReadOnly = true };
            System.Windows.Forms.Button btnBrowse = new System.Windows.Forms.Button() { Text = "Обзор...", Location = new System.Drawing.Point(leftMargin + labelWidth + controlWidth - 70, yPos), Size = new System.Drawing.Size(70, 25) };
            panel.Controls.AddRange(new System.Windows.Forms.Control[] { lblScanPath, txtScanPath, btnBrowse });
            yPos += 35;

            btnBrowse.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "PDF files|*.pdf|Image files|*.jpg;*.png|All files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtScanPath.Text = ofd.FileName;
                }
            };

            System.Windows.Forms.Button btnOk = new System.Windows.Forms.Button() { Text = "OK", Location = new System.Drawing.Point(leftMargin + 100, yPos + 20), Size = new System.Drawing.Size(100, 35), BackColor = System.Drawing.Color.LightGreen };
            System.Windows.Forms.Button btnCancel = new System.Windows.Forms.Button() { Text = "Отмена", Location = new System.Drawing.Point(leftMargin + 220, yPos + 20), Size = new System.Drawing.Size(100, 35), BackColor = System.Drawing.Color.LightCoral };
            panel.Controls.AddRange(new System.Windows.Forms.Control[] { btnOk, btnCancel });

            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtLastName.Text) || string.IsNullOrEmpty(txtFirstName.Text) ||
                    string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPassportSeries.Text) ||
                    string.IsNullOrEmpty(txtPassportNumber.Text))
                {
                    MessageBox.Show("Заполните все обязательные поля (отмечены *)!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!txtEmail.Text.Contains("@"))
                {
                    MessageBox.Show("Введите корректный email!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dtpBirthDate.Value > DateTime.Today.AddYears(-16))
                {
                    MessageBox.Show("Возраст посетителя должен быть не менее 16 лет!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Visitor.LastName = txtLastName.Text;
                Visitor.FirstName = txtFirstName.Text;
                Visitor.MiddleName = txtMiddleName.Text;
                Visitor.Phone = txtPhone.Text;
                Visitor.Email = txtEmail.Text;
                Visitor.Organization = txtOrganization.Text;
                Visitor.Note = txtNote.Text;
                Visitor.BirthDate = dtpBirthDate.Value;
                Visitor.PassportSeries = txtPassportSeries.Text;
                Visitor.PassportNumber = txtPassportNumber.Text;
                Visitor.PassportScanPath = txtScanPath.Text;

                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
        }

        private System.Windows.Forms.TextBox AddField(System.Windows.Forms.Panel panel, string labelText, ref int yPos, int labelWidth, int controlWidth, int leftMargin)
        {
            System.Windows.Forms.Label label = new System.Windows.Forms.Label() { Text = labelText, Location = new System.Drawing.Point(leftMargin, yPos), Size = new System.Drawing.Size(labelWidth, 25) };
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox() { Location = new System.Drawing.Point(leftMargin + labelWidth, yPos), Size = new System.Drawing.Size(controlWidth, 25) };
            panel.Controls.AddRange(new System.Windows.Forms.Control[] { label, textBox });
            yPos += 35;
            return textBox;
        }
    }
}