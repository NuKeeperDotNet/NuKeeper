using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuKeeper.Types
{
    public static class LinqAsync
    {
        /// <summary>
        /// Filter a list by an async operation on each element
        /// Code from: https://codereview.stackexchange.com/a/32162
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> items, Func<T, Task<bool>> predicate)
        {
            var itemTaskList = items
                .Select(item => new { Item = item, PredTask = predicate.Invoke(item) })
                .ToList();

            await Task.WhenAll(itemTaskList.Select(x => x.PredTask));

            return itemTaskList
                .Where(x => x.PredTask.Result)
                .Select(x => x.Item);
        }
    }
}
