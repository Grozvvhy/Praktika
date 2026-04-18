using System;

namespace HranitelPROGeneralDepartmentTerminal.Models
{
    public class RequestViewItem
    {
        public int RequestId { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Purpose { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeFullName { get; set; }
        public string StatusName { get; set; }
        public string UserEmail { get; set; }
        public string VisitorsList { get; set; }
    }
}