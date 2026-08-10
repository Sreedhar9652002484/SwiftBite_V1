FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/SwiftBite.RestaurantService/SwiftBite.RestaurantService.API/SwiftBite.RestaurantService.API.csproj", "src/Services/SwiftBite.RestaurantService/SwiftBite.RestaurantService.API/"]
COPY ["src/Services/SwiftBite.RestaurantService/SwiftBite.RestaurantService.Application/SwiftBite.RestaurantService.Application.csproj", "src/Services/SwiftBite.RestaurantService/SwiftBite.RestaurantService.Application/"]
COPY ["src/Services/SwiftBite.RestaurantService/SwiftBite.RestaurantService.Domain/SwiftBite.RestaurantService.Domain.csproj", "src/Services/SwiftBite.RestaurantService/SwiftBite.RestaurantService.Domain/"]
COPY ["src/Services/SwiftBite.RestaurantService/SwiftBite.RestaurantService.Infrastructure/SwiftBite.RestaurantService.Infrastructure.csproj", "src/Services/SwiftBite.RestaurantService/SwiftBite.RestaurantService.Infrastructure/"]
COPY ["SwiftBite.Shared.Kernel/SwiftBite.Shared.Kernel.csproj", "SwiftBite.Shared.Kernel/"]
COPY ["SwiftBite.Shared.Exceptions/SwiftBite.Shared.Exceptions.csproj", "SwiftBite.Shared.Exceptions/"]

RUN dotnet restore "src/Services/SwiftBite.RestaurantService/SwiftBite.RestaurantService.API/SwiftBite.RestaurantService.API.csproj"

COPY . .

WORKDIR "/src/src/Services/SwiftBite.RestaurantService/SwiftBite.RestaurantService.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SwiftBite.RestaurantService.API.dll"]