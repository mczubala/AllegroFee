using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add the HttpClient service
builder.Services.AddHttpClient();
// Add services to the container.
// Register the access token provider with the DI container
builder.Services.AddSingleton<IAccessTokenProvider>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var clientId = builder.Configuration["clientId"];
    var clientSecret = builder.Configuration["clientSecret"];
    var tokenUrl = builder.Configuration["tokenUrl"];
    var authorizationEndpoint = builder.Configuration["authorizationEndpoint"];
    return new AccessTokenProvider(httpClient, clientId, clientSecret, tokenUrl, authorizationEndpoint);
});

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