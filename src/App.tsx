import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { Ghost } from 'lucide-react';
import CreateMessage from './pages/CreateMessage';
import ViewMessage from './pages/ViewMessage';

function App() {
  return (
    <BrowserRouter>
      <div className="layout-container">
        <header>
          <Link to="/" className="logo">
            <Ghost size={36} color="var(--accent-primary)" />
            <span>Memory Shards</span>
          </Link>
          <p style={{ color: 'var(--text-secondary)', marginTop: '0.5rem', fontSize: '0.95rem' }}>
            Compartilhe segredos de forma segura e temporária.
          </p>
        </header>

        <main>
          <Routes>
            <Route path="/" element={<CreateMessage />} />
            <Route path="/m/:slug" element={<ViewMessage />} />
            <Route path="*" element={
              <div className="glass-card" style={{ textAlign: 'center' }}>
                <h2 className="title">404 - Página não encontrada</h2>
                <Link to="/">
                  <button className="primary">Voltar para o Início</button>
                </Link>
              </div>
            } />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
