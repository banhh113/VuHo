FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CLB_Vo_Website_Skeleton/CLBVo.csproj ./CLB_Vo_Website_Skeleton/
RUN dotnet restore CLB_Vo_Website_Skeleton/CLBVo.csproj

COPY CLB_Vo_Website_Skeleton/ ./CLB_Vo_Website_Skeleton/
WORKDIR /src/CLB_Vo_Website_Skeleton
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=10000
ENTRYPOINT ["dotnet", "CLBVo.dll"]