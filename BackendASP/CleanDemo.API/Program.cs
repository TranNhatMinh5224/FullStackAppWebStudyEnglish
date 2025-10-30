using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using CleanDemo.Infrastructure.Data;
using CleanDemo.Application.Mappings;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Service;
using CleanDemo.Infrastructure.Repositories;
using CleanDemo.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// load config từ appsettings và biến môi trường
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// đọc config cần thiết }
var frontendUrl = builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// validate config JWT }
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key is missing or too short (>=32 chars).");
if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("Jwt:Issuer is missing.");
if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Jwt:Audience is missing.");

// add controller + swagger endpoint
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// swagger config + JWT authorize button
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FullStack English Learning API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
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
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
            });
});

// CORS cho phép FE gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", p =>
            p.WithOrigins(frontendUrl)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials());
});

// FluentValidation tự động quét validators trong Application
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

// AutoMapper mapping giữa DTO và Entity
builder.Services.AddAutoMapper(typeof(MappingProfile));

// cấu hình database PostgreSQL
if (string.IsNullOrWhiteSpace(conn))
    throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(conn, npgsql =>
    {
        npgsql.EnableRetryOnFailure(0);
    }));

// JWT Auth setup
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// register repository layer 
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITeacherPackageRepository, TeacherPackageRepository>();
builder.Services.AddScoped<ITeacherSubscriptionRepository, TeacherSubscriptionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// register service layer 
builder.Services.AddScoped<IAdminCourseService, AdminCourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ITeacherCourseService, TeacherCourseService>();
builder.Services.AddScoped<ITeacherPackageService, TeacherPackageService>();
builder.Services.AddScoped<ITeacherSubscriptionService, TeacherSubscriptionService>();
builder.Services.AddScoped<IUserEnrollmentService, UserEnrollmentService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IUserCourseService, UserCourseService>();
builder.Services.AddScoped<IEnrollmentQueryService, EnrollmentQueryService>();
builder.Services.AddScoped<IProgressService, ProgressService>();

// build app 
var app = builder.Build();

// middleware pipeline 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
