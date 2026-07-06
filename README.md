# 👻 Memory Shards

**Memory Shards** é uma plataforma minimalista, segura e focada em privacidade para o compartilhamento de mensagens secretas e temporárias. Seus segredos (shards) são criptografados, protegidos e configurados para se autodestruírem permanentemente após um tempo determinado.

---

## 🌟 Principais Funcionalidades

### 🔒 Segurança em Primeiro Lugar
* **Criptografia Opcional (PIN):** Proteja mensagens com um código PIN de 6 dígitos numéricos. A descriptografia só ocorre se a senha correta for fornecida.
* **Proteção Anti-Força Bruta:** Implementação rigorosa de *Progressive Backoff*. Após 3 tentativas incorretas, a interface bloqueia o usuário com um timer exponencial (1m, 2m, 4m, 8m...) salvo no cache (`localStorage`).
* **Proteção Anti-Screenshot e "Bisbilhoteiros":** 
  * Se a aba perder o foco (ex: o usuário tenta abrir uma ferramenta de recorte), a mensagem embaraça instantaneamente.
  * **(Removido a pedido para melhor UX)**: Inicialmente utilizava um sistema "Hold to Reveal".

### ✏️ Criação e Leitura 
* **Rich Text Editor:** Editor de texto integrado através do **Tiptap**, permitindo negrito, itálico, código, listas, etc.
* **Autodestruição Temporizada:** Escolha se o segredo irá expirar em 12 horas, 7 dias ou 1 mês. Após o prazo, o dado desaparece permanentemente do banco.
* **Compartilhamento Fácil:** Gere e compartilhe Links ou **QR Codes** da mensagem secreta diretamente pela plataforma.

### 🎨 Design Minimalista e Moderno
* **Glassmorphism UI:** Interface limpa, utilizando caixas de vidro (glass-card) e fontes modernas (Inter).
* **Dark Mode Dinâmico:** Tema claro e escuro que respeita a configuração padrão do seu sistema, além de permitir alteração manual com botão de sol/lua no cabeçalho.
* **OTP Input Style:** Campo numérico dividido em caixas (estilo OTP/Token) para a inserção confortável do PIN.
* **SEO Dinâmico:** Títulos e descrições das páginas ajustam-se automaticamente à rota atual.

---

## 🛠️ Tecnologias Utilizadas

**Frontend:**
* [React](https://reactjs.org/) + [TypeScript](https://www.typescriptlang.org/)
* [Vite](https://vitejs.dev/) (Build tool e HMR)
* [Tiptap](https://tiptap.dev/) (Editor de Rich Text)
* [Lucide React](https://lucide.dev/) (Ícones)
* [React QR Code](https://www.npmjs.com/package/react-qr-code)

**Backend:**
* C# / .NET (API RESTful segura)
* Criptografia e Lógica de destruição em memória/banco.

---

## 🚀 Como Rodar o Projeto (Desenvolvimento)

1. **Clone o repositório:**
   ```bash
   git clone https://github.com/estevooliveira111/memory-shards.git
   cd memory-shards
   ```

2. **Instale as dependências do Frontend:**
   ```bash
   npm install
   ```

3. **Inicie o servidor de desenvolvimento Vite:**
   ```bash
   npm run dev
   # Ou utilize o Makefile existente: make dev
   ```

4. Acesse `http://localhost:5173` no seu navegador.

---

## 🧠 Decisões de Arquitetura & UX
* **Local Storage para Estado de Tema e Backoff:** Evita chamadas excessivas ao servidor para estado visual, mas protege a segurança travando fisicamente a interface na presença de ataques de força bruta locais.
* **Temporizadores Absolutos:** O tempo de bloqueio é calculado com base no `Date.now()`, salvando o limite real no cache. Isso impede burlar o cronômetro com um simples "F5".
* **Timezone Automático:** O sistema salva timestamps em modo Universal (UTC) internamente e formata na interface (`toLocaleString()`), adequando perfeitamente ao idioma e fuso horário do usuário que visualiza.

---

*Desenvolvido como um cofre moderno para fragmentos de memória.*
