import { useState } from 'react';
import { api } from '../services/api';
import { Lock, Clock, Send, ShieldAlert, Check, Share2 } from 'lucide-react';
import QRCode from 'react-qr-code';

export default function CreateMessage() {
  const [content, setContent] = useState('');
  const [expiration, setExpiration] = useState('12h');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successData, setSuccessData] = useState<{ url: string; expiresAt: string } | null>(null);
  const [copied, setCopied] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!content.trim()) {
      setError('O conteúdo da mensagem é obrigatório.');
      return;
    }
    if (password && !/^\d{1,6}$/.test(password)) {
      setError('A senha deve conter apenas números (máx 6 dígitos).');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = await api.createMessage({
        content,
        expiration,
        password: password || undefined,
      });
      // The backend returns url like "https://localhost:7001/m/slug"
      // Since we want the frontend URL, we reconstruct it using window.location.origin
      const frontendUrl = `${window.location.origin}/m/${response.slug}`;
      setSuccessData({ url: frontendUrl, expiresAt: response.expiresAt });
    } catch (err: any) {
      setError(err.message || 'Falha ao criar a mensagem secreta.');
    } finally {
      setLoading(false);
    }
  };

  const copyToClipboard = () => {
    if (successData) {
      navigator.clipboard.writeText(successData.url);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  if (successData) {
    return (
      <div className="glass-card">
        <h2 className="title">Sua mensagem está pronta! 🤫</h2>
        <div className="success-message">
          O link abaixo contém sua mensagem temporária. 
          {password && ' Como você adicionou uma senha, a mensagem está criptografada e segura.'}
        </div>
        
        <div className="form-group">
          <label>Link de Compartilhamento</label>
          <div className="link-box">
            <input type="text" readOnly value={successData.url} />
            <button className="primary" style={{ width: 'auto' }} onClick={copyToClipboard}>
              {copied ? <Check size={20} /> : 'Copiar'}
            </button>
          </div>
          {typeof navigator !== 'undefined' && navigator.share && (
            <button 
              className="secondary" 
              style={{ width: '100%', marginTop: '0.5rem', display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '0.5rem' }}
              onClick={async () => {
                try {
                  await navigator.share({
                    title: 'Memory Shards',
                    text: 'Tenho um segredo para você! Abra este link antes que ele expire:',
                    url: successData.url
                  });
                } catch (err) {
                  console.error('Erro ao compartilhar:', err);
                }
              }}
            >
              <Share2 size={18} /> Compartilhar Link
            </button>
          )}
        </div>

        <div style={{ display: 'flex', justifyContent: 'center', margin: '2rem 0', background: 'white', padding: '1rem', borderRadius: '12px' }}>
          <QRCode 
            value={successData.url} 
            size={180}
            level="M"
          />
        </div>

        <p className="meta-text">
          Expira em: {new Date(successData.expiresAt).toLocaleString()}
        </p>

        <button 
          className="secondary" 
          style={{ width: '100%', marginTop: '1.5rem' }}
          onClick={() => {
            setSuccessData(null);
            setContent('');
            setPassword('');
          }}
        >
          Criar Nova Mensagem
        </button>
      </div>
    );
  }

  return (
    <div className="glass-card">
      <h2 className="title">Criar Mensagem Secreta</h2>
      
      {error && (
        <div className="error-message">
          <ShieldAlert size={20} />
          <span>{error}</span>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>O que você deseja compartilhar?</label>
          <textarea 
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder="Digite seu texto, código ou segredo aqui..."
            disabled={loading}
            maxLength={50000}
          />
        </div>

        <div className="form-group">
          <label>Tempo de Expiração <Clock size={14} style={{ display: 'inline', verticalAlign: 'middle', marginLeft: '4px' }}/></label>
          <select 
            value={expiration} 
            onChange={(e) => setExpiration(e.target.value)}
            disabled={loading}
          >
            <option value="12h">12 Horas</option>
            <option value="7d">7 Dias</option>
            <option value="1m">1 Mês</option>
          </select>
        </div>

        <div className="form-group">
          <label>Senha de Proteção (Opcional) <Lock size={14} style={{ display: 'inline', verticalAlign: 'middle', marginLeft: '4px' }}/></label>
          <input 
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Até 6 dígitos (ex: 123456)"
            disabled={loading}
            pattern="\d*"
            maxLength={6}
          />
          <p className="meta-text" style={{ textAlign: 'left', marginTop: '0.25rem', fontSize: '0.75rem' }}>
            Se definido, a mensagem será criptografada com AES-256 e só poderá ser lida com a senha.
          </p>
        </div>

        <button type="submit" className="primary" disabled={loading || !content.trim()}>
          {loading ? 'Criptografando e salvando...' : (
            <>Gerar Link Seguro <Send size={18} /></>
          )}
        </button>
      </form>
    </div>
  );
}
