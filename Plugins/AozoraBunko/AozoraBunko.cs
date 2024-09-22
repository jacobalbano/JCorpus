using Common.Addins;
using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AutoDiscoverAssembly]

namespace AozoraBunko;

public record class AozoraBunko(
    string Author = "Jacob Albano",
    string Description = "Download and extract text from 青空文庫, the definitive source for freely-available Japanese literature"
) : IPlugin<AozoraBunko>;