.PHONY: install dev-api dev-web dev build clean

# ── Variáveis ──
API_DIR = Api
WEB_DIR = .

# ── Instalação ──
install:
	@echo "📦 Instalando dependências do Front-end..."
	npm install
	@echo "📦 Restaurando pacotes do Back-end..."
	cd $(API_DIR) && dotnet restore

# ── Desenvolvimento ──
dev-api:
	@echo "🚀 Iniciando Back-end (API)..."
	cd $(API_DIR) && dotnet run

dev-web:
	@echo "🚀 Iniciando Front-end (Web)..."
	npm run dev

# Roda ambos simultaneamente (necessário suporte a jobs paralelos com make -j2 dev)
dev:
	@echo "🔥 Iniciando API e Web em paralelo (use 'make -j2 dev')..."
	$(MAKE) -j2 dev-api dev-web

# ── Build ──
build:
	@echo "🔨 Buildando Back-end..."
	cd $(API_DIR) && dotnet build --configuration Release
	@echo "🔨 Buildando Front-end..."
	npm run build

# ── Limpeza ──
clean:
	@echo "🧹 Limpando artefatos de build do Back-end..."
	cd $(API_DIR) && dotnet clean
	rm -rf $(API_DIR)/bin $(API_DIR)/obj
	@echo "🧹 Limpando artefatos do Front-end..."
	rm -rf node_modules dist
	@echo "✨ Limpeza concluída!"
