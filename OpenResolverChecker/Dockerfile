FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["OpenResolverChecker/OpenResolverChecker.csproj", "OpenResolverChecker/"]
RUN dotnet restore "OpenResolverChecker/OpenResolverChecker.csproj"
COPY . .
WORKDIR "/src/OpenResolverChecker"
RUN dotnet build "OpenResolverChecker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenResolverChecker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenResolverChecker.dll"]
