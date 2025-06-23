# Stage 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project file
COPY EduConnectBE.sln .
COPY EduConnectBE/EduConnect.Presentation/EduConnect.Presentation.csproj EduConnectBE/EduConnect.Presentation/

# Restore dependencies
RUN dotnet restore EduConnectBE.sln

# Copy the rest of the source code
COPY . .

# Publish app
WORKDIR /src/EduConnectBE/EduConnect.Presentation
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 7127
EXPOSE 7126

ENTRYPOINT ["dotnet", "EduConnect.Presentation.dll"]
