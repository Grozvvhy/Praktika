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
    public partial class GeneralDepartmentMainWindow : Window
    {
        private readonly int _currentEmployeeId;

        public GeneralDepartmentMainWindow(int employeeId)
        {
            InitializeComponent();
            _currentEmployeeId = employeeId;
            LoadFilterData();
            LoadRequests();
        }

        private void LoadFilterData()
        {
            string deptSql = "SELECT id, name FROM departments ORDER BY name";
            DataTable deptDt = DatabaseHelper.ExecuteQuery(deptSql);
            DataRow allDeptRow = deptDt.NewRow();
            allDeptRow["id"] = DBNull.Value;
            allDeptRow["name"] = "Все";
            deptDt.Rows.InsertAt(allDeptRow, 0);
            DepartmentFilterComboBox.ItemsSource = deptDt.DefaultView;

            string statusSql = "SELECT id, name FROM statuses ORDER BY name";
            DataTable statusDt = DatabaseHelper.ExecuteQuery(statusSql);
            DataRow allStatusRow = statusDt.NewRow();
            allStatusRow["id"] = DBNull.Value;
            allStatusRow["name"] = "Все";
            statusDt.Rows.InsertAt(allStatusRow, 0);
            StatusFilterComboBox.ItemsSource = statusDt.DefaultView;
        }

        private void LoadRequests(string type = null, int? departmentId = null, int? statusId = null)
        {
            try
            {
                string sql = "SELECT * FROM FilteringRequests(@type, @deptId, @statusId)";
                var parameters = new[]
                {
                    new NpgsqlParameter("@type", (object)type ?? DBNull.Value),
                    new NpgsqlParameter("@deptId", (object)departmentId ?? DBNull.Value),
                    new NpgsqlParameter("@statusId", (object)statusId ?? DBNull.Value)
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
                        CreatedAt = Convert.ToDateTime(row["created_at"]),
                        DepartmentName = row["department_name"].ToString(),
                        EmployeeFullName = row["employee_full_name"].ToString(),
                        StatusName = row["status_name"].ToString(),
                        UserEmail = row["user_email"].ToString(),
                        VisitorsList = row["visitors_list"].ToString()
                    });
                }
                RequestsDataGrid.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}");
            }
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            string type = null;
            if (TypeFilterComboBox.SelectedItem is ComboBoxItem item && item.Content.ToString() != "Все")
                type = item.Content.ToString();

            int? deptId = null;
            if (DepartmentFilterComboBox.SelectedValue != null && DepartmentFilterComboBox.SelectedValue != DBNull.Value)
                deptId = Convert.ToInt32(DepartmentFilterComboBox.SelectedValue);

            int? statusId = null;
            if (StatusFilterComboBox.SelectedValue != null && StatusFilterComboBox.SelectedValue != DBNull.Value)
                statusId = Convert.ToInt32(StatusFilterComboBox.SelectedValue);

            LoadRequests(type, deptId, statusId);
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            TypeFilterComboBox.SelectedIndex = 0;
            DepartmentFilterComboBox.SelectedIndex = 0;
            StatusFilterComboBox.SelectedIndex = 0;
            LoadRequests();
        }

        private void RequestsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (RequestsDataGrid.SelectedItem is RequestViewItem selected)
            {
                var reviewWindow = new RequestReviewWindow(selected.RequestId);
                reviewWindow.ShowDialog();
                ApplyFilter_Click(null, null);
            }
        }
    }
}