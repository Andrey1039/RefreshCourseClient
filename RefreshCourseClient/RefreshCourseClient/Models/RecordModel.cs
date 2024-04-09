using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefreshCourseClient.Models
{
    internal class RecordModel
    {
        public int Id { get; set; }
        public string? GroupName { get; set; }
        public string? SubjectName { get; set; }
        public string? LessonType { get; set; }
        public int HoursCount { get; set; }

        public double PayHour { get; set; }
        public double Money { get; set; }
    }
}
