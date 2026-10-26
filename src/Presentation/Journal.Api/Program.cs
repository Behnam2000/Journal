using System.Text.Json.Serialization;
<<<<<<< HEAD
using Journal.Application.Common.Interfaces;
using Journal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
=======
using Journal.Application;
using Journal.Infrastructure;
>>>>>>> devin/1792968709-step2-layering

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

app.MapControllers();

app.Run();
