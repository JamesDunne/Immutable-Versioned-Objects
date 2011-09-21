using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    /// <summary>
    /// Represents a single application of ordering applied to a result set.
    /// </summary>
    /// <typeparam name="TorderBy"></typeparam>
    public sealed class OrderByApplication<TorderBy>
        where TorderBy : struct
    {
        public OrderByApplication(TorderBy orderBy, OrderByDirection direction)
        {
            this.OrderBy = orderBy;
            this.Direction = direction;
        }

        public TorderBy OrderBy { get; private set; }
        public OrderByDirection Direction { get; private set; }
    }
}
