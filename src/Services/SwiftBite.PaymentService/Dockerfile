FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/SwiftBite.PaymentService/SwiftBite.PaymentService.API/SwiftBite.PaymentService.API.csproj", "src/Services/SwiftBite.PaymentService/SwiftBite.PaymentService.API/"]
COPY ["src/Services/SwiftBite.PaymentService/SwiftBite.PaymentService.Application/SwiftBite.PaymentService.Application.csproj", "src/Services/SwiftBite.PaymentService/SwiftBite.PaymentService.Application/"]
COPY ["src/Services/SwiftBite.PaymentService/SwiftBite.PaymentService.Domain/SwiftBite.PaymentService.Domain.csproj", "src/Services/SwiftBite.PaymentService/SwiftBite.PaymentService.Domain/"]
COPY ["src/Services/SwiftBite.PaymentService/SwiftBite.PaymentService.Infrastructure/SwiftBite.PaymentService.Infrastructure.csproj", "src/Services/SwiftBite.PaymentService/SwiftBite.PaymentService.Infrastructure/"]
COPY ["SwiftBite.Shared.Kernel/SwiftBite.Shared.Kernel.csproj", "SwiftBite.Shared.Kernel/"]
COPY ["SwiftBite.Shared.Exceptions/SwiftBite.Shared.Exceptions.csproj", "SwiftBite.Shared.Exceptions/"]

RUN dotnet restore "src/Services/SwiftBite.PaymentService/SwiftBite.PaymentService.API/SwiftBite.PaymentService.API.csproj"

COPY . .

WORKDIR "/src/src/Services/SwiftBite.PaymentService/SwiftBite.PaymentService.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SwiftBite.PaymentService.API.dll"]