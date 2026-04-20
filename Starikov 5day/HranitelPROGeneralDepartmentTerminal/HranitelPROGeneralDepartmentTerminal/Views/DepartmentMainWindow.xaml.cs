using HranitelPROGeneralDepartmentTerminal.Data;
using HranitelPROGeneralDepartmentTerminal.Models;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class DepartmentMainWindow : Window
    {
        private readonly int _employeeId;
        private readonly int _departmentId;
        private string _employeeName;
        private string _departmentName;

        public DepartmentMainWindow(int employeeId, int departmentId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            _departmentId = departmentId;

            try
            {
                LoadEmployeeInfo();
                UpdateHeader();
                LoadApprovedRequests();
                UpdateStatus($"Загружены заявки для подразделения: {_departmentName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации окна: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает информацию о сотруднике и подразделении
        /// </summary>
        private void LoadEmployeeInfo()
        {
            string sql = @"
                SELECT de.full_name AS employee_name, d.name AS department_name
                FROM department_employees de
                JOIN departments d ON de.department_id = d.id
                WHERE de.id = @empId AND d.id = @deptId";

            var parameters = new[]
            {
                new NpgsqlParameter("@empId", NpgsqlDbType.Integer) { Value = _employeeId },
                new NpgsqlParameter("@deptId", NpgsqlDbType.Integer) { Value = _departmentId }
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            if (dt.Rows.Count > 0)
            {
                _employeeName = dt.Rows[0]["employee_name"].ToString();
                _departmentName = dt.Rows[0]["department_name"].ToString();
            }
            else
            {
                _employeeName = "Неизвестный сотрудник";
                _departmentName = "Неизвестное подразделение";
            }
        }

        /// <summary>
        /// Обновляет заголовок окна и информационные блоки
        /// </summary>
        private void UpdateHeader()
        {
            Title = $"Терминал сотрудника подразделения - {_departmentName}";
            HeaderTextBlock.Text = $"Сотрудник: {_employeeName}";
            SubHeaderTextBlock.Text = $"Подразделение: {_departmentName} | Код сотрудника: {_employeeId}";
        }

        /// <summary>
        /// Загружает одобренные заявки для подразделения с учётом фильтров по датам
        /// </summary>
        private void LoadApprovedRequests(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                UpdateStatus("Загрузка заявок...");

                string sql = @"
                    SELECT v.request_id, v.type, v.start_date, v.end_date, v.purpose,
                           v.department_name, v.employee_full_name, v.status_name,
                           v.user_email, v.visitors_list, v.created_at
                    FROM ViewListRequests v
                    WHERE v.status_name = 'одобрена'
                      AND v.department_name = (SELECT name FROM departments WHERE id = @deptId)
                      AND (@startDate IS NULL OR v.start_date >= @startDate)
                      AND (@endDate IS NULL OR v.end_date <= @endDate)
                    ORDER BY v.start_date DESC, v.created_at DESC";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@deptId", NpgsqlDbType.Integer) { Value = _departmentId },
                    new NpgsqlParameter("@startDate", NpgsqlDbType.Date)
                    {
                        Value = startDate.HasValue ? (object)startDate.Value : DBNull.Value
                    },
                    new NpgsqlParameter("@endDate", NpgsqlDbType.Date)
                    {
                        Value = endDate.HasValue ? (object)endDate.Value : DBNull.Value
                    }
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
                List<RequestViewItem> list = new List<RequestViewItem>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new RequestViewItem
                    {
                        RequestId = Convert.ToInt32(row["request_id"]),
                        Type = row["type"].ToString(),
                        StartDate = Convert.ToDateTime(row["start_date"]),
                        EndDate = Convert.ToDateTime(row["end_date"]),
                        Purpose = row["purpose"].ToString(),
                        DepartmentName = row["department_name"].ToString(),
                        EmployeeFullName = row["employee_full_name"].ToString(),
                        StatusName = row["status_name"].ToString(),
                        UserEmail = row["user_email"].ToString(),
                        VisitorsList = row["visitors_list"]?.ToString() ?? "Нет данных",
                        CreatedAt = row["created_at"] != DBNull.Value ? Convert.ToDateTime(row["created_at"]) : DateTime.MinValue
                    });
                }

                RequestsDataGrid.ItemsSource = list;
                CountTextBlock.Text = $"Заявок: {list.Count}";

                if (list.Count == 0)
                {
                    UpdateStatus("Нет одобренных заявок для данного подразделения за указанный период.");
                }
                else
                {
                    UpdateStatus($"Загружено {list.Count} заявок.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заявок: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"Ошибка загрузки: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновляет текст в статусной строке
        /// </summary>
        private void UpdateStatus(string message)
        {
            StatusTextBlock.Text = message;
        }

        /// <summary>
        /// Применяет фильтр по датам
        /// </summary>
        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? startDate = StartDatePicker.SelectedDate;
                DateTime? endDate = EndDatePicker.SelectedDate;

                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    MessageBox.Show("Дата начала не может быть больше даты окончания.",
                                    "Некорректный диапазон", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                LoadApprovedRequests(startDate, endDate);
                UpdateStatus($"Применён фильтр: с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтра: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Сбрасывает фильтр по датам
        /// </summary>
        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartDatePicker.SelectedDate = null;
                EndDatePicker.SelectedDate = null;
                LoadApprovedRequests();
                UpdateStatus("Фильтр сброшен. Показаны все заявки.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтра: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обновляет список заявок
        /// </summary>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadApprovedRequests(StartDatePicker.SelectedDate, EndDatePicker.SelectedDate);
                UpdateStatus("Список заявок обновлён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Открывает окно управления посещением при двойном клике на заявку
        /// </summary>
        private void RequestsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (RequestsDataGrid.SelectedItem is RequestViewItem selected)
                {
                    UpdateStatus($"Открытие заявки №{selected.RequestId}...");
                    var window = new DepartmentVisitWindow(selected.RequestId, _employeeId, _departmentId);
                    window.Owner = this;
                    window.ShowDialog();

                    LoadApprovedRequests(StartDatePicker.SelectedDate, EndDatePicker.SelectedDate);
                    UpdateStatus($"Заявка №{selected.RequestId} обработана. Список обновлён.");
                }
                else
                {
                    MessageBox.Show("Выберите заявку для просмотра.",
                                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии заявки: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"Ошибка: {ex.Message}");
            }
        }
    }
}