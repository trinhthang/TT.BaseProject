using TT.BaseProject.Application.Contracts.Common;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Domain.Context;
using TT.BaseProject.HostBase;
using TT.BaseProject.HostBase.Service;
using TT.BaseProject.Library.Service;
using TT.BaseProject.Storage;
using TT.BaseProject.Storage.FileSystem;
using TT.BaseProject.Storage.MinIo;

var builder = WebApplication.CreateBuilder(args);

// Config
builder.Services.Configure<ConnectionConfig>
        (builder.Configuration.GetSection("Connections"));
builder.Services.Configure<StorageConfig>
        (builder.Configuration.GetSection("Storage"));
builder.Services.Configure<StorageConfig>
        (builder.Configuration.GetSection("Cache"));

// Add services to the container.
// Common service
builder.Services.AddSingleton<ITypeService, TypeService>();
builder.Services.AddSingleton<ISerializerService, SerializerService>();

// Context
builder.Services.AddScoped<IContextService, ContextService>();

// Inject StorageService
HostBaseFactory.InjectStorageService(builder.Services, builder.Configuration);
// Inject CacheService
HostBaseFactory.InjectCacheService(builder.Services, builder.Configuration);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
