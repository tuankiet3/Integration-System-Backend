namespace Integration_System.Model
{
    public class DepartmentModel
    {
        private int departmentId;
        public int DepartmentId { get { return departmentId; } set { departmentId = value; } }
        ///////////////////////////
        private string departmentName;
        public string DepartmentName { get { return departmentName; } set { departmentName = value; } }
        ///////////////////////////
        private int managerId;
        public int ManagerId { get { return managerId; } set { managerId = value; } }
    }
}
