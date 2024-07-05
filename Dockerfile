FROM mcr.microsoft.com/dotnet/aspnet:8.0 as base
ENV DOTNET_EnableWriteXorExecute=0
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION = false
WORKDIR /app
EXPOSE 8001
EXPOSE 8000

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY ["BirthdayNotificationsBot/BirthdayNotificationsBot.csproj", "BirthdayNotificationsBot/"]
RUN dotnet restore "BirthdayNotificationsBot/BirthdayNotificationsBot.csproj"
COPY . .
WORKDIR "/source/BirthdayNotificationsBot"
RUN dotnet build "BirthdayNotificationsBot.csproj" -c Release -o /app/build

WORKDIR "/source/BirthdayNotificationsBot"
FROM build as publish
RUN dotnet publish "BirthdayNotificationsBot.csproj" -a linux/amd64 -c Release -o /app/publish /p:UseAppHost=false

FROM base as final
WORKDIR /app
COPY BirthdayNotificationsBot/Logging/Logs /app/Logging/Logs/
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BirthdayNotificationsBot.dll"]