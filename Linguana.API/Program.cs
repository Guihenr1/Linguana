using Azure.Identity;
using Linguana.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add this logging configuration
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Modify the Key Vault configuration to include specific options
var keyVaultUri = "https://kv-linguana-dev.vault.azure.net/";
var credentialOptions = new DefaultAzureCredentialOptions
{
    Retry = { MaxRetries = 3, NetworkTimeout = TimeSpan.FromSeconds(5) },
    ExcludeVisualStudioCredential = true,
    ExcludeAzureCliCredential = false,
    ExcludeInteractiveBrowserCredential = true,
    ExcludeManagedIdentityCredential = false
};

try
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential(credentialOptions));
        
    // Add debug logging to check Key Vault configuration
    var logger = LoggerFactory.Create(config => 
    {
        config.AddConsole();
    }).CreateLogger("Program");
    
    logger.LogInformation("Azure Key Vault configured with URI: {uri}", keyVaultUri);
    
    // Check if we can access the configuration
    var allKeys = builder.Configuration.AsEnumerable().ToList();
    logger.LogInformation("Total configuration keys: {count}", allKeys.Count);
    
    // Specifically check for OpenAiApiKey
    var openAiKey = builder.Configuration["OpenAiApiKey"];
    logger.LogInformation("OpenAiApiKey exists: {exists}", !string.IsNullOrEmpty(openAiKey));
    
    // List all available keys from Key Vault (just names, not values)
    var keyVaultKeys = allKeys
        .Where(k => k.Key.StartsWith("OpenAi"))
        .Select(k => k.Key)
        .ToList();
    logger.LogInformation("Found Key Vault keys: {keys}", string.Join(", ", keyVaultKeys));
}
catch (Exception ex)
{
    builder.Logging.AddConsole();
    var logger = LoggerFactory.Create(config => 
    {
        config.AddConsole();
    }).CreateLogger("Program");
    
    logger.LogError(ex, "Failed to configure Azure Key Vault");
}

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors();

app.Run();