using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Application.Service.PaymentService;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.PaymentService;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IPaymentValidator> _paymentValidatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<LearningEnglish.Application.Service.PaymentService.PaymentService>> _loggerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPayOSService> _payOSServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly List<Mock<IPaymentStrategy>> _strategyMocks;
    private readonly LearningEnglish.Application.Service.PaymentService.PaymentService _service;

    public PaymentServiceTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _paymentValidatorMock = new Mock<IPaymentValidator>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<LearningEnglish.Application.Service.PaymentService.PaymentService>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _payOSServiceMock = new Mock<IPayOSService>();
        _configurationMock = new Mock<IConfiguration>();

        // Setup Strategy Pattern
        var courseStrategy = new Mock<IPaymentStrategy>();
        courseStrategy.Setup(s => s.ProductType).Returns(ProductType.Course);
        courseStrategy.Setup(s => s.GetProductNameAsync(It.IsAny<int>())).ReturnsAsync("Course A");
        courseStrategy.Setup(s => s.ProcessPostPaymentAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true });

        _strategyMocks = new List<Mock<IPaymentStrategy>> { courseStrategy };
        
        _service = new LearningEnglish.Application.Service.PaymentService.PaymentService(
            _paymentRepositoryMock.Object,
            _paymentValidatorMock.Object,
            _strategyMocks.Select(m => m.Object),
            _mapperMock.Object,
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _payOSServiceMock.Object,
            _configurationMock.Object
        );
    }

    [Fact]
    public async Task ProcessPaymentAsync_IdempotencyKeyExists_ReturnsExistingPayment()
    {
        // Arrange
        var userId = 1;
        var request = new requestPayment { IdempotencyKey = "key123", ProductId = 1, typeproduct = ProductType.Course };
        var existingPayment = new Payment { PaymentId = 99, IdempotencyKey = "key123", Amount = 1000 };

        _paymentRepositoryMock.Setup(r => r.GetPaymentByIdempotencyKeyAsync(userId, "key123"))
            .ReturnsAsync(existingPayment);

        // Act
        var result = await _service.ProcessPaymentAsync(userId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(99, result.Data.PaymentId);
        Assert.Equal("Payment đã được tạo trước đó (idempotent request)", result.Message);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ValidationFailed_ReturnsBadRequest()
    {
        // Arrange
        var request = new requestPayment { ProductId = 1, typeproduct = ProductType.Course };
        _paymentValidatorMock.Setup(v => v.ValidateUserPaymentAsync(It.IsAny<int>(), 1, ProductType.Course))
            .ReturnsAsync(new ServiceResponse<bool> { Success = false, Message = "Already owned" });

        // Act
        var result = await _service.ProcessPaymentAsync(1, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Already owned", result.Message);
    }

    [Fact]
    public async Task ProcessPaymentAsync_FreeProduct_AutoConfirms()
    {
        // Arrange
        var userId = 1;
        var request = new requestPayment { ProductId = 1, typeproduct = ProductType.Course };
        
        _paymentValidatorMock.Setup(v => v.ValidateUserPaymentAsync(userId, 1, ProductType.Course))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true });
        
        _paymentValidatorMock.Setup(v => v.ValidateProductAsync(1, ProductType.Course))
            .ReturnsAsync(new ServiceResponse<decimal> { Success = true, Data = 0 }); // Free

        // Act
        var result = await _service.ProcessPaymentAsync(userId, request);

        // Assert
        Assert.True(result.Success);
        _paymentRepositoryMock.Verify(r => r.AddPaymentAsync(It.Is<Payment>(p => p.Status == PaymentStatus.Completed && p.Amount == 0)), Times.Once);
        // Verify strategy was called to enroll user
        _strategyMocks.First().Verify(s => s.ProcessPostPaymentAsync(userId, 1, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_Success_CompletesPaymentAndCallsStrategy()
    {
        // Arrange
        var userId = 1;
        var paymentId = 100;
        var payment = new Payment 
        { 
            PaymentId = paymentId, 
            UserId = userId, 
            Status = PaymentStatus.Pending, 
            Amount = 50000, 
            ProductId = 1, 
            ProductType = ProductType.Course 
        };
        var confirmDto = new CompletePayment 
        { 
            PaymentId = paymentId, 
            Amount = 50000, 
            ProductId = 1, 
            ProductType = ProductType.Course 
        };

        _paymentRepositoryMock.Setup(r => r.GetPaymentByIdAsync(paymentId)).ReturnsAsync(payment);

        // Act
        var result = await _service.ConfirmPaymentAsync(confirmDto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        
        // Verify strategy was called
        _strategyMocks.First().Verify(s => s.ProcessPostPaymentAsync(userId, 1, paymentId), Times.Once);
        
        // Verify transaction commit
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_AmountMismatch_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var payment = new Payment { PaymentId = 100, UserId = userId, Status = PaymentStatus.Pending, Amount = 50000 };
        var confirmDto = new CompletePayment { PaymentId = 100, Amount = 10000 }; // Mismatch

        _paymentRepositoryMock.Setup(r => r.GetPaymentByIdAsync(100)).ReturnsAsync(payment);

        // Act
        var result = await _service.ConfirmPaymentAsync(confirmDto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Số tiền thanh toán không khớp", result.Message);
    }
}
