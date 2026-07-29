using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Helpers
{
    public static class EnumerableExtension
    {
        public static IEnumerable? GetBatchByNumber(this IEnumerable collection, int batchSize, int batchNumber)
        {
            if (collection == null)
                return null;

            var firstItemIndex = batchSize * (batchNumber);
            var list = collection.Cast<object>().ToList();
            if (list.Count() < firstItemIndex)
                return null;

            if (list.Count() < firstItemIndex + batchSize)
                batchSize = list.Count() - firstItemIndex;

            return list.GetRange(firstItemIndex, batchSize);
        }
    }
}
