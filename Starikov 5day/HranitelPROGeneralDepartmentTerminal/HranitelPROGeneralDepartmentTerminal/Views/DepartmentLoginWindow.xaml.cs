using HranitelPROGeneralDepartmentTerminal.Data;
using Npgsql;
using System;
using System.Data;
using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class DepartmentLoginWindow : Window
    {
        public DepartmentLoginWindow()
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

            try
            {
                string sql = @"
                    SELECT d.id, d.name, de.full_name
                    FROM department_employees de
                    JOIN departments d ON de.department_id = d.id
                    WHERE de.id = @id AND d.id NOT IN (6, 7);";

                var param = new NpgsqlParameter("@id", employeeId);
                var dt = DatabaseHelper.ExecuteQuery(sql, new[] { param });

                if (dt.Rows.Count > 0)
                {
                    int deptId = Convert.ToInt32(dt.Rows[0]["id"]);
                    string deptName = dt.Rows[0]["name"].ToString();
                    string employeeName = dt.Rows[0]["full_name"].ToString();

                    MessageBox.Show($"Добро пожаловать, {employeeName}\nПодразделение: {deptName}",
                                    "Авторизация успешна", MessageBoxButton.OK, MessageBoxImage.Information);

                    var mainWindow = new DepartmentMainWindow(employeeId, deptId);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ErrorTextBlock.Text = "Неверный код сотрудника или сотрудник не принадлежит подразделению.";
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Ошибка: {ex.Message}";
            }
        }
    }
}