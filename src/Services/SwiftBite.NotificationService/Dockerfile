FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/SwiftBite.NotificationService/SwiftBite.NotificationService.API/SwiftBite.NotificationService.API.csproj", "src/Services/SwiftBite.NotificationService/SwiftBite.NotificationService.API/"]
COPY ["src/Services/SwiftBite.NotificationService/SwiftBite.NotificationService.Application/SwiftBite.NotificationService.Application.csproj", "src/Services/SwiftBite.NotificationService/SwiftBite.NotificationService.Application/"]
COPY ["src/Services/SwiftBite.NotificationService/SwiftBite.NotificationService.Domain/SwiftBite.NotificationService.Domain.csproj", "src/Services/SwiftBite.NotificationService/SwiftBite.NotificationService.Domain/"]
COPY ["src/Services/SwiftBite.NotificationService/SwiftBite.NotificationService.Infrastructure/SwiftBite.NotificationService.Infrastructure.csproj", "src/Services/SwiftBite.NotificationService/SwiftBite.NotificationService.Infrastructure/"]
COPY ["SwiftBite.Shared.Kernel/SwiftBite.Shared.Kernel.csproj", "SwiftBite.Shared.Kernel/"]
COPY ["SwiftBite.Shared.Exceptions/SwiftBite.Shared.Exceptions.csproj", "SwiftBite.Shared.Exceptions/"]

RUN dotnet restore "src/Services/SwiftBite.NotificationService/SwiftBite.NotificationService.API/SwiftBite.NotificationService.API.csproj"

COPY . .

WORKDIR "/src/src/Services/SwiftBite.NotificationService/SwiftBite.NotificationService.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SwiftBite.NotificationService.API.dll"]