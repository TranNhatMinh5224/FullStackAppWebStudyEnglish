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
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Application.Service;
using LearningEnglish.Application.Service.ProgressServices;
using LearningEnglish.Application.Service.PaymentProcessors;
using LearningEnglish.Application.Service.ScoringStrategies;
using LearningEnglish.Application.Service.BackgroundJobs;
using LearningEnglish.Application.Validators;
using LearningEnglish.Infrastructure.Repositories;
using LearningEnglish.Infrastructure.Services;
using LearningEnglish.Application.Cofigurations;
using Microsoft.Extensions.Options;
using Minio;
using LearningEnglish.Infrastructure.MinioFileStorage;


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
builder.Services.AddScoped<IFlashCardReviewRepository, FlashCardReviewRepository>();
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
builder.Services.AddScoped<IPronunciationAssessmentRepository, PronunciationAssessmentRepository>();
builder.Services.AddScoped<IPronunciationProgressRepository, PronunciationProgressRepository>();
builder.Services.AddScoped<ICourseProgressRepository, CourseProgressRepository>();
builder.Services.AddScoped<ILessonCompletionRepository, LessonCompletionRepository>();
builder.Services.AddScoped<IModuleCompletionRepository, ModuleCompletionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// Service layer
builder.Services.AddScoped<IAdminCourseService, AdminCourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<ILectureService, LectureService>();
builder.Services.AddScoped<IFlashCardService, FlashCardService>();
builder.Services.AddScoped<IVocabularyReviewService, VocabularyReviewService>();
builder.Services.AddScoped<IStreakRepository, StreakRepository>();
builder.Services.AddScoped<IStreakService, StreakService>();
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
builder.Services.AddScoped<IEmailSender, EmailSender>();
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
builder.Services.AddScoped<IPronunciationAssessmentService, PronunciationAssessmentService>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();
builder.Services.AddScoped<ICourseProgressService, CourseProgressService>();
builder.Services.AddScoped<ILessonProgressService, LessonProgressService>();
builder.Services.AddScoped<IModuleProgressService, ModuleProgressService>();
builder.Services.AddScoped<IProgressDashboardService, ProgressDashboardService>();

// 🎤 Azure Speech Service for Pronunciation Assessment
builder.Services.AddScoped<IAzureSpeechService, AzureSpeechService>();

// HttpClient for Dictionary API
builder.Services.AddHttpClient();

// Oxford Dictionary Configuration
builder.Services.Configure<OxfordDictionaryOptions>(builder.Configuration.GetSection("OxfordDictionary"));

// Unsplash Configuration
builder.Services.Configure<UnsplashOptions>(builder.Configuration.GetSection("Unsplash"));

// MinIO Configuration
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("MinIO"));

// MinIO Client (Singleton - dùng chung cho toàn bộ app)
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
    var client = new MinioClient()
        .WithEndpoint(options.Endpoint)
        .WithCredentials(options.AccessKey, options.SecretKey);

    if (options.UseSSL)
        client.WithSSL();

    return client.Build();
});

// File Storage Service
builder.Services.AddScoped<IMinioFileStorage, MinioFileStorageService>();

// Azure Speech Service
builder.Services.AddHttpClient<IAzureSpeechService, AzureSpeechService>();

// Background Jobs
builder.Services.AddScoped<TempFileCleanupJob>();

// Payment related services
builder.Services.AddScoped<IPaymentValidator, PaymentValidator>();
builder.Services.AddScoped<IPaymentNotificationService, PaymentNotificationService>();
builder.Services.AddScoped<IPaymentStrategy, CoursePaymentProcessor>();
builder.Services.AddScoped<IPaymentStrategy, TeacherPackagePaymentProcessor>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateLectureDtoValidator>();
// Explicit validators for notifications and reminders (also discovered via assembly scan)
builder.Services.AddTransient<FluentValidation.IValidator<LearningEnglish.Application.DTOs.CreateNotificationDto>, LearningEnglish.Application.Validators.NotificationValidators.CreateNotificationDtoValidator>();
builder.Services.AddTransient<FluentValidation.IValidator<LearningEnglish.Application.DTOs.CreateStudyReminderDto>, LearningEnglish.Application.Validators.StudyReminderValidators.CreateStudyReminderDtoValidator>();

// Scoring strategies
builder.Services.AddScoped<IScoringStrategy, FillBlankScoringStrategy>();
builder.Services.AddScoped<IScoringStrategy, MultipleChoiceScoringStrategy>();
builder.Services.AddScoped<IScoringStrategy, TrueFalseScoringStrategy>();
builder.Services.AddScoped<IScoringStrategy, MultipleAnswersScoringStrategy>();
builder.Services.AddScoped<IScoringStrategy, MatchingScoringStrategy>();
builder.Services.AddScoped<IScoringStrategy, OrderingScoringStrategy>();

// Background services
builder.Services.AddHostedService<QuizAutoSubmitService>();
builder.Services.AddHostedService<TempFileCleanupHostedService>();
builder.Services.AddHostedService<StudyReminderJob>();

// Notification services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStudyReminderService, StudyReminderService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IStudyReminderRepository, StudyReminderRepository>();

// Build app
var app = builder.Build();

// Configure BuildPublicUrl helper for MinIO public URLs
LearningEnglish.Application.Common.Helpers.BuildPublicUrl.Configure(builder.Configuration);

// Auto-migrate database - TEMPORARILY DISABLED (DNS issue)
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     db.Database.Migrate();
// }

// Middleware pipeline
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
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
