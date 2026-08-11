FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/SwiftBite.UserService/SwiftBite.UserService.API/SwiftBite.UserService.API.csproj", "src/Services/SwiftBite.UserService/SwiftBite.UserService.API/"]
COPY ["src/Services/SwiftBite.UserService/SwiftBite.UserService.Application/SwiftBite.UserService.Application.csproj", "src/Services/SwiftBite.UserService/SwiftBite.UserService.Application/"]
COPY ["src/Services/SwiftBite.UserService/SwiftBite.UserService.Domain/SwiftBite.UserService.Domain.csproj", "src/Services/SwiftBite.UserService/SwiftBite.UserService.Domain/"]
COPY ["src/Services/SwiftBite.UserService/SwiftBite.UserService.Infrastructure/SwiftBite.UserService.Infrastructure.csproj", "src/Services/SwiftBite.UserService/SwiftBite.UserService.Infrastructure/"]
COPY ["SwiftBite.Shared.Kernel/SwiftBite.Shared.Kernel.csproj", "SwiftBite.Shared.Kernel/"]
COPY ["SwiftBite.Shared.Exceptions/SwiftBite.Shared.Exceptions.csproj", "SwiftBite.Shared.Exceptions/"]

RUN dotnet restore "src/Services/SwiftBite.UserService/SwiftBite.UserService.API/SwiftBite.UserService.API.csproj"

COPY . .

WORKDIR "/src/src/Services/SwiftBite.UserService/SwiftBite.UserService.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SwiftBite.UserService.API.dll"]