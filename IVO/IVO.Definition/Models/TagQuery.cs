using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace IVO.Definition.Models
{
    /// <summary>
    /// Represents the search criteria to find tags.
    /// </summary>
    public sealed class TagQuery
    {
        public TagQuery(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, string name, string tagger)
        {
            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
            this.Name = name;
            this.Tagger = tagger;
        }

        // Date range searching:
        public DateTimeOffset? DateFrom { get; private set; }
        public DateTimeOffset? DateTo { get; private set; }

        // Filter by name
        public string Name { get; private set; }

        // Filter by tagger
        public string Tagger { get; private set; }
    }
}
