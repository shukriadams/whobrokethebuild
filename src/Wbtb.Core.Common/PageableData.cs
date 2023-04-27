using System;
using System.Collections.Generic;
using System.Linq;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Collection of objects with properties for supporting of paging through a collection of objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class PageableData<T>
    {
        #region PROPERTIES

        /// <summary>
        /// Page of objects on current page
        /// </summary>
        public IList<T> Items { get; set; }

        /// <summary>
        /// Total number of objects in source collection that can be paged through
        /// </summary>
        public long TotalItemCount { get; set; }

        /// <summary>
        /// Maximum number of objects on page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Position of current page in source collection.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Total number of pages available.
        /// </summary>
        public long TotalPages { get; private set; }

        #endregion

        #region CTORS

        public PageableData(IEnumerable<T> items, int pageIndex, int pageSize, long virtualItemCount)
        {
            this.Items = items.ToList();
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.TotalItemCount = virtualItemCount;

            this.TotalPages = this.TotalItemCount / this.PageSize;
            if (this.TotalItemCount % this.PageSize != 0)
                this.TotalPages++;
        }

        #endregion
    }
}
