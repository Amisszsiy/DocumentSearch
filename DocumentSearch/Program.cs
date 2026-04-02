using DocumentSearch.Auth;
using DocumentSearch.Extensions;
using DocumentSearch.Persistance;
using DocumentSearch.Persistance.Seed;
using DocumentSearch.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DocumentDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHttpClient("ThaiTokenizer", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ThaiTokenizer:BaseUrl"]!);
});

builder.Services.AddScoped<IThaiTokenizerService, ThaiTokenizerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithApiKey();

builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Uncomment to seed
//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
//    var tokenizer = scope.ServiceProvider.GetRequiredService<IThaiTokenizerService>();
//    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
//    await DocumentSeeder.SeedAsync(context, tokenizer, logger);
//}

app.Run();
