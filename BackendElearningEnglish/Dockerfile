# Build stage  1 
# Use the official .NET SDK image to build the application

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build  

WORKDIR /backend

# Copy .sln 
COPY *.sln ./
# Copy các project csproj vào thư mục tương ứng trong container
COPY LearningEnglish.API/*.csproj ./LearningEnglish.API/
COPY LearningEnglish.Application/*.csproj ./LearningEnglish.Application/
COPY LearningEnglish.Domain/*.csproj ./LearningEnglish.Domain/
COPY LearningEnglish.Infrastructure/*.csproj ./LearningEnglish.Infrastructure/

# và Tải Nuget packages cho project dotnet  vd : tải auto mapper , entity framework ... 
RUN dotnet restore 


# Copy course code vào thư mục backend/pulish của container
COPY . .
RUN dotnet publish LearningEnglish.API/LearningEnglish.API.csproj -c Release -o /backend/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# Set the working directory in the container
WORKDIR /backend
# Copy the published output from the build stage to the runtime stage
COPY --from=build /backend/publish .
# Expose port 8080 for the application

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "LearningEnglish.API.dll"]
