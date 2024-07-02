using DBT_API.Enums;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace DBT_API.Entities
{
    public class SparqlUpdater
    {
        public SparqlConnector dataReadConnector;
        public SparqlConnector infoReadConnector;
        public SparqlConnector knowReadConnector;
        public SparqlRemoteUpdateEndpoint dataWriteConnector;
        public SparqlRemoteUpdateEndpoint infoWriteConnector;
        public SparqlRemoteUpdateEndpoint knowWriteConnector;

        public SparqlUpdater(string domain)
        {
            this.dataReadConnector = new SparqlConnector(new Uri(domain + "_data"));
            this.infoReadConnector = new SparqlConnector(new Uri(domain + "_info"));
            this.knowReadConnector = new SparqlConnector(new Uri(domain + "_know"));
            this.dataWriteConnector = new SparqlRemoteUpdateEndpoint(new Uri(domain + "_data" + "/statements"));
            this.infoWriteConnector = new SparqlRemoteUpdateEndpoint(new Uri(domain + "_info" + "/statements"));
            this.knowWriteConnector = new SparqlRemoteUpdateEndpoint(new Uri(domain + "_know" + "/statements"));
        }

        public SparqlConnector ReturnReadConnector(KID kid)
        {
            if (kid == KID.Data)
                return dataReadConnector;
            else if (kid == KID.Information)
                return infoReadConnector;
            else if (kid == KID.Knowledge)
                return knowReadConnector;
            else
                return null;
        }

        public SparqlRemoteUpdateEndpoint ReturnWriteConnector(KID kid)
        {
            if (kid == KID.Data)
                return dataWriteConnector;
            else if (kid == KID.Information)
                return infoWriteConnector;
            else if (kid == KID.Knowledge)
                return knowWriteConnector;
            else
                return null;
        }
    }
}
