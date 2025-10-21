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

// 0) Load ENV file (early)
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// 1) Configuration sources (ENV -> Configuration)
builder.Configuration.AddEnvironmentVariables();

// 2) App constants & ENV reads
var frontendUrl = Environment.GetEnvironmentVariable("Frontend__BaseUrl")
                  ?? builder.Configuration["Frontend:BaseUrl"]
                  ?? "http://localhost:3000";

var dbServer = Environment.GetEnvironmentVariable("DB__Server");
var dbName = Environment.GetEnvironmentVariable("DB__Name");
var trusted = Environment.GetEnvironmentVariable("DB__Trusted_Connection") ?? "True";
var encrypt = Environment.GetEnvironmentVariable("DB__Encrypt") ?? "True";

if (string.IsNullOrWhiteSpace(dbServer) || string.IsNullOrWhiteSpace(dbName))
    throw new InvalidOperationException("DB__Server hoặc DB__Name chưa cấu hình.");

var useTrusted = bool.TryParse(trusted, out var t) ? t : true;
var useEncrypt = bool.TryParse(encrypt, out var e) ? e : true;

var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key")
             ?? builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("Jwt__Key chưa cấu hình.");

var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer")
               ?? builder.Configuration["Jwt:Issuer"]
               ?? throw new InvalidOperationException("Jwt__Issuer chưa cấu hình.");

var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience")
                 ?? builder.Configuration["Jwt:Audience"]
                 ?? throw new InvalidOperationException("Jwt__Audience chưa cấu hình.");


// ===================== SERVICES (DI) =====================
// 3) Core framework services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 4) Swagger + Security (đăng ký trước dùng ở pipeline Dev)
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
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference {
            Type = ReferenceType.SecurityScheme, Id = "Bearer"}}, Array.Empty<string>() }
    });
});

// 5) CORS (đặt sớm, dùng trong pipeline trước Auth)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", p =>
        p.WithOrigins(frontendUrl)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// 6) FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CleanDemo.Application.Validators.CourseValidators.AdminCreateCourseRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CleanDemo.Application.Validators.Payment.RequestPaymentValidator>();

// 7) AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 8) Database (DbContext) — đăng ký trước Auth/Services để sẵn sàng migrate
string connectionString = builder.Environment.IsDevelopment()
    ? @"Server=(localdb)\mssqllocaldb;Database=ELearning_English;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
    : useTrusted
        ? $"Server={dbServer};Database={dbName};Trusted_Connection=True;Encrypt={useEncrypt};TrustServerCertificate=True;"
        : $"Server={dbServer};Database={dbName};User ID={Environment.GetEnvironmentVariable("DB__User")};Password={Environment.GetEnvironmentVariable("DB__Password")};Encrypt={useEncrypt};TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

// 9) AuthN & AuthZ (đăng ký sau DbContext, trước App services)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

// 10) Repositories (Infrastructure)
builder.Services.AddScoped<CleanDemo.Application.Interface.ICourseRepository, CleanDemo.Infrastructure.Repositories.CourseRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IUserRepository, CleanDemo.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ILessonRepository, CleanDemo.Infrastructure.Repositories.LessonRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IPasswordResetTokenRepository, CleanDemo.Infrastructure.Repositories.PasswordResetTokenRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IPaymentRepository, CleanDemo.Infrastructure.Repositories.PaymentRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IRefreshTokenRepository, CleanDemo.Infrastructure.Repositories.RefreshTokenRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ITeacherPackageRepository, CleanDemo.Infrastructure.Repositories.TeacherPackageRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.ITeacherSubscriptionRepository, CleanDemo.Infrastructure.Repositories.TeacherSubscriptionRepository>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IUnitOfWork, CleanDemo.Infrastructure.Repositories.UnitOfWork>();

// 11) Services (Application)
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

// 12) Auth sub-services
builder.Services.AddScoped<CleanDemo.Application.Service.Auth.Register.IRegisterService, CleanDemo.Application.Service.Auth.Register.RegisterService>();
builder.Services.AddScoped<CleanDemo.Application.Service.Auth.Login.ILoginService, CleanDemo.Application.Service.Auth.Login.LoginService>();
builder.Services.AddScoped<CleanDemo.Application.Service.Auth.Token.ITokenService, CleanDemo.Application.Service.Auth.Token.TokenService>();

// 13) Infrastructure services (mail, template, ...)
builder.Services.AddScoped<CleanDemo.Application.Service.EmailService>();
builder.Services.AddScoped<CleanDemo.Application.Interface.IEmailTemplateService, CleanDemo.Infrastructure.Services.EmailTemplateService>();


// ===================== APP PIPELINE =====================
var app = builder.Build();

// (A) Dev tooling
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// (B) Security & infra middlewares – thứ tự quan trọng
app.UseHttpsRedirection();

// Nếu dùng proxy/nginx phía trước: bật ForwardedHeaders trước Routing
// app.UseForwardedHeaders(new ForwardedHeadersOptions {
//     ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
// });

app.UseRouting();

// CORS phải nằm giữa UseRouting và UseAuthentication/UseAuthorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// (C) Endpoints mapping – đặt cuối pipeline sau AuthZ
app.MapControllers();

// (D) DB migration khi khởi động
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

public partial class Program { }
