using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using picture_backend.Models;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using picture_backend.Repositories.IRepositories;
using picture_backend.Repositories;
using picture_backend.Services.IServices;
using picture_backend.Services;

var builder = WebApplication.CreateBuilder(args);

// ����JWT������֤
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        
    };
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<picture_backend.Models.AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IScriptRepository, ScriptRepository>();
builder.Services.AddScoped<IScriptHistoryRepository, ScriptHistoryRepository>();
builder.Services.AddScoped<IScriptAnalysisRepository, ScriptAnalysisRepository>();
builder.Services.AddScoped<IVisualElementRepository, VisualElementRepository>();

builder.Services.AddScoped<IScriptHistoryService, ScriptHistoryService>();
builder.Services.AddScoped<IScriptAnalysisService, ScriptAnalysisService>();
builder.Services.AddScoped<IVisualElementService, VisualElementService>();
builder.Services.AddScoped<IUserLoginService, UserLoginService>();
builder.Services.AddScoped<IScriptService, ScriptService>();
builder.Services.AddScoped<IUserRegisterService, UserRegisterService>();
builder.Services.AddScoped<IAIService, AIService>();

builder.Services.AddScoped<IAIService, AIService>();

builder.Services.AddHttpClient<IAIService, AIService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var serviceProvider = builder.Services.BuildServiceProvider();
var aiService = serviceProvider.GetService<IAIService>();
if (aiService == null)
{
    Console.WriteLine("IAIService δ��ȷע�ᣡ");
}


app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

//app.Run();
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
