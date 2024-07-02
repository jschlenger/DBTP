using Minio.DataModel.Args;
using Minio;
using Minio.Exceptions;

namespace DBT_API.Util
{
    public static class MinioHelper
    {
        public static async Task CreateMinioBucket(MinioClient minioClient, string bucketName)
        {
            // Check if a bucket already exists
            var bktExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await minioClient.BucketExistsAsync(bktExistsArgs);

            if (!found)
            {
                // Create bucket
                var mkBktArgs = new MakeBucketArgs().WithBucket(bucketName);
                await minioClient.MakeBucketAsync(mkBktArgs);
                Console.WriteLine(bucketName + " created successfully.");
            }
            else
            {
                Console.WriteLine(bucketName + " already exists.");
            }
        }

        public static async Task UploadToMinioFormData(MinioClient minio, string bucketName, string objectName, string filePath)
        {
            var contentType = "multipart/form-data";
            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                bool found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(bucketName);
                    await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }
                // Upload a file to bucket.
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithFileName(filePath)
                    .WithContentType(contentType);
                await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                Console.WriteLine("Successfully uploaded " + objectName);
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }
        }

        public static async Task UploadToMinioOct(MinioClient minio, string bucketName, string objectName, string filePath)
        {
            var contentType = "application/octet-stream";
            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                bool found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(bucketName);
                    await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }
                // Upload a file to bucket via stream
                var bs = File.ReadAllBytes(filePath);
                using var filestream = new MemoryStream(bs);
                {
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(filestream)
                        .WithObjectSize(-1)
                        .WithContentType(contentType);
                    await minio.PutObjectAsync(putObjectArgs);
                    Console.WriteLine("Successfully uploaded " + objectName);
                }
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }
        }
    }
}