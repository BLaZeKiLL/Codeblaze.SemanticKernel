FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Codeblaze.SemanticKernel.Console/Codeblaze.SemanticKernel.Console.csproj", "Codeblaze.SemanticKernel.Console/"]
RUN dotnet restore "Codeblaze.SemanticKernel.Console/Codeblaze.SemanticKernel.Console.csproj"
COPY . .
WORKDIR "/src/Codeblaze.SemanticKernel.Console"
RUN dotnet build "Codeblaze.SemanticKernel.Console.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Codeblaze.SemanticKernel.Console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Codeblaze.SemanticKernel.Console.dll"]
