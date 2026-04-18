using HranitelPROGeneralDepartmentTerminal.Models;
using System;
using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class VisitorEditDialog : Window
    {
        public Visitor Visitor { get; private set; }

        public VisitorEditDialog()
        {
            InitializeComponent();
            Visitor = new Visitor();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Простая валидация
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                BirthDatePicker.SelectedDate == null ||
                string.IsNullOrWhiteSpace(PassportSeriesTextBox.Text) ||
                string.IsNullOrWhiteSpace(PassportNumberTextBox.Text))
            {
                MessageBox.Show("Заполните обязательные поля: фамилия, имя, email, дата рождения, серия и номер паспорта.");
                return;
            }

            Visitor.LastName = LastNameTextBox.Text.Trim();
            Visitor.FirstName = FirstNameTextBox.Text.Trim();
            Visitor.MiddleName = MiddleNameTextBox.Text.Trim();
            Visitor.Email = EmailTextBox.Text.Trim();
            Visitor.Phone = PhoneTextBox.Text.Trim();
            Visitor.Organization = OrganizationTextBox.Text.Trim();
            Visitor.BirthDate = BirthDatePicker.SelectedDate.Value;
            Visitor.PassportSeries = PassportSeriesTextBox.Text.Trim();
            Visitor.PassportNumber = PassportNumberTextBox.Text.Trim();
            Visitor.Note = NoteTextBox.Text.Trim();
            Visitor.PassportScanPath = "/scans/dummy.pdf"; // временно

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}