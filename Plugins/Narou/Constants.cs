using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.IO;

namespace Narou;

internal static class Constants
{
    public static readonly IReadOnlyList<string> Tags = new[] { NarouHtml };

    public const string NarouHtml = nameof(NarouHtml);
}
