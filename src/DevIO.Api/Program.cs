using DevIO.Api.Configuration;
using DevIO.Data.Context;
//using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using SoftwareTech.IO.Api.Configuration;
using SoftwareTech.IO.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

// ConfigureServices
builder.Services.AddDbContext<MeuDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentityConfiguration(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.WebApiConfig();

builder.Services.AddSwaggerConfig();


builder.Services.AddLoggingConfig(builder.Configuration);

builder.Services.ResolveDependecies();

var app = builder.Build();

app.UseAuthentication();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMvcConfiguration(app.Environment);
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();


app.UseSwaggerConfig(apiVersionDescriptionProvider);
app.UseLoggingConfiguration();
app.UseAuthorization();

app.MapControllers();
app.Run();
