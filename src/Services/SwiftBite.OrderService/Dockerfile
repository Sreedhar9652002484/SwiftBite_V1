FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/SwiftBite.OrderService/SwiftBite.OrderService.API/SwiftBite.OrderService.API.csproj", "src/Services/SwiftBite.OrderService/SwiftBite.OrderService.API/"]
COPY ["src/Services/SwiftBite.OrderService/SwiftBite.OrderService.Application/SwiftBite.OrderService.Application.csproj", "src/Services/SwiftBite.OrderService/SwiftBite.OrderService.Application/"]
COPY ["src/Services/SwiftBite.OrderService/SwiftBite.OrderService.Domain/SwiftBite.OrderService.Domain.csproj", "src/Services/SwiftBite.OrderService/SwiftBite.OrderService.Domain/"]
COPY ["src/Services/SwiftBite.OrderService/SwiftBite.OrderService.Infrastructure/SwiftBite.OrderService.Infrastructure.csproj", "src/Services/SwiftBite.OrderService/SwiftBite.OrderService.Infrastructure/"]
COPY ["SwiftBite.Shared.Kernel/SwiftBite.Shared.Kernel.csproj", "SwiftBite.Shared.Kernel/"]
COPY ["SwiftBite.Shared.Exceptions/SwiftBite.Shared.Exceptions.csproj", "SwiftBite.Shared.Exceptions/"]

RUN dotnet restore "src/Services/SwiftBite.OrderService/SwiftBite.OrderService.API/SwiftBite.OrderService.API.csproj"

COPY . .

WORKDIR "/src/src/Services/SwiftBite.OrderService/SwiftBite.OrderService.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SwiftBite.OrderService.API.dll"]