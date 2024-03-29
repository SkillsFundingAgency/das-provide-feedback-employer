﻿using System;
using System.Collections.Generic;

namespace ESFA.DAS.EmployerProvideFeedback.Paging
{
    public abstract class PaginatedList
    {
        public abstract int PageIndex { get; }
        public abstract int PageSize { get; }
        public abstract int PageSetSize { get; set; }

        public abstract int TotalRecordCount { get; }
        public abstract int TotalPages { get; }

        public abstract bool HasPreviousPage { get; }
        public abstract bool HasNextPage { get; }

        public abstract int FirstVisiblePage { get; }
        public abstract int LastVisiblePage { get; }

        public abstract int FirstVisibleItem { get; }
        public abstract int LastVisibleItem { get; }
    }

    public class PaginatedList<T> : PaginatedList
    {
        public override int PageIndex { get; }
        public override int PageSize { get; }
        public override int PageSetSize { get; set; }

        public List<T> Items { get; } = new List<T>();

        public override int TotalRecordCount { get; }
        public override int TotalPages => (TotalRecordCount / PageSize) + ((TotalRecordCount % PageSize) > 0 ? 1 : 0);

        private PaginatedList(List<T> items, int totalRecordCount, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalRecordCount = totalRecordCount;

            if (items != null)
            {
                Items.AddRange(items);
            }
        }

        public PaginatedList(List<T> items, int totalRecordCount, int pageIndex, int pageSize, int pageSetSize)
            : this(items, totalRecordCount, pageIndex, pageSize)
        {
            PageSetSize = (pageSetSize == default(int)) ? 1 : pageSetSize;
        }

        public override bool HasPreviousPage => (PageIndex > 1);
        public override bool HasNextPage => (PageIndex < TotalPages);

        public override int FirstVisiblePage => Math.Max(Math.Min(PageIndex - (PageSetSize / 3), (TotalPages + 1) - PageSetSize), 1);
        public override int LastVisiblePage => Math.Min(Math.Max(PageIndex + (PageSetSize / 2), PageSetSize), TotalPages);

        public override int FirstVisibleItem => ((PageIndex - 1) * PageSize) + 1;
        public override int LastVisibleItem => Math.Min(PageIndex * PageSize, TotalRecordCount);

        public PaginatedList<T1> Convert<T1>() where T1 : class
        {
            return new PaginatedList<T1>(Items.ConvertAll(p => p as T1), TotalRecordCount, PageIndex, PageSize, PageSetSize);
        }
    }
}
