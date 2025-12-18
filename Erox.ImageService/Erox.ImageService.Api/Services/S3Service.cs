using Amazon.S3.Model;
using Amazon.S3;

namespace Erox.ImageService.Api.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IConfiguration config)
        {
            var region = config["AWS:Region"];
            _bucketName = config["AWS:BucketName"] ?? throw new ArgumentNullException("BucketName");

            _s3Client = new AmazonS3Client(
                config["AWS:AccessKey"],
                config["AWS:SecretKey"],
                Amazon.RegionEndpoint.GetBySystemName(region));
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = file.ContentType
            };


            await _s3Client.PutObjectAsync(request);

            return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{fileName}";
        }

       
        public async Task DeleteFileAsync(string key)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
    }
}
