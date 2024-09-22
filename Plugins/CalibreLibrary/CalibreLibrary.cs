using Common.Addins;
using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AutoDiscoverAssembly]

namespace CalibreLibrary;

public record class CalibreLibrary(
    string Author = "Jacob Albano",
    string Description = "Access ebooks from your Calibre Ebook library"
) : IPlugin<CalibreLibrary>;
