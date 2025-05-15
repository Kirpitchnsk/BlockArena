using System;
using System.Threading.Tasks;

namespace BlockArena.Domain
{
    public class IndexedTaskProvider<T>
    {
        public int Index { get; set; }
        public Func<Task<T>> TaskProvider { get; set; }
    }
}