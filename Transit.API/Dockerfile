FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5299

ENV ASPNETCORE_URLS=http://+:5299

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Transit.API/Transit.API.csproj", "Transit.API/"]
RUN dotnet restore "Transit.API/Transit.API.csproj"
COPY . .
WORKDIR "/src/Transit.API"
RUN dotnet build "Transit.API.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Transit.API.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Transit.API.dll"]
