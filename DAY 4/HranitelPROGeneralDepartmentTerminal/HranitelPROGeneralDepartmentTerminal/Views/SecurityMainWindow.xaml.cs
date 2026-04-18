using HranitelPROGeneralDepartmentTerminal.Data;
using HranitelPROGeneralDepartmentTerminal.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class SecurityMainWindow : Window
    {
        private readonly int _employeeId;

        public SecurityMainWindow(int employeeId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            LoadFilterData();
            LoadApprovedRequests();
        }

        private void LoadFilterData()
        {
            string deptSql = "SELECT id, name FROM departments ORDER BY name";
            DataTable deptDt = DatabaseHelper.ExecuteQuery(deptSql);
            DataRow allRow = deptDt.NewRow();
            allRow["id"] = DBNull.Value;
            allRow["name"] = "Все";
            deptDt.Rows.InsertAt(allRow, 0);
            DepartmentFilterComboBox.ItemsSource = deptDt.DefaultView;
        }

        private void LoadApprovedRequests(string searchText = null, DateTime? date = null, string type = null, int? deptId = null)
        {
            string sql = @"
                SELECT v.* FROM ViewListRequests v
                WHERE v.status_name = 'одобрена'
                  AND (@date IS NULL OR v.start_date <= @date AND v.end_date >= @date)
                  AND (@type IS NULL OR v.type = @type)
                  AND (@deptId IS NULL OR v.department_name = (SELECT name FROM departments WHERE id = @deptId))
                  AND (@search IS NULL OR v.visitors_list ILIKE '%' || @search || '%')
                ORDER BY v.start_date DESC";

            var parameters = new[]
            {
                new NpgsqlParameter("@date", (object)date ?? DBNull.Value),
                new NpgsqlParameter("@type", (object)type ?? DBNull.Value),
                new NpgsqlParameter("@deptId", (object)deptId ?? DBNull.Value),
                new NpgsqlParameter("@search", (object)searchText ?? DBNull.Value)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
            List<RequestViewItem> list = new List<RequestViewItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRequestViewItem(row));
            }
            RequestsDataGrid.ItemsSource = list;
        }

        private RequestViewItem MapRequestViewItem(DataRow row)
        {
            return new RequestViewItem
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
            };
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            DateTime? date = FilterDatePicker.SelectedDate;
            string type = ((ComboBoxItem)TypeFilterComboBox.SelectedItem).Content.ToString();
            if (type == "Все") type = null;
            int? deptId = DepartmentFilterComboBox.SelectedValue as int?;
            LoadApprovedRequests(SearchTextBox.Text, date, type, deptId);
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterDatePicker.SelectedDate = null;
            TypeFilterComboBox.SelectedIndex = 0;
            DepartmentFilterComboBox.SelectedIndex = 0;
            SearchTextBox.Text = "";
            LoadApprovedRequests();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter_Click(sender, e);
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            ApplyFilter_Click(sender, e);
        }

        private void RequestsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (RequestsDataGrid.SelectedItem is RequestViewItem selected)
            {
                var window = new SecurityVisitWindow(selected.RequestId, _employeeId);
                window.ShowDialog();
                ApplyFilter_Click(null, null);
            }
        }
    }
}