FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/ApiGateway/SwiftBite.ApiGateway/SwiftBite.ApiGateway.csproj", "src/ApiGateway/SwiftBite.ApiGateway/"]
COPY ["SwiftBite.Shared.Kernel/SwiftBite.Shared.Kernel.csproj", "SwiftBite.Shared.Kernel/"]
COPY ["SwiftBite.Shared.Exceptions/SwiftBite.Shared.Exceptions.csproj", "SwiftBite.Shared.Exceptions/"]

RUN dotnet restore "src/ApiGateway/SwiftBite.ApiGateway/SwiftBite.ApiGateway.csproj"

COPY . .

WORKDIR "/src/src/ApiGateway/SwiftBite.ApiGateway"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SwiftBite.ApiGateway.dll"]