using Amazon.S3;
using FieldControl.Minio.Data;
using FieldControl.Minio.Interfaces;
using FieldControl.Minio.Services.InspectionAppService;
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

    return new AmazonS3Client(
        minio["AccessKey"],
        minio["SecretKey"],
        new AmazonS3Config
        {
            ServiceURL = minio["Endpoint"],
            ForcePathStyle = true // ⚠️ MinIO için zorunlu
        });
});

// SERVICES (DI)
builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
builder.Services.AddScoped<InspectionAppService>();

var app = builder.Build();

// MIDDLEWARE PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();