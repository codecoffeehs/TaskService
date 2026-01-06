# -------------------------
# Stage 1: Build
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set working directory
WORKDIR /src

# Copy project file and restore dependencies
COPY TaskService.csproj ./
RUN dotnet restore TaskService.csproj

# Copy all source files
COPY . .

# Publish using default output location
RUN dotnet publish TaskService.csproj -c Release --no-restore

# -------------------------
# Stage 2: Runtime
# -------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# Copy published files from build stage
COPY --from=build /src/bin/Release/net9.0/publish .

# Expose port
EXPOSE 5006

ENTRYPOINT ["dotnet", "TaskService.dll"]