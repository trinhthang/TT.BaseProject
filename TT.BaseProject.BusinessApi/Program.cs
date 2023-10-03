using TT.BaseProject.Application.Business;
using TT.BaseProject.Application.Contracts.Business;
using TT.BaseProject.Application.Contracts.Common;
using TT.BaseProject.Domain.Business;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Domain.Context;
using TT.BaseProject.Domain.MySql.Business;
using TT.BaseProject.HostBase.Filter;
using TT.BaseProject.HostBase.Service;
using TT.BaseProject.Library.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ConnectionConfig>
        (builder.Configuration.GetSection("ConnectionStrings"));

// Inject các service
builder.Services.AddSingleton<ITypeService, TypeService>();
builder.Services.AddSingleton<ISerializerService, SerializerService>();
//
builder.Services.AddScoped<IContextService, ContextService>();
//
builder.Services.AddScoped<IExampleService, ExampleService>();
//
builder.Services.AddScoped<IExampleRepo, MySqlExampleRepo>();


// Bọc bắt exception
builder.Services.AddControllers(options =>
{
    // Add custom exception
    options.Filters.Add<CustomExceptionFilter>();
});

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
