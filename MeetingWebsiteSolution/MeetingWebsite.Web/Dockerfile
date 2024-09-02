FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MeetingWebsite.Web/MeetingWebsite.Web.csproj", "MeetingWebsite.Web/"]
COPY ["MeetingWebsite.Infrastracture/MeetingWebsite.Infrastracture.csproj", "MeetingWebsite.Infrastracture/"]
COPY ["MeetingWebsite.Application/MeetingWebsite.Application.csproj", "MeetingWebsite.Application/"]
COPY ["MeetingWebsite.Domain/MeetingWebsite.Domain.csproj", "MeetingWebsite.Domain/"]
RUN dotnet restore "./MeetingWebsite.Web/MeetingWebsite.Web.csproj"
COPY . .
WORKDIR "/src/MeetingWebsite.Web"
RUN dotnet build "./MeetingWebsite.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MeetingWebsite.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MeetingWebsite.Web.dll"]
#ENTRYPOINT ["/bin/sh", "-c", "mkdir -p /appcopy && cp -r /app/* /appcopy && dotnet MeetingWebsite.Web.dll"]