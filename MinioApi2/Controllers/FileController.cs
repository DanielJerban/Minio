using Microsoft.AspNetCore.Mvc;
using Minio;
using System;

namespace MinioApi2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    [HttpPost]
    public async Task<string> UploadStream(IFormFile file)
    {
        string bucketName = "test1";
        var endpoint = "127.0.0.1:9000";
        var accessKey = "nLP2GNlVILLJQ1hWEPcl";
        var secretKey = "t4MyX6IoGmHZ35Kh6GnJt8i5x4jc6QDuT2ZDCUFT";


        var minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            // .WithSSL()
            .Build();

        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            var bs = ms.ToArray();

            using var fileStream = new MemoryStream(bs);
            var poa = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(file.FileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType("application/octet-stream");

            // upload file
            await minioClient.PutObjectAsync(poa);
        }

        var statObjectArgs = new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(file.FileName);

        // confirm upload
        var objectStat = await minioClient.StatObjectAsync(statObjectArgs);

        return
            $"{objectStat.ObjectName}: {objectStat.ETag} {objectStat.ETag} {objectStat.VersionId} {objectStat.ContentType} {objectStat.Size}";
    }

    [HttpGet]
    public async Task<string> GetFileAsBase64(string fileName)
    {
        string bucketName = "test1";
        var endpoint = "127.0.0.1:9000";
        var accessKey = "nLP2GNlVILLJQ1hWEPcl";
        var secretKey = "t4MyX6IoGmHZ35Kh6GnJt8i5x4jc6QDuT2ZDCUFT";


        var minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            // .WithSSL()
            .Build();

        // Confirm object exists before attempting to get. 
        var statObjectArgs = new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName);
        try
        {
            await minioClient.StatObjectAsync(statObjectArgs);
        }
        catch
        {
            throw new FileNotFoundException("File does not exists");
        }

        var ms = new MemoryStream();

        var buffer = new byte[16*1024];
        var getObjectArgs =  new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName)
            .WithCallbackStream((stream) =>
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                // stream.CopyTo(Console.OpenStandardOutput());
                // stream.CopyTo(ms);
            });

        await minioClient.GetObjectAsync(getObjectArgs);

        var fileBytes = ms.ToArray();
        var fileBase64 = Convert.ToBase64String(fileBytes);

        return fileBase64;

    }
}