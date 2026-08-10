FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/AuthServer/SwiftBite.AuthServer/SwiftBite.AuthServer.csproj", "src/AuthServer/SwiftBite.AuthServer/"]
COPY ["SwiftBite.Shared.Kernel/SwiftBite.Shared.Kernel.csproj", "SwiftBite.Shared.Kernel/"]
COPY ["SwiftBite.Shared.Exceptions/SwiftBite.Shared.Exceptions.csproj", "SwiftBite.Shared.Exceptions/"]

RUN dotnet restore "src/AuthServer/SwiftBite.AuthServer/SwiftBite.AuthServer.csproj"

COPY . .

WORKDIR "/src/src/AuthServer/SwiftBite.AuthServer"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SwiftBite.AuthServer.dll"]