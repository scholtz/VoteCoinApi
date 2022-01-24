#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VoteCoinApi/VoteCoinApi.csproj", "VoteCoinApi/"]
RUN dotnet restore "VoteCoinApi/VoteCoinApi.csproj"
COPY . .
WORKDIR "/src/VoteCoinApi"
RUN dotnet build "VoteCoinApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VoteCoinApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY asa-list asa-list
RUN dotnet dev-certs https --trust
ENTRYPOINT ["dotnet", "VoteCoinApi.dll"]