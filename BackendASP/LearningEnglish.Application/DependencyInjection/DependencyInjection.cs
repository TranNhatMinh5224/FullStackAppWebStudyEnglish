using FluentValidation;
using FluentValidation.AspNetCore;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Service;
using LearningEnglish.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace LearningEnglish.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Services
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<ILectureService, LectureService>();
            services.AddScoped<IFlashCardService, FlashCardService>();

            // AutoMapper is configured in Program.cs

            // FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<CreateFlashCardDtoValidator>();

            return services;
        }
    }
}
