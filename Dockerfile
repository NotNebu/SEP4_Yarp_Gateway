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
    
    # ---------- Final stage ----------
    FROM base AS final
    WORKDIR /app

    # Kopiér build-output
    COPY --from=build /app/publish .

    # Kopiér HTTPS-certifikat
    COPY certs/localhost-user-service.p12 /https/localhost-user-service.p12
    COPY certs/rootCA.crt /usr/local/share/ca-certificates/rootCA.crt
    RUN update-ca-certificates

    # Fortæl ASP.NET at bruge HTTPS med certifikat
    ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/localhost-user-service.p12"
    ENV ASPNETCORE_Kestrel__Certificates__Default__Password="changeit"

    ENTRYPOINT ["dotnet", "ApiGateway.Yarp.dll"]
