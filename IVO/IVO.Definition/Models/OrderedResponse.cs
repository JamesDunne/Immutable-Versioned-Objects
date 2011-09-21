using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace IVO.Definition.Models
{
    /// <summary>
    /// Represents an ordered response to a query.
    /// </summary>
    /// <typeparam name="Telement">The type of elements in the collection</typeparam>
    /// <typeparam name="TorderBy">The enum type that defines what columns may be ordered on</typeparam>
    public sealed class OrderedResponse<Telement, TorderBy>
        where Telement : class
        where TorderBy : struct
    {
        public OrderedResponse(ReadOnlyCollection<Telement> collection, ReadOnlyCollection<OrderByApplication<TorderBy>> orderedBy)
        {
            // Why would you pass nulls? What the hell is wrong with you?
            if (collection == null) throw new ArgumentNullException("collection");
            if (orderedBy == null) throw new ArgumentNullException("orderedBy");

            this.Collection = collection;
            this.OrderedBy = orderedBy;
        }

        public ReadOnlyCollection<Telement> Collection { get; private set; }
        public ReadOnlyCollection<OrderByApplication<TorderBy>> OrderedBy { get; private set; }
    }
}
