using System.ComponentModel.DataAnnotations;
using DBT_API.Entities;

namespace DBT_API.Dtos
{
    public record ValidationDto(Guid Id, string Name, string Description, string ValidationString);
    public record CreateValidationDto([Required] string Name, string Description, [Required] string ValidationString);

    public record BlobDto(string Etag, string Bucket, string FileName, IEnumerable<string> NodeIris);
    public record CreateBlobDto([Required]string Bucket, IEnumerable<string>? NodeIris);

    public record TimeseriesDto(string NodeIRI, DateTime Time, double Value, List<Tuple<string, string>> Tags);
    public record CreateTimeseriesByBucketDto([Required] string database, [Required] string measurement, [Required] DateTime Time, [Required] double Value, List<Tuple<string, string>>? Tags);
    public record CreateTimeseriesByNodeDto([Required] string nodeIRI, [Required] DateTime Time, [Required] double Value, List<Tuple<string, string>>? Tags);
    public record GetTimeseriesDto([Required] string NodeIRI, [Required] DateTime StartTime, DateTime EndTime, List<Tuple<string, string>>? Tags);
    public record CreateTimeseriesConnectionDto([Required] string NodeIRI, [Required] string EndPoint, [Required] string Identifier);

    public record NodeDto(string IRI, string Domain, List<string> Classes, List<Edge> Relations);
    public record CreateNodeDto([Required] string IRI, [Required] List<string> Classes, [Required] List<Edge> Relations);
}
