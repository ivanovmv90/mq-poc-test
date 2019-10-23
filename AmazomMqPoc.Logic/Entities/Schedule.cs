using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazomMqPoc.Logic.Entities
{
    public class Schedule
    {
        public string ScheduleCronString { get; set; }
        public ScheduleStatus Status { get; set; }
    }

    public enum ScheduleStatus
    {
        Active = 1,
        Inactive
    }
}
