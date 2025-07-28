FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

WORKDIR /app
EXPOSE 5075

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MapReduce.GRPC.CSharp.Master/MapReduce.GRPC.CSharp.Master.csproj", "MapReduce.GRPC.CSharp.Master/"]
COPY ["MapReduce.GRPC.CSharp.Common/MapReduce.GRPC.CSharp.Common.csproj", "MapReduce.GRPC.CSharp.Common/"]
RUN dotnet restore "./MapReduce.GRPC.CSharp.Master/MapReduce.GRPC.CSharp.Master.csproj"
COPY . .
WORKDIR "/src/MapReduce.GRPC.CSharp.Master"
RUN dotnet build "./MapReduce.GRPC.CSharp.Master.csproj" -c %BUILD_CONFIGURATION% -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MapReduce.GRPC.CSharp.Master.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# copy the sample DOCX into /app
COPY MapReduce.GRPC.CSharp.Master/alice-in-wonderland.docx .
ENTRYPOINT ["dotnet", "MapReduce.GRPC.CSharp.Master.dll"]