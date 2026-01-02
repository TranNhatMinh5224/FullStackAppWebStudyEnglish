# Build stage  1 
# Use the official .NET SDK image to build the application

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build  

WORKDIR /backend

# Copy .sln , project files  
COPY *.sln ./
COPY LearningEnglish.API/*.csproj ./LearningEnglish.API/
COPY LearningEnglish.Application/*.csproj ./LearningEnglish.Application/
COPY LearningEnglish.Domain/*.csproj ./LearningEnglish.Domain/
COPY LearningEnglish.Infrastructure/*.csproj ./LearningEnglish.Infrastructure/
COPY LearningEnglish.Tests/*.csproj ./LearningEnglish.Tests/
# và Tải Nuget packages cho project dotnet  vd : tải auto mapper , entity framework ... 
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
