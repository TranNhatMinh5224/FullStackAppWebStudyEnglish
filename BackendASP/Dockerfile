# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
## Copy file vào image
COPY CleanArchDemo.sln ./

# Copy project files 
COPY LearningEnglish.API/LearningEnglish.API.csproj LearningEnglish.API/
COPY LearningEnglish.Application/LearningEnglish.Application.csproj LearningEnglish.Application/
COPY LearningEnglish.Domain/LearningEnglish.Domain.csproj LearningEnglish.Domain/
COPY LearningEnglish.Infrastructure/LearningEnglish.Infrastructure.csproj LearningEnglish.Infrastructure/

# Restore dependencies -- tải toàn bộ NuGet packages 
RUN dotnet restore CleanArchDemo.sln

# Copy tất cả source code
COPY . .

# Build solution
RUN dotnet build CleanArchDemo.sln -c Release --no-restore

# Stage 2: Publish
FROM build AS publish
WORKDIR /src/LearningEnglish.API
RUN dotnet publish -c Release -o /app/publish --no-build

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "LearningEnglish.API.dll"]