using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    public struct PagingRequest
    {
        private int _pageNumber;
        private int _pageSize;

        public PagingRequest(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException("pageNumber", "Page number must be at least 1");
            if (pageSize < 1) throw new ArgumentOutOfRangeException("pageSize", "Page size must be at least 1");

            _pageNumber = pageNumber;
            _pageSize = pageSize;
        }

        public int PageNumber { get { return _pageNumber; } }
        public int PageSize { get { return _pageSize; } }

        public int PageIndex { get { return _pageNumber - 1; } }
    }
}
