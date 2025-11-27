# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore
COPY *.sln ./
COPY Transit.API/*.csproj ./Transit.API/
COPY Transit.Application/*.csproj ./Transit.Application/
COPY Transit.Domain/*.csproj ./Transit.Domain/

RUN dotnet restore

# Copy everything else
COPY . ./

# Build
RUN dotnet publish Transit.API/Transit.API.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Transit.API.dll"]
