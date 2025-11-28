using Azure.Identity;
using HCI.AIAssistant.API.Managers;
using HCI.AIAssistant.API.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CORS",
    policy =>
    {
        policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();
    });
});

// Replace appsettings.json values with Key Vault values
var keyVaultName = builder.Configuration[$"AppConfigurations{ConfigurationPath.KeyDelimiter}KeyVaultName"];
var secretsPrefix = builder.Configuration[$"AppConfigurations{ConfigurationPath.KeyDelimiter}SecretsPrefix"];
if (string.IsNullOrWhiteSpace(keyVaultName))
{
    throw new ArgumentNullException("KeyVaultName", "KeyVaultName is missing.");
}
if (string.IsNullOrWhiteSpace(secretsPrefix))
{
    throw new ArgumentNullException("SecretsPrefix", "SecretsPrefix is missing.");
}
var keyVaultUri = new Uri(
 $"https://{keyVaultName}.vault.azure.net/"
);

builder.Configuration.AddAzureKeyVault(
 keyVaultUri,
 new DefaultAzureCredential(),
 new CustomSecretManager(secretsPrefix)
);



// Add services to the container.

// Configure values based on appsettings.json
builder.Services.Configure<SecretsService>(builder.Configuration.GetSection("Secrets"));
builder.Services.Configure<AppConfigurationsService>(builder.Configuration.GetSection("AppConfigurations"));
// Add services to the container.
builder.Services.AddSingleton<ISecretsService>(
provider => provider.GetRequiredService<IOptions<SecretsService>>().Value
);
builder.Services.AddSingleton<IAppConfigurationsService>(
provider => provider.GetRequiredService<IOptions<AppConfigurationsService>>().Value
);
builder.Services.AddSingleton<IParametricFunctions, ParametricFunctions>();
builder.Services.AddSingleton<IAIAssistantService, AIAssistantService>();




builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("CORS");

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

Console.WriteLine(app.Services.GetService<ISecretsService>()?.AIAssistantSecrets?.EndPoint);
Console.WriteLine(app.Services.GetService<ISecretsService>()?.AIAssistantSecrets?.Key);
Console.WriteLine(app.Services.GetService<ISecretsService>()?.AIAssistantSecrets?.Id);
Console.WriteLine(app.Services.GetService<ISecretsService>()?.IoTHubSecrets?.ConnectionString);
Console.WriteLine(app.Services.GetService<IAppConfigurationsService>()?.KeyVaultName);
Console.WriteLine(app.Services.GetService<IAppConfigurationsService>()?.SecretsPrefix);
Console.WriteLine(app.Services.GetService<IAppConfigurationsService>()?.IoTDeviceName);

app.Run();
