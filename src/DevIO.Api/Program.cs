using DevIO.Api.Configuration;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;

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

builder.Services.ResolveDependecies();

var app = builder.Build();

app.UseAuthentication();
app.UseMvcConfiguration(app.Environment);
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();


app.UseSwaggerConfig(apiVersionDescriptionProvider);

app.UseAuthorization();
app.MapControllers();
app.Run();
