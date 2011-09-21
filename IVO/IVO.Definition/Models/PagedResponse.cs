using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace IVO.Definition.Models
{
    /// <summary>
    /// Represents an ordered, paged response to a query.
    /// </summary>
    /// <typeparam name="Telement">The type of elements in the collection</typeparam>
    /// <typeparam name="TorderBy">The enum type that defines what columns may be ordered on</typeparam>
    public sealed class PagedResponse<Telement, TorderBy>
        where Telement : class
        where TorderBy : struct
    {
        public PagedResponse(ReadOnlyCollection<Telement> collection, ReadOnlyCollection<OrderByApplication<TorderBy>> orderedBy, PagingRequest paging, int totalCount)
        {
            // Why would you pass nulls? What the hell is wrong with you?
            if (collection == null) throw new ArgumentNullException("collection");
            if (orderedBy == null) throw new ArgumentNullException("orderedBy");

            // We got a collection of records BIGGER than the page size? WtF?
            if (collection.Count > paging.PageSize) throw new ArgumentOutOfRangeException("collection", "Page collection is too big for page size");

            // Set our known properties:
            this.Collection = collection;
            this.OrderedBy = orderedBy;
            this.Paging = paging;
            this.TotalCount = totalCount;

            // Calculate the remaining properties:
            this.PageCount = totalCount / paging.PageSize + ( (totalCount % paging.PageSize) > 0 ? 1 : 0 );
            this.IsFirstPage = paging.PageNumber == 1;
            this.IsLastPage = paging.PageNumber == this.PageCount;
        }

        public ReadOnlyCollection<Telement> Collection { get; private set; }
        public ReadOnlyCollection<OrderByApplication<TorderBy>> OrderedBy { get; private set; }

        public PagingRequest Paging { get; private set; }
        public int TotalCount { get; private set; }
        public int PageCount { get; private set; }
        public bool IsFirstPage { get; private set; }
        public bool IsLastPage { get; private set; }
    }
}
