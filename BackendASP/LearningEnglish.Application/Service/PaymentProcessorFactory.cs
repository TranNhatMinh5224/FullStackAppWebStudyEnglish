using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Service.PaymentProcessors;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace LearningEnglish.Application.Service
{
    public class PaymentProcessorFactory : IPaymentProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentProcessor GetProcessor(TypeProduct productType)
        {
            return productType switch
            {
                TypeProduct.Course => _serviceProvider.GetRequiredService<CoursePaymentProcessor>(),
                TypeProduct.TeacherPackage => _serviceProvider.GetRequiredService<TeacherPackagePaymentProcessor>(),
                _ => throw new NotSupportedException($"Product type {productType} is not supported")
            };
        }
    }
}
