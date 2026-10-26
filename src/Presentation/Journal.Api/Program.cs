using System.Text.Json.Serialization;
using Journal.Application.Common.Interfaces;
using Journal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Register MediatR to scan the Application layer
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IAppDbContext).Assembly));

// 2. Register the IAppDbContext Interface to the concrete AppDbContext
builder.Services.AddScoped<IAppDbContext, AppDbContext>();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention());

var app = builder.Build();

app.MapControllers();

app.Run();