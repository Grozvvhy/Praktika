using HranitelPROGeneralDepartmentTerminal.Data;
using HranitelPROGeneralDepartmentTerminal.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Media;
using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class SecurityVisitWindow : Window
    {
        private readonly int _requestId;
        private readonly int _securityEmployeeId;
        private List<VisitLogItem> _visitLogs;

        public class VisitLogItem
        {
            public int Id { get; set; }
            public int VisitorId { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public DateTime? EntryTime { get; set; }
            public DateTime? ArrivalTime { get; set; }
            public DateTime? DepartureTime { get; set; }
            public DateTime? ExitTime { get; set; }
            public bool ViolationTime { get; set; }
        }

        public SecurityVisitWindow(int requestId, int securityEmployeeId)
        {
            InitializeComponent();
            _requestId = requestId;
            _securityEmployeeId = securityEmployeeId;
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
            // Получаем посетителей заявки и их логи посещения
            string sql = @"
                SELECT v.id AS visitor_id, v.last_name, v.first_name, v.middle_name,
                       vl.id AS log_id, vl.entry_time, vl.department_arrival_time, 
                       vl.department_departure_time, vl.exit_time, vl.violation_time
                FROM visitors v
                JOIN request_visitors rv ON v.id = rv.visitor_id
                LEFT JOIN visit_log vl ON vl.request_id = rv.request_id AND vl.visitor_id = v.id
                WHERE rv.request_id = @reqId
                ORDER BY rv.order_number";
            var param = new NpgsqlParameter("@reqId", _requestId);
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new[] { param });
            _visitLogs = new List<VisitLogItem>();
            foreach (DataRow row in dt.Rows)
            {
                _visitLogs.Add(new VisitLogItem
                {
                    Id = row["log_id"] != DBNull.Value ? Convert.ToInt32(row["log_id"]) : 0,
                    VisitorId = Convert.ToInt32(row["visitor_id"]),
                    LastName = row["last_name"].ToString(),
                    FirstName = row["first_name"].ToString(),
                    MiddleName = row["middle_name"].ToString(),
                    EntryTime = row["entry_time"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["entry_time"]) : null,
                    ArrivalTime = row["department_arrival_time"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["department_arrival_time"]) : null,
                    DepartureTime = row["department_departure_time"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["department_departure_time"]) : null,
                    ExitTime = row["exit_time"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["exit_time"]) : null,
                    ViolationTime = row["violation_time"] != DBNull.Value && Convert.ToBoolean(row["violation_time"])
                });
            }
            VisitorsDataGrid.ItemsSource = _visitLogs;
        }

        private void UpdateButtonStates()
        {
            bool allEntered = _visitLogs.TrueForAll(v => v.EntryTime.HasValue);
            bool allExited = _visitLogs.TrueForAll(v => v.ExitTime.HasValue);
            bool allDeparted = _visitLogs.TrueForAll(v => v.DepartureTime.HasValue);

            AllowEntryButton.IsEnabled = !allEntered;
            AllowExitButton.IsEnabled = allEntered && allDeparted && !allExited;
        }

        private void AllowEntryButton_Click(object sender, RoutedEventArgs e)
        {
            // Системный звук
            SystemSounds.Beep.Play();

            // Отправка сообщения на сервер об открытии турникета (эмуляция)
            StatusTextBlock.Text = $"Турникет открыт. Время: {DateTime.Now:HH:mm:ss}";

            // Фиксируем время входа для всех посетителей, у которых его ещё нет
            DateTime now = DateTime.Now;
            foreach (var log in _visitLogs)
            {
                if (!log.EntryTime.HasValue)
                {
                    InsertOrUpdateVisitLog(log.VisitorId, entryTime: now);
                    log.EntryTime = now;
                }
            }
            VisitorsDataGrid.ItemsSource = null;
            VisitorsDataGrid.ItemsSource = _visitLogs;
            UpdateButtonStates();

            // Проверка времени перемещения будет в терминале подразделения
        }

        private void AllowExitButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now;
            foreach (var log in _visitLogs)
            {
                if (!log.ExitTime.HasValue)
                {
                    InsertOrUpdateVisitLog(log.VisitorId, exitTime: now);
                    log.ExitTime = now;
                }
            }
            VisitorsDataGrid.ItemsSource = null;
            VisitorsDataGrid.ItemsSource = _visitLogs;
            UpdateButtonStates();
            StatusTextBlock.Text = $"Выход разрешён. Время: {now:HH:mm:ss}";
        }

        private void InsertOrUpdateVisitLog(int visitorId, DateTime? entryTime = null, DateTime? exitTime = null)
        {
            // Проверяем, есть ли уже запись
            string checkSql = "SELECT id FROM visit_log WHERE request_id = @reqId AND visitor_id = @visId";
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@reqId", _requestId),
                new NpgsqlParameter("@visId", visitorId)
            };
            var existingId = DatabaseHelper.ExecuteScalar(checkSql, parameters.ToArray());

            if (existingId != null && existingId != DBNull.Value)
            {
                // Обновляем
                string updateSql = "UPDATE visit_log SET ";
                var updateParams = new List<NpgsqlParameter>();
                if (entryTime.HasValue)
                {
                    updateSql += "entry_time = @entry, ";
                    updateParams.Add(new NpgsqlParameter("@entry", entryTime.Value));
                }
                if (exitTime.HasValue)
                {
                    updateSql += "exit_time = @exit, ";
                    updateParams.Add(new NpgsqlParameter("@exit", exitTime.Value));
                }
                updateSql = updateSql.TrimEnd(',', ' ') + " WHERE id = @logId";
                updateParams.Add(new NpgsqlParameter("@logId", Convert.ToInt32(existingId)));
                DatabaseHelper.ExecuteNonQuery(updateSql, updateParams.ToArray());
            }
            else
            {
                // Вставляем новую
                string insertSql = @"
                    INSERT INTO visit_log (request_id, visitor_id, entry_time, exit_time)
                    VALUES (@reqId, @visId, @entry, @exit)";
                var insertParams = new[]
                {
                    new NpgsqlParameter("@reqId", _requestId),
                    new NpgsqlParameter("@visId", visitorId),
                    new NpgsqlParameter("@entry", (object)entryTime ?? DBNull.Value),
                    new NpgsqlParameter("@exit", (object)exitTime ?? DBNull.Value)
                };
                DatabaseHelper.ExecuteNonQuery(insertSql, insertParams);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}