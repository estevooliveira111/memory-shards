export interface CreateMessageRequest {
  content: string;
  expiration: string;
  password?: string;
}

export interface CreateMessageResponse {
  slug: string;
  url: string;
  expiresAt: string;
}

export interface GetMessageResponse {
  content: string;
}

export interface ApiError {
  title: string;
  status: number;
}

const handleResponse = async (res: Response) => {
  if (!res.ok) {
    let errorData = null;
    try {
      errorData = await res.json();
    } catch {
      // Ignored
    }
    const error: any = new Error(errorData?.title || 'Um erro inesperado ocorreu.');
    error.status = res.status;
    error.data = errorData;
    throw error;
  }
  
  if (res.status === 204) return null;
  return res.json();
};

const API_BASE_URL = import.meta.env.VITE_API_URL || '';

export const api = {
  createMessage: async (data: CreateMessageRequest): Promise<CreateMessageResponse> => {
    const res = await fetch(`${API_BASE_URL}/api/messages`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    return handleResponse(res);
  },

  getMessage: async (slug: string, password?: string): Promise<GetMessageResponse> => {
    const res = await fetch(`${API_BASE_URL}/api/messages/${slug}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        ...(password ? { 'X-Password': password } : {})
      }
    });
    return handleResponse(res);
  }
};
