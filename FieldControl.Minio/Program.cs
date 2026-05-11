using Amazon.S3;
using FieldControl.Minio.Data;
using FieldControl.Minio.Interfaces;
using FieldControl.Minio.Services.BucketService;
using FieldControl.Minio.Services.Inspection;
using FieldControl.Minio.Services.InspectionFileService;
using FieldControl.Minio.Services.Report;
using FieldControl.Minio.Services.Storage;
using FieldControl.Minio.Services.StorageCleanup;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.AllowSynchronousIO = true;
});

//  KESTREL LIMIT
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
});

//  MULTIPART LIMIT
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});

// CONTROLLERS & SWAGGER
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DATABASE
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

// MINIO
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var minio = config.GetSection("MinioSettings");

    return new AmazonS3Client(
        minio["AccessKey"]!,
        minio["SecretKey"]!,
        new AmazonS3Config
        {
            ServiceURL = minio["Endpoint"]!,
            ForcePathStyle = true,
            UseHttp = true
        });
});

// SERVICES
builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
builder.Services.AddScoped<InspectionFileService>();
builder.Services.AddScoped<InspectionService>();
builder.Services.AddScoped<BucketService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<StorageCleanupService>();

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