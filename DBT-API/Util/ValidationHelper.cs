using VDS.RDF;
using VDS.RDF.Shacl.Validation;
using VDS.RDF.Shacl;


namespace DBT_API.Util
{
    public static class ValidationHelper
    {
        public static Tuple<bool, string> CheckForCompliance(IGraph dataGraph, List<Graph> validationGraphs)
        {
            bool conforming = true;
            string errorMessage = "";

            // perform valiation
            foreach (Graph gr in validationGraphs)
            {
                conforming = new ShapesGraph(gr).Conforms(dataGraph);
                if (!conforming)
                {
                    var result = new ShapesGraph(gr).Validate(dataGraph).Results;
                    foreach (Result res in result)
                    {
                        errorMessage += "Validation error at " + res.FocusNode.ToString() + " : " + res.Message.Value.ToString() + " \n";
                    }
                    break;
                }
            }

            return new Tuple<bool, string>(conforming, errorMessage);
        }
    }
}
