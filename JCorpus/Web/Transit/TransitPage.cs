using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Web.Transit;

record class TransitPage<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageCount,
    int PageNumber,
    int PageSize
)
{
    public static TransitPage<T> OfSize(IReadOnlyList<T> source, int pageSize, int page)
    {
        page = Math.Max(1, page);
        return new(
            source.Skip(pageSize * (page - 1)).Take(pageSize),
            source.Count,
            source.Count / pageSize + 1,
            page,
            pageSize
        );
    }
}