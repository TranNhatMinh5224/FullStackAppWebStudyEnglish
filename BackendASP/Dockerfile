# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src 

# Copy csproj files and restore
COPY *.sln ./
COPY LearningEnglish.API/*.csproj ./LearningEnglish.API/
COPY LearningEnglish.Application/*.csproj ./LearningEnglish.Application/
COPY LearningEnglish.Domain/*.csproj ./LearningEnglish.Domain/
COPY LearningEnglish.Infrastructure/*.csproj ./LearningEnglish.Infrastructure/
COPY LearningEnglish.Tests/*.csproj ./LearningEnglish.Tests/
RUN dotnet restore

# Copy everything and build
COPY . .
RUN dotnet publish LearningEnglish.API/LearningEnglish.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "LearningEnglish.API.dll"]
