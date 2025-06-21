using EdTech.Interfaces;
using EdTech.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<EdTech.Interfaces.IConnectionProvider, EdTech.Utils.ConnectionProvider>();
builder.Services.AddSingleton<EdTech.Interfaces.IBCryptHelper, BCryptHelper>();
builder.Services.AddScoped<ITools, Tools>();
builder.Services.AddSwaggerGen();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Disable certificate validation in development (only for local testing)
    System.Net.ServicePointManager.ServerCertificateValidationCallback =
        (sender, certificate, chain, sslPolicyErrors) => true;
    //app.UseSwagger();
    app.UseSwagger(options =>
    {
        options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
    });
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

ControllerActionEndpointConventionBuilder controllerActionEndpointConventionBuilder = app.MapControllers();

app.Run();
