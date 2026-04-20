using HranitelPROGeneralDepartmentTerminal.Data;
using HranitelPROGeneralDepartmentTerminal.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class DepartmentVisitWindow : Window
    {
        private readonly int _requestId;
        private readonly int _employeeId;
        private readonly int _departmentId;
        private List<DepartmentVisitLogItem> _visitLogs;

        public class DepartmentVisitLogItem
        {
            public int LogId { get; set; }
            public int VisitorId { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public DateTime? EntryTime { get; set; }
            public DateTime? ArrivalTime { get; set; }
            public DateTime? DepartureTime { get; set; }
            public bool ViolationTime { get; set; }
        }

        public DepartmentVisitWindow(int requestId, int employeeId, int departmentId)
        {
            InitializeComponent();
            _requestId = requestId;
            _employeeId = employeeId;
            _departmentId = departmentId;
            LoadRequestData();
            LoadVisitLogs();
            UpdateButtonStates();
        }

        private void LoadRequestData()
        {
            string sql = @"
                SELECT r.id, r.type, r.purpose, d.name AS dept_name, de.full_name AS emp_name
                FROM requests r
                JOIN departments d ON r.department_id = d.id
                JOIN department_employees de ON r.employee_id = de.id
                WHERE r.id = @reqId";
            var param = new NpgsqlParameter("@reqId", _requestId);
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new[] { param });
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                RequestIdText.Text = row["id"].ToString();
                TypeText.Text = row["type"].ToString();
                DepartmentText.Text = row["dept_name"].ToString();
                EmployeeText.Text = row["emp_name"].ToString();
                PurposeText.Text = row["purpose"].ToString();
            }
        }

        private void LoadVisitLogs()
        {
            string sql = @"
                SELECT v.id AS visitor_id, v.last_name, v.first_name, v.middle_name,
                       vl.id AS log_id, vl.entry_time, vl.department_arrival_time, 
                       vl.department_departure_time, vl.violation_time
                FROM visitors v
                JOIN request_visitors rv ON v.id = rv.visitor_id
                LEFT JOIN visit_log vl ON vl.request_id = rv.request_id AND vl.visitor_id = v.id
                WHERE rv.request_id = @reqId
                ORDER BY rv.order_number";
            var param = new NpgsqlParameter("@reqId", _requestId);
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new[] { param });
            _visitLogs = new List<DepartmentVisitLogItem>();
            foreach (DataRow row in dt.Rows)
            {
                _visitLogs.Add(new DepartmentVisitLogItem
                {
                    LogId = row["log_id"] != DBNull.Value ? Convert.ToInt32(row["log_id"]) : 0,
                    VisitorId = Convert.ToInt32(row["visitor_id"]),
                    LastName = row["last_name"].ToString(),
                    FirstName = row["first_name"].ToString(),
                    MiddleName = row["middle_name"].ToString(),
                    EntryTime = row["entry_time"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["entry_time"]) : null,
                    ArrivalTime = row["department_arrival_time"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["department_arrival_time"]) : null,
                    DepartureTime = row["department_departure_time"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["department_departure_time"]) : null,
                    ViolationTime = row["violation_time"] != DBNull.Value && Convert.ToBoolean(row["violation_time"])
                });
            }
            VisitorsDataGrid.ItemsSource = _visitLogs;
        }

        private void UpdateButtonStates()
        {
            bool allEntered = _visitLogs.All(v => v.EntryTime.HasValue);
            bool allArrived = _visitLogs.All(v => v.ArrivalTime.HasValue);
            bool allDeparted = _visitLogs.All(v => v.DepartureTime.HasValue);

            ArrivalButton.IsEnabled = allEntered && !allArrived;
            DepartureButton.IsEnabled = allArrived && !allDeparted;
        }

        private int GetTravelTime()
        {
            string sql = "SELECT minutes FROM travel_time WHERE department_id = @deptId";
            var param = new NpgsqlParameter("@deptId", _departmentId);
            var result = DatabaseHelper.ExecuteScalar(sql, new[] { param });
            return result != null ? Convert.ToInt32(result) : 10;
        }

        private void ArrivalButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now;
            int travelMinutes = GetTravelTime();

            foreach (var log in _visitLogs)
            {
                if (log.EntryTime.HasValue && !log.ArrivalTime.HasValue)
                {
                    var travelDuration = (now - log.EntryTime.Value).TotalMinutes;
                    bool violation = travelDuration > travelMinutes;

                    UpdateVisitLogArrival(log.VisitorId, now, violation);
                    log.ArrivalTime = now;
                    log.ViolationTime = violation;

                    if (violation)
                    {
                        StatusTextBlock.Text += $"Нарушение времени перемещения для {log.LastName} {log.FirstName}. ";
                    }
                }
            }
            VisitorsDataGrid.ItemsSource = null;
            VisitorsDataGrid.ItemsSource = _visitLogs;
            UpdateButtonStates();
            StatusTextBlock.Text += "Время прибытия зафиксировано.";
        }

        private void DepartureButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now;
            foreach (var log in _visitLogs)
            {
                if (log.ArrivalTime.HasValue && !log.DepartureTime.HasValue)
                {
                    UpdateVisitLogDeparture(log.VisitorId, now);
                    log.DepartureTime = now;
                }
            }
            VisitorsDataGrid.ItemsSource = null;
            VisitorsDataGrid.ItemsSource = _visitLogs;
            UpdateButtonStates();
            StatusTextBlock.Text += "Время убытия зафиксировано.";
        }

        private void UpdateVisitLogArrival(int visitorId, DateTime arrivalTime, bool violation)
        {
            string checkSql = "SELECT id FROM visit_log WHERE request_id = @reqId AND visitor_id = @visId";
            var parameters = new[]
            {
                new NpgsqlParameter("@reqId", _requestId),
                new NpgsqlParameter("@visId", visitorId)
            };
            var existingId = DatabaseHelper.ExecuteScalar(checkSql, parameters);

            if (existingId != null && existingId != DBNull.Value)
            {
                string updateSql = @"
                    UPDATE visit_log 
                    SET department_arrival_time = @arrival, violation_time = @violation 
                    WHERE id = @logId";
                var updateParams = new[]
                {
                    new NpgsqlParameter("@arrival", arrivalTime),
                    new NpgsqlParameter("@violation", violation),
                    new NpgsqlParameter("@logId", Convert.ToInt32(existingId))
                };
                DatabaseHelper.ExecuteNonQuery(updateSql, updateParams);
            }
        }

        private void UpdateVisitLogDeparture(int visitorId, DateTime departueTime)
        {
            string sql = @"
                UPDATE visit_log 
                SET department_departure_time = @departure 
                WHERE request_id = @reqId AND visitor_id = @visId";
            var parameters = new[]
            {
                new NpgsqlParameter("@departure", departueTime),
                new NpgsqlParameter("@reqId", _requestId),
                new NpgsqlParameter("@visId", visitorId)
            };
            DatabaseHelper.ExecuteNonQuery(sql, parameters);
        }

        private void AddToBlacklist_Click(object sender, RoutedEventArgs e)
        {
            if (VisitorsDataGrid.SelectedItem is DepartmentVisitLogItem selected)
            {
                var dialog = new BlacklistReasonDialog();
                if (dialog.ShowDialog() == true)
                {
                    string sql = "INSERT INTO blacklist (visitor_id, reason) VALUES (@visId, @reason)";
                    var parameters = new[]
                    {
                        new NpgsqlParameter("@visId", selected.VisitorId),
                        new NpgsqlParameter("@reason", dialog.Reason)
                    };
                    DatabaseHelper.ExecuteNonQuery(sql, parameters);
                    MessageBox.Show($"Посетитель {selected.LastName} {selected.FirstName} добавлен в чёрный список.");
                }
            }
            else
            {
                MessageBox.Show("Выберите посетителя в таблице.");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}