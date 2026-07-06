import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { Ghost, Sun, Moon } from 'lucide-react';
import CreateMessage from './pages/CreateMessage';
import ViewMessage from './pages/ViewMessage';

function App() {
  const [theme, setTheme] = useState<'light' | 'dark' | 'system'>(() => {
    return (localStorage.getItem('theme') as 'light' | 'dark' | 'system') || 'system';
  });

  useEffect(() => {
    const root = document.documentElement;
    root.classList.remove('light', 'dark');
    
    if (theme === 'system') {
      localStorage.removeItem('theme');
    } else {
      root.classList.add(theme);
      localStorage.setItem('theme', theme);
    }
  }, [theme]);

  const toggleTheme = () => {
    if (theme === 'system') {
      const isSystemDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      setTheme(isSystemDark ? 'light' : 'dark');
    } else if (theme === 'dark') {
      setTheme('light');
    } else {
      setTheme('dark');
    }
  };

  return (
    <BrowserRouter>
      <div className="layout-container">
        <header style={{ position: 'relative' }}>
          <button 
            onClick={toggleTheme}
            style={{ position: 'absolute', right: 0, top: 0, background: 'transparent', border: 'none', cursor: 'pointer', color: 'var(--text-secondary)' }}
            title="Alternar Tema"
          >
            {theme === 'dark' ? <Sun size={24} /> : <Moon size={24} />}
          </button>
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
