using HranitelPROGeneralDepartmentTerminal.Data;
using Npgsql;
using System;
using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class SecurityLoginWindow : Window
    {
        public SecurityLoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(EmployeeCodeTextBox.Text.Trim(), out int employeeId))
            {
                ErrorTextBlock.Text = "Код сотрудника должен быть числом.";
                return;
            }

            string sql = @"
                SELECT COUNT(*) 
                FROM department_employees de
                JOIN departments d ON de.department_id = d.id
                WHERE de.id = @id AND d.id = 7;";
            var param = new NpgsqlParameter("@id", employeeId);
            var result = DatabaseHelper.ExecuteScalar(sql, new[] { param });

            if (Convert.ToInt32(result) > 0)
            {
                var mainWindow = new SecurityMainWindow(employeeId);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ErrorTextBlock.Text = "Неверный код сотрудника или сотрудник не принадлежит отделу охраны.";
            }
        }
    }
}