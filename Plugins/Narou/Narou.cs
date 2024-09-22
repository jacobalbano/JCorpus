using Common.Addins;
using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AutoDiscoverAssembly]

namespace Narou;

public record class Narou(
    string Author = "Jacob Albano",
    string Description = "Download and extract works from 小説家になろう (https://ncode.syosetu.com/)"
) : IPlugin<Narou>;
