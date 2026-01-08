using System.ComponentModel;
using System.Reflection;
using LearningEnglish.Application.DTOS.Common;

namespace LearningEnglish.Application.Common.Helpers
{
    public static class EnumHelper
    {
        public static List<EnumMappingDto> GetEnumMappings<T>() where T : Enum
        {
            var enumType = typeof(T);
            return GetEnumMappings(enumType);
        }

        public static List<EnumMappingDto> GetEnumMappings(Type enumType)
        {
            var list = new List<EnumMappingDto>();
            
            if (!enumType.IsEnum) return list;

            foreach (var value in Enum.GetValues(enumType))
            {
                var name = Enum.GetName(enumType, value) ?? "";
                var field = enumType.GetField(name);
                var displayName = name;

                // Lấy Description attribute nếu có
                var descriptionAttribute = field?.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    displayName = descriptionAttribute.Description;
                }

                list.Add(new EnumMappingDto
                {
                    Value = (int)value,
                    Name = name,
                    DisplayName = displayName
                });
            }

            return list;
        }
    }
}
