using DBT_API.Dtos;
using DBT_API.Entities;

namespace DBT_API
{
    public static class Extensions
    {
        public static ValidationDto AsDto(this Validation validation)
        {
            return new ValidationDto(validation.Id, validation.Name, validation.Description, validation.ValidationString);
        }

        public static BlobDto AsDto(this Blob blob)
        {
            return new BlobDto(blob.Etag, blob.Bucket, blob.FileName, blob.NodeIris);
        }

        public static TimeseriesDto AsDto(this Timeseries timeseries)
        {
            return new TimeseriesDto(timeseries.NodeIRI, timeseries.Time, timeseries.Value, timeseries.Tags);
        }
    }
}