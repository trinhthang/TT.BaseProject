using TT.BaseProject.Application.Business;
using TT.BaseProject.Application.Contracts.Auth;
using TT.BaseProject.Application.Contracts.Business;
using TT.BaseProject.Application.Contracts.Common;
using TT.BaseProject.Cache.Models;
using TT.BaseProject.Domain.Authen;
using TT.BaseProject.Domain.Business;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Domain.Context;
using TT.BaseProject.Domain.MySql.Business;
using TT.BaseProject.HostBase;
using TT.BaseProject.HostBase.Service;
using TT.BaseProject.Library.Service;

var builder = WebApplication.CreateBuilder(args);

// Config
builder.Services.Configure<ConnectionConfig>
        (builder.Configuration.GetSection("Connections"));
builder.Services.Configure<AuthConfig>
        (builder.Configuration.GetSection("Auth"));
builder.Services.Configure<CacheConfig>
        (builder.Configuration.GetSection("Cache"));

// Add services to the container.
// Common service
builder.Services.AddSingleton<ITypeService, TypeService>();
builder.Services.AddSingleton<ISerializerService, SerializerService>();

// Context
builder.Services.AddScoped<IContextService, ContextService>();

// Inject Service
builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();
builder.Services.AddScoped<IUserService, UserService>();

// Inject Repo
builder.Services.AddScoped<IAuthenticateRepo, MySqlIAuthenticateRepo>();
builder.Services.AddScoped<IUserRepo, MySqlUserRepo>();

// Inject CacheService
HostBaseFactory.InjectCacheService(builder.Services, builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("corsapp");

app.UseAuthorization();

app.MapControllers();

app.Run();
