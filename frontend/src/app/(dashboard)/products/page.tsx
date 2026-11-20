'use client';
import { useEffect, useState, useCallback } from 'react';
import api from '@/lib/api';
import Modal from '@/components/ui/Modal';
import type { Product, ProductCategory, Unit } from '@/types';

// ─── Category Form ────────────────────────────────────────────────────────────
function CategoryForm({ onSaved, onClose }: { onSaved: () => void; onClose: () => void }) {
  const [form, setForm] = useState({ name: '', description: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.post('/api/ProductCategories', form);
      onSaved();
      onClose();
    } catch {
      setError('Failed to save. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <form onSubmit={submit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
        <input required value={form.name} onChange={e => setForm({ ...form, name: e.target.value })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="e.g. Raw Materials" />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
        <input value={form.description} onChange={e => setForm({ ...form, description: e.target.value })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Optional" />
      </div>
      {error && <p className="text-red-600 text-sm">{error}</p>}
      <div className="flex gap-3 pt-2">
        <button type="button" onClick={onClose} className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50">Cancel</button>
        <button type="submit" disabled={saving} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg text-sm font-medium">
          {saving ? 'Saving...' : 'Save'}
        </button>
      </div>
    </form>
  );
}

// ─── Unit Form ────────────────────────────────────────────────────────────────
function UnitForm({ onSaved, onClose }: { onSaved: () => void; onClose: () => void }) {
  const [form, setForm] = useState({ name: '', symbol: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.post('/api/Units', form);
      onSaved();
      onClose();
    } catch {
      setError('Failed to save. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <form onSubmit={submit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
        <input required value={form.name} onChange={e => setForm({ ...form, name: e.target.value })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="e.g. Kilogram" />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Symbol</label>
        <input required value={form.symbol} onChange={e => setForm({ ...form, symbol: e.target.value })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="e.g. kg" />
      </div>
      {error && <p className="text-red-600 text-sm">{error}</p>}
      <div className="flex gap-3 pt-2">
        <button type="button" onClick={onClose} className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50">Cancel</button>
        <button type="submit" disabled={saving} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg text-sm font-medium">
          {saving ? 'Saving...' : 'Save'}
        </button>
      </div>
    </form>
  );
}

// ─── Product Form ─────────────────────────────────────────────────────────────
function ProductForm({ categories, units, onSaved, onClose }: {
  categories: ProductCategory[]; units: Unit[];
  onSaved: () => void; onClose: () => void;
}) {
  const [form, setForm] = useState({
    name: '', sku: '', categoryId: '', unitId: '',
    costPrice: '', sellingPrice: '', reorderLevel: '0',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.post('/api/Products', {
        ...form,
        costPrice: parseFloat(form.costPrice),
        sellingPrice: parseFloat(form.sellingPrice),
        reorderLevel: parseInt(form.reorderLevel),
      });
      onSaved();
      onClose();
    } catch {
      setError('Failed to save. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const f = (field: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  return (
    <form onSubmit={submit} className="space-y-4">
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
          <input required value={form.name} onChange={f('name')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Product name" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">SKU</label>
          <input required value={form.sku} onChange={f('sku')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="e.g. RM-001" />
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Category</label>
          <select required value={form.categoryId} onChange={f('categoryId')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            <option value="">Select category</option>
            {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Unit</label>
          <select required value={form.unitId} onChange={f('unitId')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            <option value="">Select unit</option>
            {units.map(u => <option key={u.id} value={u.id}>{u.name} ({u.symbol})</option>)}
          </select>
        </div>
      </div>
      <div className="grid grid-cols-3 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Cost Price (₹)</label>
          <input required type="number" min="0" step="0.01" value={form.costPrice} onChange={f('costPrice')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="0.00" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Selling Price (₹)</label>
          <input required type="number" min="0" step="0.01" value={form.sellingPrice} onChange={f('sellingPrice')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="0.00" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Reorder Level</label>
          <input required type="number" min="0" value={form.reorderLevel} onChange={f('reorderLevel')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </div>
      </div>
      {error && <p className="text-red-600 text-sm">{error}</p>}
      <div className="flex gap-3 pt-2">
        <button type="button" onClick={onClose} className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50">Cancel</button>
        <button type="submit" disabled={saving} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg text-sm font-medium">
          {saving ? 'Saving...' : 'Save Product'}
        </button>
      </div>
    </form>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────
type Tab = 'products' | 'categories' | 'units';

export default function ProductsPage() {
  const [tab, setTab] = useState<Tab>('products');
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<ProductCategory[]>([]);
  const [units, setUnits] = useState<Unit[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [modal, setModal] = useState<'product' | 'category' | 'unit' | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [p, c, u] = await Promise.all([
        api.get<Product[]>('/api/Products'),
        api.get<ProductCategory[]>('/api/ProductCategories'),
        api.get<Unit[]>('/api/Units'),
      ]);
      setProducts(p.data);
      setCategories(c.data);
      setUnits(u.data);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const filtered = products.filter(p =>
    p.name.toLowerCase().includes(search.toLowerCase()) ||
    p.sku.toLowerCase().includes(search.toLowerCase())
  );

  const tabs: { key: Tab; label: string }[] = [
    { key: 'products', label: `Products (${products.length})` },
    { key: 'categories', label: `Categories (${categories.length})` },
    { key: 'units', label: `Units (${units.length})` },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Products</h1>
        <div className="flex gap-2">
          {tab === 'products' && (
            <button onClick={() => setModal('product')}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg">
              + New Product
            </button>
          )}
          {tab === 'categories' && (
            <button onClick={() => setModal('category')}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg">
              + New Category
            </button>
          )}
          {tab === 'units' && (
            <button onClick={() => setModal('unit')}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg">
              + New Unit
            </button>
          )}
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 mb-4 bg-gray-100 rounded-lg p-1 w-fit">
        {tabs.map(t => (
          <button key={t.key} onClick={() => setTab(t.key)}
            className={`px-4 py-1.5 rounded-md text-sm font-medium transition-colors ${
              tab === t.key ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'
            }`}>
            {t.label}
          </button>
        ))}
      </div>

      <div className="bg-white rounded-xl border border-gray-200">
        {/* Products Tab */}
        {tab === 'products' && (
          <>
            <div className="p-4 border-b border-gray-100">
              <input type="text" placeholder="Search by name or SKU..." value={search}
                onChange={e => setSearch(e.target.value)}
                className="w-72 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            {loading ? <div className="p-8 text-center text-gray-400 text-sm">Loading...</div>
              : filtered.length === 0 ? <div className="p-8 text-center text-gray-400 text-sm">No products yet — click + New Product</div>
              : (
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                      <th className="px-4 py-3">SKU</th><th className="px-4 py-3">Name</th>
                      <th className="px-4 py-3">Category</th><th className="px-4 py-3">Unit</th>
                      <th className="px-4 py-3 text-right">Cost</th><th className="px-4 py-3 text-right">Selling</th>
                      <th className="px-4 py-3">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-50">
                    {filtered.map(p => (
                      <tr key={p.id} className="hover:bg-gray-50">
                        <td className="px-4 py-3 font-mono text-gray-500">{p.sku}</td>
                        <td className="px-4 py-3 font-medium text-gray-900">{p.name}</td>
                        <td className="px-4 py-3 text-gray-600">{p.categoryName}</td>
                        <td className="px-4 py-3 text-gray-600">{p.unitName}</td>
                        <td className="px-4 py-3 text-right text-gray-600">₹{p.costPrice.toFixed(2)}</td>
                        <td className="px-4 py-3 text-right font-medium">₹{p.sellingPrice.toFixed(2)}</td>
                        <td className="px-4 py-3">
                          <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${p.isActive ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                            {p.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
          </>
        )}

        {/* Categories Tab */}
        {tab === 'categories' && (
          loading ? <div className="p-8 text-center text-gray-400 text-sm">Loading...</div>
            : categories.length === 0 ? <div className="p-8 text-center text-gray-400 text-sm">No categories yet — click + New Category</div>
            : (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                    <th className="px-4 py-3">Name</th><th className="px-4 py-3">Description</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {categories.map(c => (
                    <tr key={c.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 font-medium text-gray-900">{c.name}</td>
                      <td className="px-4 py-3 text-gray-500">{c.description || '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )
        )}

        {/* Units Tab */}
        {tab === 'units' && (
          loading ? <div className="p-8 text-center text-gray-400 text-sm">Loading...</div>
            : units.length === 0 ? <div className="p-8 text-center text-gray-400 text-sm">No units yet — click + New Unit</div>
            : (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                    <th className="px-4 py-3">Name</th><th className="px-4 py-3">Symbol</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {units.map(u => (
                    <tr key={u.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 font-medium text-gray-900">{u.name}</td>
                      <td className="px-4 py-3 font-mono text-gray-500">{u.symbol}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )
        )}
      </div>

      {/* Modals */}
      {modal === 'category' && (
        <Modal title="New Category" onClose={() => setModal(null)}>
          <CategoryForm onSaved={load} onClose={() => setModal(null)} />
        </Modal>
      )}
      {modal === 'unit' && (
        <Modal title="New Unit" onClose={() => setModal(null)}>
          <UnitForm onSaved={load} onClose={() => setModal(null)} />
        </Modal>
      )}
      {modal === 'product' && (
        <Modal title="New Product" onClose={() => setModal(null)}>
          <ProductForm categories={categories} units={units} onSaved={load} onClose={() => setModal(null)} />
        </Modal>
      )}
    </div>
  );
}
