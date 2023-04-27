using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres
{
    internal class ListableRecordsHelper
    {
        internal static PageableData<Common> ToPageableData<Common, Record>(IRecordConverter<Common> converter, IEnumerable<Record> items, int index, int pageSize, long virtualItemCount)
        {
            IList<Common> list = new List<Common>();
            foreach (Record r in items)
                list.Add(converter.ToCommon(null));

            return new PageableData<Common>(list, index, pageSize, virtualItemCount);
        }

        internal static IEnumerable<Common> ToEnumerable<Common, Record>(IRecordConverter<Common> converter, IEnumerable<Record> items)
        {
            IList<Common> list = new List<Common>();
            foreach (Record r in items)
                list.Add(converter.ToCommon(null));

            return list;
        }

    }
}
