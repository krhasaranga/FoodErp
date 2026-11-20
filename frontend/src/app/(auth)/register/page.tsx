'use client';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import api from '@/lib/api';
import { auth } from '@/lib/auth';
import type { AuthResponse } from '@/types';

export default function RegisterPage() {
  const router = useRouter();
  const [form, setForm] = useState({
    tenantName: '',
    tenantSlug: '',
    firstName: '',
    lastName: '',
    email: '',
    password: '',
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const set = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((f) => ({ ...f, [field]: e.target.value }));

  // Auto-generate slug from company name
  const handleCompanyName = (e: React.ChangeEvent<HTMLInputElement>) => {
    const name = e.target.value;
    setForm((f) => ({
      ...f,
      tenantName: name,
      tenantSlug: name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, ''),
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const { data } = await api.post<AuthResponse>('/api/Auth/register', form);
      auth.setToken(data.token);
      router.push('/dashboard');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })
        ?.response?.data?.message;
      setError(msg ?? 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-200 w-full max-w-lg p-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Set up your company</h1>
        <p className="text-gray-500 mt-1 text-sm">Create your FoodERP account — takes 30 seconds</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Company name</label>
          <input
            required
            value={form.tenantName}
            onChange={handleCompanyName}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder="Acme Foods Pvt Ltd"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Company ID</label>
          <input
            required
            value={form.tenantSlug}
            onChange={set('tenantSlug')}
            pattern="[a-z0-9\-]+"
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono"
            placeholder="acme-foods"
          />
          <p className="text-xs text-gray-400 mt-1">Lowercase letters, numbers, hyphens only</p>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">First name</label>
            <input
              required
              value={form.firstName}
              onChange={set('firstName')}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="John"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Last name</label>
            <input
              required
              value={form.lastName}
              onChange={set('lastName')}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Doe"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Work email</label>
          <input
            type="email"
            required
            value={form.email}
            onChange={set('email')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder="john@acme.com"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
          <input
            type="password"
            required
            minLength={8}
            value={form.password}
            onChange={set('password')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder="Min 8 characters, 1 uppercase, 1 digit"
          />
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg px-4 py-3">
            {error}
          </div>
        )}

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-medium py-2.5 rounded-lg text-sm transition-colors mt-2"
        >
          {loading ? 'Creating account...' : 'Create account'}
        </button>
      </form>

      <p className="text-sm text-gray-500 text-center mt-6">
        Already have an account?{' '}
        <Link href="/login" className="text-blue-600 hover:underline font-medium">
          Sign in
        </Link>
      </p>
    </div>
  );
}
