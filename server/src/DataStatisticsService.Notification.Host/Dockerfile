FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/DataStatisticsService.Notification.Host/DataStatisticsService.Notification.Host.csproj", "src/DataStatisticsService.Notification.Host/"]
COPY ["src/DataStatisticsService.Data/DataStatisticsService.Data.csproj", "src/DataStatisticsService.Data/"]
COPY ["src/DataStatisticsService.Abstractions/DataStatisticsService.Abstractions.csproj", "src/DataStatisticsService.Abstractions/"]
COPY ["src/DataStatisticsService.Shared/DataStatisticsService.Shared.csproj", "src/DataStatisticsService.Shared/"]
RUN dotnet restore "src/DataStatisticsService.Notification.Host/DataStatisticsService.Notification.Host.csproj"
COPY . .
WORKDIR "/src/src/DataStatisticsService.Notification.Host"
RUN dotnet publish "DataStatisticsService.Notification.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DataStatisticsService.Notification.Host.dll"]
