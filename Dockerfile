# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0.101 AS build
WORKDIR /src

# Copy project files
COPY src/MedicalCenter.Core/MedicalCenter.Core.csproj src/MedicalCenter.Core/
COPY src/MedicalCenter.Infrastructure/MedicalCenter.Infrastructure.csproj src/MedicalCenter.Infrastructure/
COPY src/MedicalCenter.WebApi/MedicalCenter.WebApi.csproj src/MedicalCenter.WebApi/

# Restore dependencies
RUN dotnet restore src/MedicalCenter.WebApi/MedicalCenter.WebApi.csproj

# Copy all source files
COPY src/MedicalCenter.Core/ src/MedicalCenter.Core/
COPY src/MedicalCenter.Infrastructure/ src/MedicalCenter.Infrastructure/
COPY src/MedicalCenter.WebApi/ src/MedicalCenter.WebApi/

# Build the application
WORKDIR /src/src/MedicalCenter.WebApi
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "MedicalCenter.WebApi.dll"]

