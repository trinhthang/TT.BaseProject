﻿using TT.BaseProject.Application.Business;
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

// Config
builder.Services.Configure<ConnectionConfig>
        (builder.Configuration.GetSection("ConnectionStrings"));

// Add services to the container.
// Common service
builder.Services.AddSingleton<ITypeService, TypeService>();
builder.Services.AddSingleton<ISerializerService, SerializerService>();

// Context
builder.Services.AddScoped<IContextService, ContextService>();

// Service ...
builder.Services.AddScoped<IExampleService, ExampleService>();

// Repo ...
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
