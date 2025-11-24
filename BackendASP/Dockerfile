## Dockerfile for Learning English API


# STAGE 1: RESTORE (SDK)
# ========================
# docker lấy image dotnet 8.0 SDK

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
# đặt thư mục làm việc là /src
WORKDIR /src



# Copy solution + csproj để tận dụng cache restore
# Sao chép các file dự án và solution vào trong container
COPY LearningEnglish.sln ./
COPY LearningEnglish.API/LearningEnglish.API.csproj ./LearningEnglish.API/
COPY LearningEnglish.Application/LearningEnglish.Application.csproj ./LearningEnglish.Application/
COPY LearningEnglish.Domain/LearningEnglish.Domain.csproj ./LearningEnglish.Domain/
COPY LearningEnglish.Infrastructure/LearningEnglish.Infrastructure.csproj ./LearningEnglish.Infrastructure/

# Restore NuGet packages 
# chạy lệnh restore để tải về các package cần thiết, tải nuget packages
RUN dotnet restore LearningEnglish.sln


# ========================
# STAGE 2: BUILD + PUBLISH (SDK)
# ========================
# docker lấy image dotnet 8.0 SDK 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# đặt thư mục làm việc là /src
WORKDIR /src

# Copy từ stage restore sang (để reuse cache)
COPY --from=restore /src .

# Copy toàn bộ source code 
COPY . .

# Publish riêng project API
#  chạy lệnh publish để biên dịch và xuất bản ứng dụng
RUN dotnet publish LearningEnglish.API/LearningEnglish.API.csproj \
    -c Release \
    -o /app/publish
# sau bước này thì code sẽ compile xong và nằm trong /app/publish  và thành các file dll , appsettings.json,



# STAGE 3: RUNTIME (ASP.NET)

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy file đã publish từ stage build
COPY --from=build /app/publish .

# Expose port trong container
EXPOSE 8080

# Cho ASP.NET listen trên mọi interface trong container, port 8080
ENV ASPNETCORE_URLS=http://+:8080

# Môi trường mặc định
ENV ASPNETCORE_ENVIRONMENT=Production

# Lệnh chạy app
ENTRYPOINT ["dotnet", "LearningEnglish.API.dll"]
