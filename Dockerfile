# ----- Build Stage -----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build

# Copy project files for restore
COPY ["src/MechanicShop.Api/MechanicShop.Api.csproj", "src/MechanicShop.Api/"]
COPY ["src/MechanicShop.Client/MechanicShop.Client.csproj", "src/MechanicShop.Client/"]
COPY ["src/MechanicShop.Application/MechanicShop.Application.csproj", "src/MechanicShop.Application/"]
COPY ["src/MechanicShop.Domain/MechanicShop.Domain.csproj", "src/MechanicShop.Domain/"]
COPY ["src/MechanicShop.Contracts/MechanicShop.Contracts.csproj", "src/MechanicShop.Contracts/"]
COPY ["src/MechanicShop.Infrastructure/MechanicShop.Infrastructure.csproj", "src/MechanicShop.Infrastructure/"]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]

# Restore dependencies (only once)
RUN dotnet restore "src/MechanicShop.Api/MechanicShop.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish "src/MechanicShop.Api/MechanicShop.Api.csproj" -c Release -o /app

# ----- Final Stage -----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Install timezone data for TimeZoneInfo support
RUN apt-get update && apt-get install -y tzdata && \
    ln -fs /usr/share/zoneinfo/America/Montreal /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    rm -rf /var/lib/apt/lists/*

ENV TZ=America/Montreal

WORKDIR /app
COPY --from=build /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "MechanicShop.Api.dll"]