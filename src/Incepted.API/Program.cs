using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddApplicationInsightsTelemetry();
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
        {
            c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
            c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidAudience = builder.Configuration["Auth0:Audience"],
                ValidIssuer = builder.Configuration["Auth0:Domain"]
            };
        });
    builder.Services.AddControllersWithViews()
        .AddJsonOptions(options => {
            //to add json converters etc
        });
    builder.Services.AddRazorPages();
    builder.Services.AddHttpContextAccessor();

    AddSwaggerService(builder);

    Incepted.Domain.Deals.ConfigureServices.With(builder.Services);
    Incepted.Domain.Companies.ConfigureServices.With(builder.Services);
    Incepted.Db.ConfigureServices.With(builder.Services);
    Incepted.Files.ConfigureServices.With(builder.Services);
    Incepted.DocGen.ConfigureServices.With(builder.Services);
    Incepted.Notifications.ConfigureServices.With(builder.Services);

    // Add logging
    builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                // using static System.Net.Mime.MediaTypeNames;
                context.Response.ContentType = Text.Plain;

                await context.Response.WriteAsync("An exception was thrown.");

                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();

                if (exceptionHandlerPathFeature?.Error is not null)
                {
                    var ex = exceptionHandlerPathFeature.Error;
                    Log.Error("Global error caught. Reason: {ErrorType} | {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        ex.GetType().Name, 500, ex.Message, ex.StackTrace ?? "no stack trace");

                    //await context.Response.WriteAsync($"Global error caught. Reason: " +
                    //    $"{ex.GetType().Name} | 500 | {ex.Message} | {ex.StackTrace ?? "no stack trace"}");
                }
            });
        });
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    
    app.UseSerilogIngestion();
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        };
    });

    app.UseHttpsRedirection();

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to start the API server");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

void AddSwaggerService(WebApplicationBuilder builder)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Incepted API",
            Description = "The back end API to the Incepted platform",
            Contact = new OpenApiContact
            {
                Name = "Incepted Tech",
                Email = "tech@incepted.co.uk"
            },
        });

        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[]{}
            }
        });
    });
}

[ExcludeFromCodeCoverage]
public partial class Program { }