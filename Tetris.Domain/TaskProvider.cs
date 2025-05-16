using System;
using System.Threading.Tasks;

namespace BlockArena.Common
{
    public class TaskProvider<T>
    {
        public int Index { get; set; }
        public Func<Task<T>> Provider { get; set; }
    }
}