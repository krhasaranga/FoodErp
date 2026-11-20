import axios from 'axios';
import { auth } from './auth';

// Call backend directly — HTTP port avoids self-signed cert issues
// CORS is configured on the backend to allow localhost:3000
const api = axios.create({
  baseURL: 'http://localhost:5036',
});

api.interceptors.request.use((config) => {
  const token = auth.getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      auth.clearToken();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
