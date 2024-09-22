using Common.Content;
using Common.DI;
using Danbo.Utility;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using JCorpus.Persistence;
using JCorpus.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Web.Resources;

record class Thing(
    CorpusWorkId Id  
);

internal class Work : IWebResource
{
    [ResourceMethod]
    public IEnumerable<CorpusWorkId> Get()
    {
        return db.Select<DbCorpusWork>()
            .ToList()
            .Select(x => x.CorpusWorkId);
    }

    [ResourceMethod(RequestMethod.GET, ":id")]
    public Thing Get(string id)
    {
        return new(db.Select<DbCorpusWork>()
            .Where(x => x.CorpusWorkId == id)
            .FirstOrDefault()?.CorpusWorkId ?? throw new ProviderException(ResponseStatus.NotFound, "Not found"));
    }

    [ResourceMethod(RequestMethod.DELETE, ":id")]
    public CorpusWorkId Delete(string id)
    {
        var toDelete = db.Select<DbCorpusWork>()
            .Where(x => x.CorpusWorkId == id)
            .FirstOrDefault() ?? throw new ProviderException(ResponseStatus.NotFound, "Not found");

        using var s = db.BeginSession();
        s.Delete(toDelete);
        return toDelete.CorpusWorkId;
        // TODO: delete the directory
    }

    public Work(Database db)
    {
        this.db = db;
    }

    private readonly Database db;
}
