using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;

using LearningEnglish.Infrastructure.Data;
using LearningEnglish.Application.Mappings;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Service;
using LearningEnglish.Application.Service.PaymentProcessors;
using LearningEnglish.Application.Service.ScoringStrategies;
using LearningEnglish.Application.Validators;
using LearningEnglish.Infrastructure.Repositories;
using LearningEnglish.Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);

// Load configuration (appsettings + environment)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// đọc cấu hình từ appsettings
var frontendUrl = builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Validate JWT
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key is missing or too short (>=32 chars).");
if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("Jwt:Issuer is missing.");
if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Jwt:Audience is missing.");

// Add controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

// CORS có chức năng cho phép frontend truy cập API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", p =>
        p.WithOrigins(frontendUrl)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});


// FluentValidation
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Database (PostgreSQL)
if (string.IsNullOrWhiteSpace(conn))
    throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(conn, npgsql =>
    {
        npgsql.EnableRetryOnFailure(0);
    }));

// JWT authentication
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

// Repository layer
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<ILectureRepository, LectureRepository>();
builder.Services.AddScoped<IFlashCardRepository, FlashCardRepository>();
builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<IEssayRepository, EssayRepository>();
builder.Services.AddScoped<IEssaySubmissionRepository, EssaySubmissionRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITeacherPackageRepository, TeacherPackageRepository>();
builder.Services.AddScoped<ITeacherSubscriptionRepository, TeacherSubscriptionRepository>();
builder.Services.AddScoped<IQuizSectionRepository, QuizSectionRepository>();
builder.Services.AddScoped<IQuizGroupRepository, QuizGroupRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// Service layer
builder.Services.AddScoped<IAdminCourseService, AdminCourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<ILectureService, LectureService>();
builder.Services.AddScoped<IFlashCardService, FlashCardService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IEssayService, EssayService>();
builder.Services.AddScoped<IEssaySubmissionService, EssaySubmissionService>();
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
builder.Services.AddScoped<ITemplatePathResolver, TemplatePathResolver>();
builder.Services.AddScoped<IUserCourseService, UserCourseService>();
builder.Services.AddScoped<IEnrollmentQueryService, EnrollmentQueryService>();
builder.Services.AddScoped<IQuizSectionService, QuizSectionService>();
builder.Services.AddScoped<IQuizGroupService, QuizGroupService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
builder.Services.AddScoped<IQuizAttemptAdminService, QuizAttemptAdminService>();

// Payment related services
builder.Services.AddScoped<IPaymentValidator, PaymentValidator>();
builder.Services.AddScoped<IPaymentProcessorFactory, PaymentProcessorFactory>();
builder.Services.AddScoped<IPaymentNotificationService, PaymentNotificationService>();
builder.Services.AddScoped<CoursePaymentProcessor>();
builder.Services.AddScoped<TeacherPackagePaymentProcessor>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateLectureDtoValidator>();

// Scoring strategies
builder.Services.AddScoped<FillBlankScoringStrategy>();
builder.Services.AddScoped<MultipleChoiceScoringStrategy>();
builder.Services.AddScoped<TrueFalseScoringStrategy>();
builder.Services.AddScoped<MultipleAnswersScoringStrategy>();
builder.Services.AddScoped<MatchingScoringStrategy>();
builder.Services.AddScoped<OrderingScoringStrategy>();

// Background services
builder.Services.AddHostedService<QuizAutoSubmitService>();

// Build app
var app = builder.Build();

// Middleware pipeline
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
