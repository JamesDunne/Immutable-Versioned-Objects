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
    public sealed class FullQueryResponse<Tquery, Telement>
        where Tquery : class
        where Telement : class
    {
        public FullQueryResponse(Tquery query, ReadOnlyCollection<Telement> collection)
        {
            // Why would you pass nulls? What the hell is wrong with you?
            if (query == null) throw new ArgumentNullException("query");
            if (collection == null) throw new ArgumentNullException("collection");

            this.Query = query;
            this.Collection = collection;
        }

        public Tquery Query { get; private set; }
        public ReadOnlyCollection<Telement> Collection { get; private set; }
    }
}
