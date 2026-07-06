# ==========================================
# Estágio 1: Build do Front-end (Vite)
# ==========================================
FROM node:22-alpine AS frontend-build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# ==========================================
# Estágio 2: Build do Back-end (.NET 10)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS backend-build
WORKDIR /src
COPY ["Api/Api.csproj", "Api/"]
RUN dotnet restore "Api/Api.csproj"
COPY Api/ Api/
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Publish
FROM backend-build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# Estágio 3: Runtime Unificado (Final)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# 1. Copia o backend compilado
COPY --from=publish /app/publish .

# 2. Copia o frontend (dist) para a pasta wwwroot (onde o ASP.NET procura arquivos estáticos)
COPY --from=frontend-build /app/dist ./wwwroot

# Configura o ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]
