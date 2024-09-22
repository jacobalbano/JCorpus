using Common.Addins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frequency;

public record class Frequency(
    string Author = "Jacob Albano",
    string Description = "Get information about the frequency of characters and words throughout the corpus"
) : IPlugin<Frequency>;