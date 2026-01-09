using AutoMapper;
using LearningEnglish.Application.DTOS.Common;
using System.ComponentModel;
using System.Reflection;

namespace LearningEnglish.Application.Mappings.Converters
{
    // Converter Generic xử lý mọi loại Enum
    public class EnumTypeConverter<TEnum> : ITypeConverter<TEnum, EnumMappingDto> where TEnum : Enum
    {
        public EnumMappingDto Convert(TEnum source, EnumMappingDto destination, ResolutionContext context)
        {
            var type = source.GetType();
            var name = Enum.GetName(type, source) ?? source.ToString();
            var field = type.GetField(name);
            var displayName = name;

            if (field != null)
            {
                var attr = field.GetCustomAttribute<DescriptionAttribute>();
                if (attr != null)
                {
                    displayName = attr.Description;
                }
            }

            return new EnumMappingDto
            {
                Value = ConvertToInt(source),
                Name = name,
                DisplayName = displayName
            };
        }

        private int ConvertToInt(TEnum source)
        {
             return (int)(object)source;
        }
    }
}
