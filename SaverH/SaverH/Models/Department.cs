namespace SaverH.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DepartmentEmployee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int DepartmentId { get; set; }
    }
}