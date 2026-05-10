using Amazon.S3;
using FieldControl.Minio.Data;
using FieldControl.Minio.Interfaces;
using FieldControl.Minio.Services.BucketService;
using FieldControl.Minio.Services.Inspection;
using FieldControl.Minio.Services.InspectionFileService;
using FieldControl.Minio.Services.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// CONTROLLERS & SWAGGER
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DATABASE (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

// MINIO (S3 CLIENT)

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var minio = config.GetSection("MinioSettings");

    var accessKey = minio["AccessKey"]!;
    var secretKey = minio["SecretKey"]!;
    var endpoint = minio["Endpoint"]!;

    return new AmazonS3Client(
        accessKey,
        secretKey,
        new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            UseHttp = true //  MinIO için 
        });
});

// SERVICES (DI)
builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
builder.Services.AddScoped<InspectionFileService>();
builder.Services.AddScoped<InspectionService>();
builder.Services.AddScoped<BucketService>();


// PIPELINE
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();