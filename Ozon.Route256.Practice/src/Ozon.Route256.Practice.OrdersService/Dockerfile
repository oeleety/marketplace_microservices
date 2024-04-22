#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR Ozon.Route256.Practice.OrdersService/src
COPY ["Ozon.Route256.Practice.OrdersService/Ozon.Route256.Practice.OrdersService.csproj", "Ozon.Route256.Practice.OrdersService/src/Ozon.Route256.Practice.OrdersService/"]
COPY ["Ozon.Route256.Practice.Shared/Ozon.Route256.Practice.Shared.csproj", "Ozon.Route256.Practice.Shared/src/Ozon.Route256.Practice.Shared/"]
COPY ["Ozon.Route256.Practice.OrdersService.Application/Ozon.Route256.Practice.OrdersService.Application.csproj", "Ozon.Route256.Practice.OrdersService.Application/src/Ozon.Route256.Practice.OrdersService.Application/"]
COPY ["Ozon.Route256.Practice.OrdersService.Domain/Ozon.Route256.Practice.OrdersService.Domain.csproj", "Ozon.Route256.Practice.OrdersService.Domain/src/Ozon.Route256.Practice.OrdersService.Domain/"]
COPY ["Ozon.Route256.Practice.OrdersService.Infrastructure/Ozon.Route256.Practice.OrdersService.Infrastructure.csproj", "Ozon.Route256.Practice.OrdersService.Infrastructure/src/Ozon.Route256.Practice.OrdersService.Infrastructure/"]
RUN dotnet restore "Ozon.Route256.Practice.OrdersService/src/Ozon.Route256.Practice.OrdersService/./Ozon.Route256.Practice.OrdersService.csproj"
WORKDIR "Ozon.Route256.Practice.OrdersService/src/Ozon.Route256.Practice.OrdersService"
COPY . .
RUN dotnet build "./Ozon.Route256.Practice.OrdersService/Ozon.Route256.Practice.OrdersService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ozon.Route256.Practice.OrdersService/Ozon.Route256.Practice.OrdersService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ozon.Route256.Practice.OrdersService.dll"]