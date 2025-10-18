using Microsoft.EntityFrameworkCore;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Service;
using CleanDemo.Infrastructure.Data;
using CleanDemo.Infrastructure.Repositories;
using CleanDemo.Infrastructure.Services;
using CleanDemo.Application.Validators;
using CleanDemo.Application.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using CleanDemo.Application.Service.Auth.Register;
using CleanDemo.Application.Service.Auth.Login;
using CleanDemo.Application.Service.Auth.Token;
using CleanDemo.Application.Validators.User;
using Microsoft.OpenApi.Models;
using DotNetEnv;

// Load environment variables from .env
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Gộp .env vào Configuration 
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// ==================
// 🔹 CORS CONFIG
// ==================
var frontendUrl = Environment.GetEnvironmentVariable("Frontend__BaseUrl")
                  ?? builder.Configuration["Frontend:BaseUrl"]
                  ?? "http://localhost:3000";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ==================
// 🔹 AUTOMAPPER
// ==================
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ==================
// 🔹 SWAGGER
// ==================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FullStack English Learning API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// ==================
// 🔹 DATABASE
// ==================
var dbServer = Environment.GetEnvironmentVariable("DB__Server");
var dbName = Environment.GetEnvironmentVariable("DB__Name");
var trusted = Environment.GetEnvironmentVariable("DB__Trusted_Connection");
var encrypt = Environment.GetEnvironmentVariable("DB__Encrypt");

var connectionString = $"Server={dbServer};Database={dbName};Trusted_Connection={trusted};Encrypt={encrypt};";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==================
// 🔹 JWT AUTH
// ==================
var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key")
             ?? builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("JWT Key not configured");

var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ==================
// 🔹 DEPENDENCY INJECTION
// ==================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddScoped<ITeacherPackageRepository, TeacherPackageRepository>();
builder.Services.AddScoped<ITeacherPackageService, TeacherPackageService>();

builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserDtoValidator>();
builder.Services.AddValidatorsFromAssembly(typeof(CourseService).Assembly);
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollCourseService, EnrollCourseService>();

// ==================
// 🔹 BUILD APP
// ==================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
