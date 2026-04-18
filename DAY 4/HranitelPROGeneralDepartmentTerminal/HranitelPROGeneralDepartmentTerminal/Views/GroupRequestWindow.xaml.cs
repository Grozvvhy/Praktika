using HranitelPROGeneralDepartmentTerminal.Data;
using HranitelPROGeneralDepartmentTerminal.Models;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class GroupRequestWindow : Window
    {
        private readonly int _currentUserId;
        public ObservableCollection<Visitor> Visitors { get; set; } = new ObservableCollection<Visitor>();

        public GroupRequestWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            LoadDepartments();
            VisitorsDataGrid.ItemsSource = Visitors;
        }

        private void LoadDepartments()
        {
            string sql = "SELECT id, name FROM departments ORDER BY name";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            DepartmentComboBox.ItemsSource = dt.DefaultView;
            DepartmentComboBox.SelectedValuePath = "id";
            DepartmentComboBox.DisplayMemberPath = "name";
        }

        private void DepartmentComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DepartmentComboBox.SelectedValue != null && int.TryParse(DepartmentComboBox.SelectedValue.ToString(), out int deptId))
            {
                LoadEmployees(deptId);
            }
        }

        private void LoadEmployees(int departmentId)
        {
            string sql = "SELECT id, full_name FROM department_employees WHERE department_id = @deptId ORDER BY full_name";
            var param = new NpgsqlParameter("@deptId", departmentId);
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new[] { param });
            EmployeeComboBox.ItemsSource = dt.DefaultView;
            EmployeeComboBox.SelectedValuePath = "id";
            EmployeeComboBox.DisplayMemberPath = "full_name";
        }

        private void AddVisitorButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VisitorEditDialog();
            if (dialog.ShowDialog() == true)
            {
                Visitors.Add(dialog.Visitor);
            }
        }

        private void RemoveVisitor_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is Visitor visitor)
            {
                Visitors.Remove(visitor);
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(PurposeTextBox.Text))
            {
                MessageBox.Show("Укажите цель посещения.");
                return;
            }
            if (DepartmentComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите подразделение.");
                return;
            }
            if (EmployeeComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите ответственного сотрудника.");
                return;
            }
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Укажите даты начала и окончания.");
                return;
            }
            if (Visitors.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одного посетителя.");
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string insertRequestSql = @"
                                INSERT INTO requests (user_id, type, start_date, end_date, purpose, department_id, employee_id, status_id)
                                VALUES (@userId, 'group', @startDate, @endDate, @purpose, @deptId, @empId, 
                                        (SELECT id FROM statuses WHERE name = 'проверка'))
                                RETURNING id;";
                            using (var cmd = new NpgsqlCommand(insertRequestSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@userId", _currentUserId);
                                cmd.Parameters.AddWithValue("@startDate", StartDatePicker.SelectedDate.Value);
                                cmd.Parameters.AddWithValue("@endDate", EndDatePicker.SelectedDate.Value);
                                cmd.Parameters.AddWithValue("@purpose", PurposeTextBox.Text);
                                cmd.Parameters.AddWithValue("@deptId", int.Parse(DepartmentComboBox.SelectedValue.ToString()));
                                cmd.Parameters.AddWithValue("@empId", int.Parse(EmployeeComboBox.SelectedValue.ToString()));
                                int requestId = Convert.ToInt32(cmd.ExecuteScalar());

                                int order = 1;
                                foreach (var visitor in Visitors)
                                {
                                    int visitorId = EnsureVisitorExists(visitor, conn, transaction);
                                    string linkSql = @"
                                        INSERT INTO request_visitors (request_id, visitor_id, order_number)
                                        VALUES (@reqId, @visId, @order)";
                                    using (var linkCmd = new NpgsqlCommand(linkSql, conn, transaction))
                                    {
                                        linkCmd.Parameters.AddWithValue("@reqId", requestId);
                                        linkCmd.Parameters.AddWithValue("@visId", visitorId);
                                        linkCmd.Parameters.AddWithValue("@order", order++);
                                        linkCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                            transaction.Commit();
                            MessageBox.Show("Групповая заявка успешно создана.");
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Ошибка при создании заявки: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private int EnsureVisitorExists(Visitor visitor, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string checkSql = @"
                SELECT id FROM visitors 
                WHERE email = @email OR (passport_series = @series AND passport_number = @number)
                LIMIT 1;";
            using (var cmd = new NpgsqlCommand(checkSql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@email", visitor.Email);
                cmd.Parameters.AddWithValue("@series", visitor.PassportSeries);
                cmd.Parameters.AddWithValue("@number", visitor.PassportNumber);
                var existingId = cmd.ExecuteScalar();
                if (existingId != null)
                    return Convert.ToInt32(existingId);
            }

            string insertSql = @"
                INSERT INTO visitors (last_name, first_name, middle_name, phone, email, organization, note, birth_date, passport_series, passport_number, passport_scan_path)
                VALUES (@ln, @fn, @mn, @phone, @email, @org, @note, @bdate, @series, @number, @scan)
                RETURNING id;";
            using (var cmd = new NpgsqlCommand(insertSql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@ln", visitor.LastName);
                cmd.Parameters.AddWithValue("@fn", visitor.FirstName);
                cmd.Parameters.AddWithValue("@mn", (object)visitor.MiddleName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@phone", (object)visitor.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@email", visitor.Email);
                cmd.Parameters.AddWithValue("@org", (object)visitor.Organization ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@note", (object)visitor.Note ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@bdate", visitor.BirthDate);
                cmd.Parameters.AddWithValue("@series", visitor.PassportSeries);
                cmd.Parameters.AddWithValue("@number", visitor.PassportNumber);
                cmd.Parameters.AddWithValue("@scan", (object)visitor.PassportScanPath ?? DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}