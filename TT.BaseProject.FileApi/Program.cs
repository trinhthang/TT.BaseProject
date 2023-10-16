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
builder.Services.Configure<StorageConfig>
        (builder.Configuration.GetSection("Storage"));

// Add services to the container.
// Common service
builder.Services.AddSingleton<ITypeService, TypeService>();
builder.Services.AddSingleton<ISerializerService, SerializerService>();

// Context
builder.Services.AddScoped<IContextService, ContextService>();

// TODO Service
HostBaseFactory.InjectStorageService(builder.Services, builder.Configuration);


builder.Services.AddSingleton<IStorageService, FileStorageService>();
//builder.Services.AddSingleton<IStorageService, MinIoStorageService>();


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
