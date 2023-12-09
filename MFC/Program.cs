using MFC.Configurations;
using MFC.Interfaces;
using MFC.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add the HttpClient service
builder.Services.AddHttpClient();
// Add services to the container.
// Register the access token provider with the DI container

builder.Services.Configure<AllegroApiSettings>(builder.Configuration.GetSection("AllegroApiSettings"));

builder.Services.AddSingleton<IAccessTokenProvider>(sp =>
{
    var options = sp.GetRequiredService<IOptions<AllegroApiSettings>>().Value;
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new AccessTokenProvider(httpClient, options.ClientId, options.ClientSecret, options.TokenUrl, options.AuthorizationEndpoint);
});

builder.Services.AddScoped<IAllegroApiService, AllegroApiService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<ICalculationService, CalculationService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();