using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Web;

/// <summary>
/// Classes implementing this interface will automatically be hosted in the web service for API calls.
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Singleton)]
public interface IWebResource
{
}
