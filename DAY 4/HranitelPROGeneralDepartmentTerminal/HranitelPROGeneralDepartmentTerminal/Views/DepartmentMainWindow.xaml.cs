using HranitelPROGeneralDepartmentTerminal.Data;
using HranitelPROGeneralDepartmentTerminal.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class DepartmentMainWindow : Window
    {
        private readonly int _employeeId;
        private readonly int _departmentId;

        public DepartmentMainWindow(int employeeId, int departmentId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            _departmentId = departmentId;
            LoadApprovedRequests();
        }

        private void LoadApprovedRequests(DateTime? startDate = null, DateTime? endDate = null)
        {
            string sql = @"
                SELECT v.* FROM ViewListRequests v
                WHERE v.status_name = 'одобрена'
                  AND v.department_name = (SELECT name FROM departments WHERE id = @deptId)
                  AND (@startDate IS NULL OR v.start_date >= @startDate)
                  AND (@endDate IS NULL OR v.end_date <= @endDate)
                ORDER BY v.start_date DESC";

            var parameters = new[]
            {
                new NpgsqlParameter("@deptId", _departmentId),
                new NpgsqlParameter("@startDate", (object)startDate ?? DBNull.Value),
                new NpgsqlParameter("@endDate", (object)endDate ?? DBNull.Value)
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
                    VisitorsList = row["visitors_list"].ToString()
                });
            }
            RequestsDataGrid.ItemsSource = list;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadApprovedRequests(StartDatePicker.SelectedDate, EndDatePicker.SelectedDate);
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            LoadApprovedRequests();
        }

        private void RequestsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (RequestsDataGrid.SelectedItem is RequestViewItem selected)
            {
                var window = new DepartmentVisitWindow(selected.RequestId, _employeeId, _departmentId);
                window.ShowDialog();
                ApplyFilter_Click(null, null);
            }
        }
    }
}