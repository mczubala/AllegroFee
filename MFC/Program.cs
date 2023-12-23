using Azure.Identity;
using FluentValidation.AspNetCore;
using MFC.Configurations;
using MFC.Interfaces;
using MFC.Services;
using Microsoft.Extensions.Options;
using Refit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.CookiePolicy;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add the HttpClient service
builder.Services.AddHttpClient();
// Add services to the container.
// builder.Configuration.AddAzureKeyVault(
//     new Uri(builder.Configuration["https://mfcwebapi.vault.azure.net/"]),
//     new DefaultAzureCredential());

var settings = new RefitSettings();
settings.ContentSerializer = new NewtonsoftJsonContentSerializer(new JsonSerializerSettings
{
    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
});

builder.Services.AddRefitClient<IAllegroApiClient>(settings)
    .ConfigureHttpClient((sp, c) =>
    {
        var options = sp.GetRequiredService<IOptions<AllegroApiSettings>>().Value;
        c.BaseAddress = new Uri(options.AllegroApiBaseUrl);
    });


builder.Services.Configure<AllegroApiSettings>(builder.Configuration.GetSection("AllegroApiSettings"));

// Register the access token provider with the DI container
builder.Services.AddSingleton<IAccessTokenProvider>(sp =>
{
    var options = sp.GetRequiredService<IOptions<AllegroApiSettings>>().Value;
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new AccessTokenProvider(httpClient, options.ClientId, options.ClientSecret, options.TokenUrl, options.AuthorizationEndpoint);
});
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax; 
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

builder.Services.Configure<GoogleOAuthSettings>(builder.Configuration.GetSection("GoogleOAuth"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<GoogleOAuthSettings>>().Value);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(options =>
    {
        var googleSettings = builder.Configuration.GetSection("GoogleOAuth").Get<GoogleOAuthSettings>();
        options.ClientId = googleSettings.ClientId;
        options.ClientSecret = googleSettings.ClientSecret;
    });

builder.Services.AddScoped<IAllegroApiService, AllegroApiService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<ICalculationService, CalculationService>();

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();
app.UseCookiePolicy();
app.UseAuthentication();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();