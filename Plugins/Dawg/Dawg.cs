using Common.Addins;
using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AutoDiscoverAssembly]

namespace Dawg;

public record class Dawg(
    string Author = "Jacob Albano",
    string Description = "Convert corpus text into an optimized data structure which can be searched with fuzzy matching at very high speed"
) : IPlugin<Dawg>;