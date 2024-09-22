using Common.Addins;
using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AutoDiscoverAssembly]

namespace Mokuro;

public record class Mokuro(
    string Author = "Jacob Albano",
    string Description = "Extract text from manga using Mokuro OCR"
) : IPlugin<Mokuro>;
