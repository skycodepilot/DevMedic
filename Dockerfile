# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Update the path to include the folder name
# We copy the csproj to a matching folder structure inside the image
COPY ["DevMedic.Api/DevMedic.Api.csproj", "DevMedic.Api/"]

# 2. Restore using that specific path
RUN dotnet restore "DevMedic.Api/DevMedic.Api.csproj"

# 3. Copy EVERYTHING from the root (respecting .dockerignore)
COPY . .

# 4. Change directory into the specific project folder to build it
WORKDIR "/src/DevMedic.Api"

RUN dotnet publish "DevMedic.Api.csproj" -c Release -o /app/publish

# Stage 2: Run the application (No changes needed here usually)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DevMedic.Api.dll"]