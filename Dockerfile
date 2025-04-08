# ---------- Base stage: ASP.NET runtime ----------
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
    WORKDIR /app
    EXPOSE 80
    
    # ---------- Build stage: SDK med kildetekst ----------
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    
    # Kopiér kun projektfil først for bedre caching
    COPY ApiGateway.Yarp.csproj ./
    RUN dotnet restore ApiGateway.Yarp.csproj
    
    # Kopiér resten af koden og byg
    COPY . .
    RUN dotnet publish ApiGateway.Yarp.csproj -c Release -o /app/publish
    
    # ---------- Final stage: Kørsel med kun runtime ----------
    FROM base AS final
    WORKDIR /app
    COPY --from=build /app/publish .
    
    # Start applikationen
    ENTRYPOINT ["dotnet", "ApiGateway.Yarp.dll"]
    