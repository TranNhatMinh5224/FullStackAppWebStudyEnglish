using System.Text;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using CleanDemo.Infrastructure.Data;
using CleanDemo.Application.Mappings;

// =====================================
// 0) Builder
// =====================================
var builder = WebApplication.CreateBuilder(args);

// 1) Load .env 
var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envPath)) Env.Load(envPath);


builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// =====================================
// 3) Đọc biến môi trường (đã có hậu tố _ASPELEARNING như bạn dùng)
// =====================================
string dbHost = Environment.GetEnvironmentVariable("DB__Server_ASPELEARNING") ?? "localhost";
string dbPort = Environment.GetEnvironmentVariable("DB__Port_ASPELEARNING") ?? "5432";
string dbName = Environment.GetEnvironmentVariable("DB__Name_ASPELEARNING") ?? "Elearning";
string dbUser = Environment.GetEnvironmentVariable("DB__User_ASPELEARNING") ?? "postgres";
string dbPassword = Environment.GetEnvironmentVariable("DB__Password_ASPELEARNING") ?? "05022004";
string sslMode = Environment.GetEnvironmentVariable("DB__SslMode_ASPELEARNING") ?? "Disable";
string trustCert = Environment.GetEnvironmentVariable("DB__TrustServerCertificate_ASPELEARNING") ?? "true";

string? jwtKey = Environment.GetEnvironmentVariable("Jwt__Key_ASPELEARNING");

string? jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer_ASPELEARNING");
string? jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience_ASPELEARNING");

string frontendUrl = Environment.GetEnvironmentVariable("Frontend__BaseUrl") ?? "http://localhost:3000";

// Fail-fast: kiểm tra JWT config
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt__Key_ASPELEARNING is missing or too short (>=32 chars).");
if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("Jwt__Issuer_ASPELEARNING is missing.");
if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Jwt__Audience_ASPELEARNING is missing.");

// =====================================
// 4) Services
// =====================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT
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

// CORS (BẬT lại, khớp với UseCors phía dưới)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", p =>
        p.WithOrigins(frontendUrl)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CleanDemo.Application.Validators.CourseValidators.AdminCreateCourseRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CleanDemo.Application.Validators.Payment.RequestPaymentValidator>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Database (PostgreSQL) — dùng biến môi trường
var npgConnection =
    $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};SSL Mode={sslMode};Trust Server Certificate={trustCert};";

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(npgConnection, npgsql => 
    {
        // Tắt retry strategy để tương thích với manual transactions
        npgsql.EnableRetryOnFailure(0);
       
    }));

// JWT Authentication — dùng key đã validate
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
            ClockSkew = TimeSpan.FromMinutes(2) // optional
        };
    });

builder.Services.AddAuthorization();

// Repositories & Services 
builder.Services.AddScoped<CleanDemo.Application.Interface.ICourseRepository, CleanDemo.Infrastructure.Repositories.CourseRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IUserRepository, CleanDemo.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ILessonRepository, CleanDemo.Infrastructure.Repositories.LessonRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IPasswordResetTokenRepository, CleanDemo.Infrastructure.Repositories.PasswordResetTokenRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IPaymentRepository, CleanDemo.Infrastructure.Repositories.PaymentRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IRefreshTokenRepository, CleanDemo.Infrastructure.Repositories.RefreshTokenRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ITeacherPackageRepository, CleanDemo.Infrastructure.Repositories.TeacherPackageRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ITeacherSubscriptionRepository, CleanDemo.Infrastructure.Repositories.TeacherSubscriptionRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IUnitOfWork, CleanDemo.Infrastructure.Repositories.UnitOfWork>();

builder.Services.AddScoped<CleanDemo.Application.Interface.IAdminCourseService, CleanDemo.Application.Service.AdminCourseService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IAuthenticationService, CleanDemo.Application.Service.AuthenticationService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ILessonService, CleanDemo.Application.Service.LessonService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IPasswordService, CleanDemo.Application.Service.PasswordService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IPaymentService, CleanDemo.Application.Service.PaymentService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ITeacherCourseService, CleanDemo.Application.Service.TeacherCourseService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ITeacherPackageService, CleanDemo.Application.Service.TeacherPackageService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ITeacherSubscriptionService, CleanDemo.Application.Service.TeacherSubscriptionService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IUserEnrollmentService, CleanDemo.Application.Service.UserEnrollmentService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IUserManagementService, CleanDemo.Application.Service.UserManagementService>();

builder.Services.AddScoped<CleanDemo.Application.Interface.IRegisterService, CleanDemo.Application.Service.RegisterService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ILoginService, CleanDemo.Application.Service.LoginService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ITokenService, CleanDemo.Application.Service.TokenService>();
builder.Services.AddScoped<CleanDemo.Application.Service.EmailService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IEmailTemplateService, CleanDemo.Infrastructure.Services.EmailTemplateService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IUserCourseService, CleanDemo.Application.Service.UserCourseService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IEnrollmentQueryService, CleanDemo.Application.Service.EnrollmentQueryService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IProgressService, CleanDemo.Application.Service.ProgressService>();
// =====================================
// 5) Pipeline
// =====================================
var app = builder.Build();

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

// Cho test integration
public partial class Program { }
