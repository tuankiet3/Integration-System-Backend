using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Integration_System.Model
{
    public class AttendanceModel
    {
        [Required]
        private int attendanceId;
        public int AttendanceId { get { return attendanceId; } set { attendanceId = value; } }
        ///////////////////////////
        private int employeeId;
        public int EmployeeId { get { return employeeId; } set { employeeId = value; } }
        ///////////////////////////
        [Required]
        private int workDays;
        public int WorkDays { get { return workDays; } set { workDays = value; } }
        ///////////////////////////
        private int absentDays;
        public int AbsentDays { get { return absentDays; } set { absentDays = value; } }
        ///////////////////////////
        private int leaveDays;
        public int LeaveDays { get { return leaveDays; } set { leaveDays = value; } }
        ///////////////////////////
        [Required]
        private int attendanceMonth;
        public int AttendanceMonth { get { return attendanceMonth; } set { attendanceMonth = value; } }
        ///////////////////////////
        //private int createrAt;
        //public int CreaterAt { get { return createrAt; } set { createrAt = value; } }

    }
}
