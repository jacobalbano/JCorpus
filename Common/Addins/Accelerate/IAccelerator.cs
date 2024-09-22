using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Content;
using Common.IO;

namespace Common.Addins.Accelerate;

/// <summary>
/// Describes an object which can accelerate or optimize a <see cref="CorpusWork"/>.
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Scoped | AutoDiscoverOptions.Implementations)]
public interface IAccelerator : IAddin
{
    /// <summary>
    /// Return the object which should be saved to the corpus content directory.<br />
    /// If the directory already contains an object with the same name, the operation will terminate.
    /// </summary>
    /// <returns></returns>
    ICorpusContent ContentObject { get; }

    /// <summary>
    /// Perform the acceleration operation on this <see cref="CorpusWork"/>.<br />
    /// This operation should modify the same object that is returned by <see cref="ContentObject"/><br />
    /// Once the method completes, the content object will be saved to the corpus directory.
    /// </summary>
    /// <param name="work">The work to accelerate</param>
    /// <param name="content">The content to be accelerated</param>
    void Accelerate(CorpusWork work, IEnumerable<CorpusEntry> content);
}
