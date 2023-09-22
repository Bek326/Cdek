using Cdek.Requests;
using Cdek.Services;
using Cdek.Services.Interfaces;
using Cdek.Settings;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureApp(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Swagger
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cdek API", Version = "v1" }); });
    
    // Configuration
    services.Configure<CdekSettings>(configuration.GetSection("CdekSettings"));

    // Http Client
    services.AddHttpClient();

    //Controllers
    builder.Services.AddControllers();

    // Services
    services.AddScoped<ICdekDeliveryService, CdekDeliveryService>();
    services.AddScoped<ICdekAccessTokenService, CdekAccessTokenService>();
}

void ConfigureApp(WebApplication application)
{
    // Swagger Middleware
    application.UseSwagger();
    application.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cdek API V1"); });

    application.MapControllers();
    
    // Routes
    application.MapPost("/calculate-delivery-cost", async (CdekDeliveryRequest request,
        ICdekDeliveryService deliveryService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var cost = await deliveryService.CalculateDeliveryCostAsync(request, cancellationToken);

            return Results.Ok(new { Cost = cost });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    });
}