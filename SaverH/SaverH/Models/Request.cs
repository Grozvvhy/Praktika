using System;
using System.Collections.Generic;

namespace SaverH.Models
{
    public class Request
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Purpose { get; set; }
        public int DepartmentId { get; set; }
        public int EmployeeId { get; set; }
        public int StatusId { get; set; }
        public string RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StatusName { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeName { get; set; }
        public List<Visitor> Visitors { get; set; }
    }
}