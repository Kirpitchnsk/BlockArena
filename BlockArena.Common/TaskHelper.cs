using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockArena.Common
{
    public class TaskHelper
    {
        public static async Task WhenAll(IEnumerable<Func<Task>> tasks, int max)
        {
            var taskEnumerator = tasks.GetEnumerator();

            await Task.WhenAll(Enumerable
                .Range(1, max)
                .Select(i => RunInSeries(taskEnumerator)));
        }

        private static async Task RunInSeries(IEnumerator<Func<Task>> enumerator)
        {
            if (enumerator.MoveNext())
            {
                await enumerator.Current();
                await RunInSeries(enumerator);
            }
        }

        private static async Task<List<Result<T>>> RunInSeries<T>(IEnumerator<TaskProvider<T>> enumerator, List<Result<T>> accumalatedValues = null)
        {
            var accumalatedValuesOrEmpty = accumalatedValues ?? new List<Result<T>>();

            return enumerator.MoveNext()
                ? await RunInSeries(enumerator, accumalatedValuesOrEmpty
                    .Concat(new Result<T> { Index = enumerator.Current.Index, Data = await enumerator.Current.Provider() })
                    .ToList())
                : accumalatedValuesOrEmpty;
        }
    }
}