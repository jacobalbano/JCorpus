using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AozoraBunko;

internal class Constants
{
    public static readonly IReadOnlyList<string> Tags = new[] { AozoraRuby, AozoraHTML };

    public const string AozoraRuby = nameof(AozoraRuby);
    public const string AozoraHTML = nameof(AozoraHTML);
}
