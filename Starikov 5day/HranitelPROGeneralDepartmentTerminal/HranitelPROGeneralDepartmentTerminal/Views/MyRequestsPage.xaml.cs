using HranitelPROGeneralDepartmentTerminal.Data;
using HranitelPROGeneralDepartmentTerminal.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;

namespace HranitelPROGeneralDepartmentTerminal.Views
{
    public partial class MyRequestsPage : Page
    {
        private readonly int _userId;

        public MyRequestsPage(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadRequests();
        }

        private void LoadRequests()
        {
            string sql = @"
                SELECT * FROM ViewListRequests 
                WHERE user_email = (SELECT email FROM users WHERE id = @userId)
                ORDER BY created_at DESC";
            var param = new NpgsqlParameter("@userId", _userId);
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new[] { param });
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
                    VisitorsList = row["visitors_list"].ToString()
                });
            }
            RequestsDataGrid.ItemsSource = list;
        }
    }
}