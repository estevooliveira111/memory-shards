import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../services/api';
import { Lock, EyeOff, ShieldAlert, KeyRound, Eye } from 'lucide-react';
import DOMPurify from 'dompurify';

export default function ViewMessage() {
  const { slug } = useParams<{ slug: string }>();
  
  const [content, setContent] = useState<string | null>(null);
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<{ type: string, message: string } | null>(null);
  const [requiresPassword, setRequiresPassword] = useState(false);

  // Anti-screenshot state
  const [isRevealed, setIsRevealed] = useState(false);
  const [isFocused, setIsFocused] = useState(true);
  const [sessionId] = useState(() => Math.random().toString(36).substring(2, 10).toUpperCase());
  const [timestamp] = useState(() => new Date().toISOString());

  useEffect(() => {
    if (slug) {
      fetchMessage();
    }
  }, [slug]);

  // Anti-screenshot focus listener
  useEffect(() => {
    const handleVisibilityChange = () => setIsFocused(!document.hidden);
    const handleBlur = () => { setIsRevealed(false); setIsFocused(false); };
    const handleFocus = () => setIsFocused(true);
    
    document.addEventListener('visibilitychange', handleVisibilityChange);
    window.addEventListener('blur', handleBlur);
    window.addEventListener('focus', handleFocus);
    
    // Prevent context menu (right click) globally on this view
    const handleContextMenu = (e: MouseEvent) => e.preventDefault();
    document.addEventListener('contextmenu', handleContextMenu);

    return () => {
      document.removeEventListener('visibilitychange', handleVisibilityChange);
      window.removeEventListener('blur', handleBlur);
      window.removeEventListener('focus', handleFocus);
      document.removeEventListener('contextmenu', handleContextMenu);
    };
  }, []);

  const fetchMessage = async (pwd?: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.getMessage(slug!, pwd);
      setContent(response.content);
      setRequiresPassword(false);
    } catch (err: any) {
      if (err.status === 401) {
        setRequiresPassword(true);
        if (pwd) {
          setError({ type: 'auth', message: 'Senha incorreta. Tente novamente.' });
        }
      } else if (err.status === 404) {
        setError({ type: 'not_found', message: 'Esta mensagem não existe ou o link está incorreto.' });
      } else if (err.status === 410) {
        setError({ type: 'expired', message: 'Esta mensagem já expirou e foi permanentemente destruída.' });
      } else {
        setError({ type: 'error', message: err.message || 'Erro ao carregar a mensagem.' });
      }
    } finally {
      setLoading(false);
    }
  };

  const handlePasswordSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!password.trim()) return;
    fetchMessage(password);
  };

  if (loading) {
    return (
      <div className="glass-card" style={{ textAlign: 'center' }}>
        <div className="title">Descriptografando...</div>
      </div>
    );
  }

  if (error && error.type !== 'auth') {
    return (
      <div className="glass-card" style={{ textAlign: 'center' }}>
        <EyeOff size={48} color="var(--text-secondary)" style={{ margin: '0 auto 1.5rem' }} />
        <h2 className="title">Oops!</h2>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>{error.message}</p>
        <Link to="/">
          <button className="primary">Voltar para Início</button>
        </Link>
      </div>
    );
  }

  if (requiresPassword) {
    return (
      <div className="glass-card">
        <div style={{ textAlign: 'center', marginBottom: '1.5rem' }}>
          <KeyRound size={40} color="var(--accent-primary)" />
        </div>
        <h2 className="title">Mensagem Protegida</h2>
        <p style={{ color: 'var(--text-secondary)', textAlign: 'center', marginBottom: '2rem' }}>
          Esta mensagem está criptografada. Digite a senha para visualizar o conteúdo.
        </p>

        {error?.type === 'auth' && (
          <div className="error-message">
            <ShieldAlert size={20} />
            <span>{error.message}</span>
          </div>
        )}

        <form onSubmit={handlePasswordSubmit}>
          <div className="form-group">
            <label>Senha Numérica</label>
            <input 
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Digite a senha..."
              autoFocus
              pattern="\d*"
              maxLength={6}
            />
          </div>
          <button type="submit" className="primary" disabled={!password}>
            Descriptografar <Lock size={18} />
          </button>
        </form>
      </div>
    );
  }

  return (
    <div className="glass-card" style={{ userSelect: 'none', WebkitUserSelect: 'none' }}>
      <h2 className="title">Mensagem Recebida</h2>
      
      {!isFocused && (
        <div className="error-message" style={{ justifyContent: 'center' }}>
          <ShieldAlert size={20} />
          <span>Foco perdido. Clique na janela para visualizar.</span>
        </div>
      )}

      <div className="secure-message-container" style={{ position: 'relative', overflow: 'hidden', borderRadius: '8px', minHeight: '150px', background: '#f9fafb', border: '1px solid var(--glass-border)' }}>
        <div 
          className={`tiptap-content secure-blur-content ${(!isRevealed || !isFocused) ? 'is-blurred' : ''}`} 
          dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(content || '') }} 
          style={{ padding: '1.5rem', pointerEvents: isRevealed ? 'auto' : 'none' }}
        />
        
        {isRevealed && isFocused && (
          <div className="watermark-overlay">
            {Array.from({ length: 40 }).map((_, i) => (
              <div key={i} className="watermark-item">
                ID: {sessionId}<br/>{timestamp}
              </div>
            ))}
          </div>
        )}

        {(!isRevealed || !isFocused) && (
          <div style={{ position: 'absolute', top: '50%', left: '50%', transform: 'translate(-50%, -50%)', textAlign: 'center', pointerEvents: 'none' }}>
            <EyeOff size={32} color="var(--text-secondary)" style={{ margin: '0 auto 0.5rem' }} />
            <span style={{ color: 'var(--text-secondary)', fontWeight: 600 }}>Conteúdo Oculto</span>
          </div>
        )}
      </div>

      <button 
        className="primary" 
        style={{ width: '100%', marginTop: '1.5rem', touchAction: 'none' }}
        onPointerDown={(e) => { e.preventDefault(); setIsRevealed(true); }}
        onPointerUp={(e) => { e.preventDefault(); setIsRevealed(false); }}
        onPointerLeave={(e) => { e.preventDefault(); setIsRevealed(false); }}
        onContextMenu={(e) => e.preventDefault()}
      >
        <Eye size={18} /> Segure para revelar
      </button>

      <p className="meta-text" style={{ marginTop: '2rem' }}>
        Esta mensagem expira automaticamente e não poderá ser recuperada.
      </p>

      <Link to="/" style={{ display: 'block', marginTop: '1.5rem' }}>
        <button className="secondary" style={{ width: '100%' }}>
          Criar sua própria mensagem
        </button>
      </Link>
    </div>
  );
}
