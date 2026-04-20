using HranitelPROGeneralDepartmentTerminal.Data;
using HranitelPROGeneralDepartmentTerminal.Models;
using HranitelPROGeneralDepartmentTerminal.Services;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class RequestReviewWindow : Window
    {
        private readonly int _requestId;
        private bool _isBlacklisted = false;
        private List<Visitor> _visitors;
        private string _originalStatus;
        private int _statusIdApproved, _statusIdRejected;

        public RequestReviewWindow(int requestId)
        {
            InitializeComponent();
            _requestId = requestId;
            LoadStatuses();
            LoadRequestData();
            CheckBlacklist();
        }

        private void LoadStatuses()
        {
            string sql = "SELECT id, name FROM statuses WHERE name IN ('Одобрена', 'Отклонена') ORDER BY name";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            StatusComboBox.ItemsSource = dt.DefaultView;
            foreach (DataRow row in dt.Rows)
            {
                if (row["name"].ToString() == "Одобрена") _statusIdApproved = Convert.ToInt32(row["id"]);
                else if (row["name"].ToString() == "Отклонена") _statusIdRejected = Convert.ToInt32(row["id"]);
            }
        }

        private void LoadRequestData()
        {
            string sql = @"
                SELECT r.id, r.type, r.start_date, r.end_date, r.purpose, r.created_at,
                       d.name AS dept_name, de.full_name AS emp_name, s.name AS status_name, u.email AS user_email
                FROM requests r
                JOIN departments d ON r.department_id = d.id
                JOIN department_employees de ON r.employee_id = de.id
                JOIN statuses s ON r.status_id = s.id
                JOIN users u ON r.user_id = u.id
                WHERE r.id = @reqId";
            var param = new NpgsqlParameter("@reqId", _requestId);
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new[] { param });
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("Заявка не найдена.");
                this.Close();
                return;
            }
            DataRow row = dt.Rows[0];
            RequestIdText.Text = row["id"].ToString();
            TypeText.Text = row["type"].ToString();
            PurposeText.Text = row["purpose"].ToString();
            DepartmentText.Text = row["dept_name"].ToString();
            EmployeeText.Text = row["emp_name"].ToString();
            DatesText.Text = $"{Convert.ToDateTime(row["start_date"]).ToShortDateString()} - {Convert.ToDateTime(row["end_date"]).ToShortDateString()}";
            StatusText.Text = row["status_name"].ToString();
            UserEmailText.Text = row["user_email"].ToString();
            _originalStatus = row["status_name"].ToString();

            string visitorsSql = @"
                SELECT v.id, v.last_name, v.first_name, v.middle_name, v.email, v.phone, 
                       v.organization, v.note, v.birth_date, v.passport_series, v.passport_number
                FROM visitors v
                JOIN request_visitors rv ON v.id = rv.visitor_id
                WHERE rv.request_id = @reqId
                ORDER BY rv.order_number";
            DataTable visitorsDt = DatabaseHelper.ExecuteQuery(visitorsSql, new[] { param });
            _visitors = new List<Visitor>();
            foreach (DataRow vrow in visitorsDt.Rows)
            {
                _visitors.Add(new Visitor
                {
                    Id = Convert.ToInt32(vrow["id"]),
                    LastName = vrow["last_name"].ToString(),
                    FirstName = vrow["first_name"].ToString(),
                    MiddleName = vrow["middle_name"].ToString(),
                    Email = vrow["email"].ToString(),
                    Phone = vrow["phone"].ToString(),
                    Organization = vrow["organization"].ToString(),
                    Note = vrow["note"].ToString(),
                    BirthDate = Convert.ToDateTime(vrow["birth_date"]),
                    PassportSeries = vrow["passport_series"].ToString(),
                    PassportNumber = vrow["passport_number"].ToString()
                });
            }
            VisitorsDataGrid.ItemsSource = _visitors;
        }

        private void CheckBlacklist()
        {
            string sql = @"
                SELECT EXISTS(
                    SELECT 1 FROM request_visitors rv
                    JOIN blacklist b ON rv.visitor_id = b.visitor_id
                    WHERE rv.request_id = @reqId
                )";
            var param = new NpgsqlParameter("@reqId", _requestId);
            _isBlacklisted = Convert.ToBoolean(DatabaseHelper.ExecuteScalar(sql, new[] { param }));

            if (_isBlacklisted)
            {
                BlacklistMessageText.Text = "ВНИМАНИЕ! Один или несколько посетителей находятся в чёрном списке. Заявка автоматически отклонена.";
                BlacklistMessageText.Visibility = Visibility.Visible;
                UpdateRequestStatus(_statusIdRejected);
                SendNotificationToVisitors("Заявка на посещение объекта КИИ отклонена в связи с нарушением Федерального закона от 26.07.2017 № 187-ФЗ «О безопасности критической информационной инфраструктуры Российской Федерации»");
                ActionPanel.IsEnabled = false;
                ApproveButton.IsEnabled = false;
                RejectButton.IsEnabled = false;
                StatusComboBox.IsEnabled = false;
                VisitDatePicker.IsEnabled = false;
                VisitTimeTextBox.IsEnabled = false;
            }
            else
            {
                BlacklistMessageText.Visibility = Visibility.Collapsed;
                if (_originalStatus != "проверка")
                {
                    MessageBox.Show("Заявка уже обработана. Редактирование невозможно.");
                    ActionPanel.IsEnabled = false;
                    ApproveButton.IsEnabled = false;
                    RejectButton.IsEnabled = false;
                }
                else
                {
                    ActionPanel.IsEnabled = true;
                    StatusComboBox.SelectedValue = _statusIdApproved;
                }
            }
        }

        private void UpdateRequestStatus(int newStatusId)
        {
            string sql = "UPDATE requests SET status_id = @statusId WHERE id = @reqId";
            var parameters = new[]
            {
                new NpgsqlParameter("@statusId", newStatusId),
                new NpgsqlParameter("@reqId", _requestId)
            };
            DatabaseHelper.ExecuteNonQuery(sql, parameters);
            StatusText.Text = GetStatusNameById(newStatusId);
            _originalStatus = StatusText.Text;
        }

        private string GetStatusNameById(int statusId)
        {
            string sql = "SELECT name FROM statuses WHERE id = @id";
            return DatabaseHelper.ExecuteScalar(sql, new[] { new NpgsqlParameter("@id", statusId) }).ToString();
        }

        private void SendNotificationToVisitors(string message)
        {
            var emails = _visitors.Select(v => v.Email).Distinct().ToList();
            EmailService.SendEmails(emails, "Статус заявки на посещение", message);
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isBlacklisted) return;

            if (VisitDatePicker.SelectedDate == null || string.IsNullOrWhiteSpace(VisitTimeTextBox.Text))
            {
                MessageBox.Show("Укажите дату и время посещения.");
                return;
            }

            UpdateRequestStatus(_statusIdApproved);
            string message = $"Заявка на посещение объекта КИИ одобрена, дата посещения: {VisitDatePicker.SelectedDate.Value:dd.MM.yyyy}, время посещения: {VisitTimeTextBox.Text}";
            SendNotificationToVisitors(message);
            MessageBox.Show("Заявка одобрена.");
            this.Close();
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isBlacklisted) return;

            UpdateRequestStatus(_statusIdRejected);
            string message = "Заявка на посещение объекта КИИ отклонена в связи с нарушением Федерального закона от 26.07.2017 № 194-ФЗ «О внесении изменений в Уголовный кодекс Российской Федерации и статью 151 Уголовно-процессуального кодекса Российской Федерации в связи с принятием Федерального закона \"О безопасности критической информационной инфраструктуры Российской Федерации\" по причине указания заявителем заведомо недостоверных данных»";
            SendNotificationToVisitors(message);
            MessageBox.Show("Заявка отклонена.");
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}