FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MapReduce.GRPC.CSharp.ShuffleWorker/MapReduce.GRPC.CSharp.ShuffleWorker.csproj", "MapReduce.GRPC.CSharp.ShuffleWorker/"]
COPY ["MapReduce.GRPC.CSharp.Common/MapReduce.GRPC.CSharp.Common.csproj", "MapReduce.GRPC.CSharp.Common/"]
RUN dotnet restore "./MapReduce.GRPC.CSharp.ShuffleWorker/MapReduce.GRPC.CSharp.ShuffleWorker.csproj"
COPY . .
WORKDIR "/src/MapReduce.GRPC.CSharp.ShuffleWorker"
RUN dotnet build "./MapReduce.GRPC.CSharp.ShuffleWorker.csproj" -c %BUILD_CONFIGURATION% -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MapReduce.GRPC.CSharp.ShuffleWorker.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MapReduce.GRPC.CSharp.ShuffleWorker.dll"]