using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Management_Desktop_App;

namespace CustomComparator
{
    public class PriorityAndDeadlineComparator : IComparer<TaskItem>
    {
        private Dictionary<string, int> priorityMap = new Dictionary<string, int>
            {
                { "Low", 3 },
                { "Medium", 2 },
                { "High", 1 }
            };

        public int Compare(TaskItem? x, TaskItem? y)
        {
            if (x == null || y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            if (!priorityMap.ContainsKey(x.Priority) || !priorityMap.ContainsKey(y.Priority))
                return 0;

            if (priorityMap[x.Priority] == priorityMap[y.Priority])
            {
                return x.Deadline.CompareTo(y.Deadline);
            }

            return priorityMap[x.Priority].CompareTo(priorityMap[y.Priority]);
        }
    }
}
