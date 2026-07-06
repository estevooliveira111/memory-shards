import { useState, useEffect } from 'react';
import { api } from '../services/api';
import { Lock, Clock, Send, ShieldAlert, Check, Share2, Bold, Italic, Strikethrough, Code, List, ListOrdered } from 'lucide-react';
import QRCode from 'react-qr-code';
import { useEditor, EditorContent } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';

const MenuBar = ({ editor }: { editor: any }) => {
  if (!editor) return null;

  return (
    <div className="tiptap-toolbar">
      <button type="button" onClick={() => editor.chain().focus().toggleBold().run()} className={editor.isActive('bold') ? 'is-active' : ''}><Bold size={16} /></button>
      <button type="button" onClick={() => editor.chain().focus().toggleItalic().run()} className={editor.isActive('italic') ? 'is-active' : ''}><Italic size={16} /></button>
      <button type="button" onClick={() => editor.chain().focus().toggleStrike().run()} className={editor.isActive('strike') ? 'is-active' : ''}><Strikethrough size={16} /></button>
      <button type="button" onClick={() => editor.chain().focus().toggleCode().run()} className={editor.isActive('code') ? 'is-active' : ''}><Code size={16} /></button>
      <div className="toolbar-divider" />
      <button type="button" onClick={() => editor.chain().focus().toggleBulletList().run()} className={editor.isActive('bulletList') ? 'is-active' : ''}><List size={16} /></button>
      <button type="button" onClick={() => editor.chain().focus().toggleOrderedList().run()} className={editor.isActive('orderedList') ? 'is-active' : ''}><ListOrdered size={16} /></button>
    </div>
  );
};

export default function CreateMessage() {
  useEffect(() => {
    document.title = "Criar Segredo Seguro | Memory Shards";
    const metaDescription = document.querySelector('meta[name="description"]');
    if (metaDescription) {
      metaDescription.setAttribute('content', 'Crie uma mensagem criptografada, segura e temporária. Ninguém além do destinatário poderá ler.');
    }
  }, []);

  const [content, setContent] = useState('');
  const [expiration, setExpiration] = useState('12h');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successData, setSuccessData] = useState<{ url: string; expiresAt: string } | null>(null);
  const [copied, setCopied] = useState(false);

  const editor = useEditor({
    extensions: [StarterKit],
    content: '',
    onUpdate: ({ editor }) => {
      setContent(editor.getHTML());
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Tiptap returns <p></p> when empty, so we need to check if there is actual text
    if (!editor || editor.isEmpty) {
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

        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', margin: '2rem 0', background: 'white', padding: '1rem', borderRadius: '12px' }}>
          <QRCode 
            id="qr-code-svg"
            value={successData.url} 
            size={180}
            level="M"
          />
          <button 
            className="secondary" 
            style={{ marginTop: '1rem', fontSize: '0.75rem', padding: '0.4rem 0.8rem', border: '1px solid #e5e7eb' }}
            onClick={() => {
              const svg = document.getElementById("qr-code-svg");
              if (!svg) return;
              const svgData = new XMLSerializer().serializeToString(svg);
              const canvas = document.createElement("canvas");
              const ctx = canvas.getContext("2d");
              const img = new Image();
              img.onload = () => {
                canvas.width = img.width + 40; // add padding
                canvas.height = img.height + 40;
                if (ctx) {
                  ctx.fillStyle = "white";
                  ctx.fillRect(0, 0, canvas.width, canvas.height);
                  ctx.drawImage(img, 20, 20);
                }
                canvas.toBlob(async (blob) => {
                  if (!blob) return;
                  const file = new File([blob], "qrcode.png", { type: "image/png" });
                  if (navigator.canShare && navigator.canShare({ files: [file] })) {
                    try { await navigator.share({ files: [file], title: 'QR Code' }); } catch (e) {}
                  } else {
                    const a = document.createElement('a');
                    a.href = URL.createObjectURL(blob);
                    a.download = 'qrcode.png';
                    a.click();
                  }
                }, "image/png");
              };
              img.src = "data:image/svg+xml;base64," + btoa(unescape(encodeURIComponent(svgData)));
            }}
          >
            Compartilhar Imagem do QR Code
          </button>
        </div>

        <p className="meta-text">
          Expira em: {new Date(successData.expiresAt).toLocaleString()}
        </p>

        <button 
          className="secondary" 
          style={{ width: '100%', marginTop: '1.5rem' }}
          onClick={() => {
            setSuccessData(null);
            editor?.commands.setContent('');
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
          <div className={`tiptap-container ${loading ? 'disabled' : ''}`}>
            <MenuBar editor={editor} />
            <EditorContent editor={editor} />
          </div>
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

        <button type="submit" className="primary" disabled={loading || !editor || editor.isEmpty}>
          {loading ? 'Criptografando e salvando...' : (
            <>Gerar Link Seguro <Send size={18} /></>
          )}
        </button>
      </form>
    </div>
  );
}
