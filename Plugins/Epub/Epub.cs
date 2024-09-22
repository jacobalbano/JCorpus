using Common.Addins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epub;

public record class Epub(
    string Author = "Jacob Albano",
    string Description = "Extract text from .epub format ebooks"
) : IPlugin<Epub>;