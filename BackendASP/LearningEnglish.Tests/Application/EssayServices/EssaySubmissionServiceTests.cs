using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Domain.Entities;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Tests.Application.EssayServices;

public class EssaySubmissionServiceTests
{
    private readonly Mock<IEssaySubmissionRepository> _essaySubmissionRepositoryMock;
    private readonly Mock<IEssayRepository> _essayRepositoryMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<EssaySubmissionService>> _loggerMock;
    private readonly Mock<IModuleProgressService> _moduleProgressServiceMock;
    private readonly Mock<IAssessmentRepository> _assessmentRepositoryMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly EssaySubmissionService _essaySubmissionService;

    public EssaySubmissionServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _essaySubmissionRepositoryMock = new Mock<IEssaySubmissionRepository>();
        _essayRepositoryMock = new Mock<IEssayRepository>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<EssaySubmissionService>>();
        _moduleProgressServiceMock = new Mock<IModuleProgressService>();
        _assessmentRepositoryMock = new Mock<IAssessmentRepository>();
        _streakServiceMock = new Mock<IStreakService>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();

        _essaySubmissionService = new EssaySubmissionService(
            _essaySubmissionRepositoryMock.Object,
            _essayRepositoryMock.Object,
            _minioFileStorageMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _moduleProgressServiceMock.Object,
            _assessmentRepositoryMock.Object,
            _streakServiceMock.Object,
            _notificationRepositoryMock.Object
        );
    }

    #region GetSubmissionByIdAsync Tests

    [Fact]
    public async Task GetSubmissionByIdAsync_WithValidId_ReturnsSubmission()
    {
        // Arrange
        var submissionId = 1;
        var submission = new EssaySubmission
        {
            SubmissionId = submissionId,
            EssayId = 1,
            UserId = 1,
            TextContent = "Test submission"
        };

        var submissionDto = new EssaySubmissionDto
        {
            SubmissionId = submissionId,
            EssayId = 1,
            UserId = 1,
            TextContent = "Test submission"
        };

        _essaySubmissionRepositoryMock
            .Setup(x => x.GetSubmissionByIdAsync(submissionId))
            .ReturnsAsync(submission);

        _mapperMock
            .Setup(x => x.Map<EssaySubmissionDto>(submission))
            .Returns(submissionDto);

        // Act
        var result = await _essaySubmissionService.GetSubmissionByIdAsync(submissionId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(submissionId, result.Data.SubmissionId);

        _essaySubmissionRepositoryMock.Verify(x => x.GetSubmissionByIdAsync(submissionId), Times.Once);
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var submissionId = 999;

        _essaySubmissionRepositoryMock
            .Setup(x => x.GetSubmissionByIdAsync(submissionId))
            .ReturnsAsync((EssaySubmission?)null);

        // Act
        var result = await _essaySubmissionService.GetSubmissionByIdAsync(submissionId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Submission không tồn tại", result.Message);
    }

    #endregion

    #region GetSubmissionsByEssayIdAsync Tests

    [Fact]
    public async Task GetSubmissionsByEssayIdAsync_WithValidEssayId_ReturnsSubmissions()
    {
        // Arrange
        var essayId = 1;
        var submissions = new List<EssaySubmission>
        {
            new EssaySubmission
            {
                SubmissionId = 1,
                EssayId = essayId,
                UserId = 1
            },
            new EssaySubmission
            {
                SubmissionId = 2,
                EssayId = essayId,
                UserId = 2
            }
        };

        var submissionListDtos = new List<EssaySubmissionListDto>
        {
            new EssaySubmissionListDto
            {
                SubmissionId = 1,
                EssayId = essayId,
                UserId = 1
            },
            new EssaySubmissionListDto
            {
                SubmissionId = 2,
                EssayId = essayId,
                UserId = 2
            }
        };

        _essaySubmissionRepositoryMock
            .Setup(x => x.GetSubmissionsByEssayIdAsync(essayId))
            .ReturnsAsync(submissions);

        _mapperMock
            .Setup(x => x.Map<List<EssaySubmissionListDto>>(submissions))
            .Returns(submissionListDtos);

        // Act
        var result = await _essaySubmissionService.GetSubmissionsByEssayIdAsync(essayId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
    }

    #endregion

    #region SubmitEssayAsync Tests

    [Fact]
    public async Task SubmitEssayAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateEssaySubmissionDto
        {
            EssayId = 1,
            TextContent = "My essay answer"
        };

        var userId = 1;
        var essay = new Essay
        {
            EssayId = 1,
            Title = "Test Essay"
        };

        var submission = new EssaySubmission
        {
            SubmissionId = 1,
            EssayId = dto.EssayId,
            UserId = userId,
            TextContent = dto.TextContent
        };

        var submissionDto = new EssaySubmissionDto
        {
            SubmissionId = 1,
            EssayId = dto.EssayId,
            UserId = userId,
            TextContent = dto.TextContent
        };

        var assessment = new Assessment
        {
            AssessmentId = 1,
            DueAt = DateTime.UtcNow.AddDays(1)
        };

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(dto.EssayId))
            .ReturnsAsync(essay);

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(essay.AssessmentId))
            .ReturnsAsync(assessment);

        _essaySubmissionRepositoryMock
            .Setup(x => x.CreateSubmissionAsync(It.IsAny<EssaySubmission>()))
            .ReturnsAsync(submission);

        _mapperMock
            .Setup(x => x.Map<EssaySubmissionDto>(It.IsAny<EssaySubmission>()))
            .Returns(submissionDto);

        _essaySubmissionRepositoryMock
            .Setup(x => x.GetUserSubmissionForEssayAsync(userId, dto.EssayId))
            .ReturnsAsync((EssaySubmission?)null);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ServiceResponse<string> { Success = true, Data = "real/file-key" });

        _essaySubmissionRepositoryMock
            .Setup(x => x.CreateSubmissionAsync(It.IsAny<EssaySubmission>()))
            .ReturnsAsync(submission);

        _mapperMock
            .Setup(x => x.Map<EssaySubmissionDto>(It.IsAny<EssaySubmission>()))
            .Returns(submissionDto);

        // Act
        var result = await _essaySubmissionService.CreateSubmissionAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.TextContent, result.Data.TextContent);

        _essaySubmissionRepositoryMock.Verify(x => x.CreateSubmissionAsync(It.IsAny<EssaySubmission>()), Times.Once);
    }

    [Fact]
    public async Task SubmitEssayAsync_WithNonExistentEssay_ReturnsNotFound()
    {
        // Arrange
        var dto = new CreateEssaySubmissionDto
        {
            EssayId = 999,
            TextContent = "My essay answer"
        };

        var userId = 1;

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(dto.EssayId))
            .ReturnsAsync((Essay?)null);

        // Act
        var result = await _essaySubmissionService.CreateSubmissionAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Essay không tồn tại", result.Message);

        _essaySubmissionRepositoryMock.Verify(x => x.CreateSubmissionAsync(It.IsAny<EssaySubmission>()), Times.Never);
    }

    #endregion
}

