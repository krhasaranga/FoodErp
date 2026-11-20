'use client';
import { useEffect, useState, useCallback } from 'react';
import api from '@/lib/api';
import Modal from '@/components/ui/Modal';
import type { ProductionOrder, Recipe, Product } from '@/types';

// ─── Recipe Form ──────────────────────────────────────────────────────────────
function RecipeForm({ products, onSaved, onClose }: {
  products: Product[]; onSaved: () => void; onClose: () => void;
}) {
  const [name, setName] = useState('');
  const [outputProductId, setOutputProductId] = useState('');
  const [outputQuantity, setOutputQuantity] = useState('');
  const [lines, setLines] = useState([{ inputProductId: '', quantity: '' }]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const addLine = () => setLines(l => [...l, { inputProductId: '', quantity: '' }]);
  const removeLine = (i: number) => setLines(l => l.filter((_, idx) => idx !== i));
  const updateLine = (i: number, field: string, value: string) =>
    setLines(l => l.map((line, idx) => idx === i ? { ...line, [field]: value } : line));

  const submit = async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true); setError('');
    try {
      await api.post('/api/Recipes', {
        name, outputProductId,
        outputQuantity: parseFloat(outputQuantity),
        lines: lines.map(l => ({ inputProductId: l.inputProductId, quantity: parseFloat(l.quantity) })),
      });
      onSaved(); onClose();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setError(e?.response?.data?.error ?? 'Failed to save recipe.');
    } finally { setSaving(false); }
  };

  return (
    <form onSubmit={submit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Recipe Name</label>
        <input required value={name} onChange={e => setName(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="e.g. Tomato Sauce Recipe" />
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Output Product</label>
          <select required value={outputProductId} onChange={e => setOutputProductId(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            <option value="">Select product</option>
            {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Output Quantity</label>
          <input required type="number" min="0.01" step="0.01" value={outputQuantity}
            onChange={e => setOutputQuantity(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="e.g. 10" />
        </div>
      </div>
      <div>
        <div className="flex items-center justify-between mb-2">
          <label className="text-sm font-medium text-gray-700">Ingredients</label>
          <button type="button" onClick={addLine} className="text-xs text-blue-600 hover:underline">+ Add Ingredient</button>
        </div>
        <div className="space-y-2">
          {lines.map((line, i) => (
            <div key={i} className="grid grid-cols-12 gap-2 items-center">
              <div className="col-span-7">
                <select required value={line.inputProductId} onChange={e => updateLine(i, 'inputProductId', e.target.value)}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="">Input product</option>
                  {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                </select>
              </div>
              <div className="col-span-4">
                <input required type="number" min="0.01" step="0.01" value={line.quantity}
                  onChange={e => updateLine(i, 'quantity', e.target.value)}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Qty" />
              </div>
              <div className="col-span-1 flex justify-center">
                {lines.length > 1 && (
                  <button type="button" onClick={() => removeLine(i)} className="text-red-400 hover:text-red-600 text-lg">✕</button>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>
      {error && <p className="text-red-600 text-sm">{error}</p>}
      <div className="flex gap-3 pt-2">
        <button type="button" onClick={onClose} className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50">Cancel</button>
        <button type="submit" disabled={saving} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg text-sm font-medium">
          {saving ? 'Saving...' : 'Save Recipe'}
        </button>
      </div>
    </form>
  );
}

// ─── Production Order Form ────────────────────────────────────────────────────
function ProductionOrderForm({ recipes, onSaved, onClose }: {
  recipes: Recipe[]; onSaved: () => void; onClose: () => void;
}) {
  const [form, setForm] = useState({
    recipeId: '',
    plannedQuantity: '',
    scheduledDate: new Date().toISOString().split('T')[0],
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const submit = async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true); setError('');
    try {
      await api.post('/api/production-orders', {
        recipeId: form.recipeId,
        plannedQuantity: parseFloat(form.plannedQuantity),
        scheduledDate: form.scheduledDate,
      });
      onSaved(); onClose();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setError(e?.response?.data?.error ?? 'Failed to create order.');
    } finally { setSaving(false); }
  };

  const f = (field: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  return (
    <form onSubmit={submit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Recipe</label>
        <select required value={form.recipeId} onChange={f('recipeId')}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
          <option value="">Select recipe</option>
          {recipes.map(r => <option key={r.id} value={r.id}>{r.name} → {r.outputProductName}</option>)}
        </select>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Planned Quantity</label>
          <input required type="number" min="0.01" step="0.01" value={form.plannedQuantity} onChange={f('plannedQuantity')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="e.g. 100" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Scheduled Date</label>
          <input required type="date" value={form.scheduledDate} onChange={f('scheduledDate')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </div>
      </div>
      {error && <p className="text-red-600 text-sm">{error}</p>}
      <div className="flex gap-3 pt-2">
        <button type="button" onClick={onClose} className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50">Cancel</button>
        <button type="submit" disabled={saving} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg text-sm font-medium">
          {saving ? 'Creating...' : 'Create Order'}
        </button>
      </div>
    </form>
  );
}

// ─── Status Badge ─────────────────────────────────────────────────────────────
const statusColor: Record<string, string> = {
  Draft: 'bg-gray-100 text-gray-600',
  InProgress: 'bg-blue-50 text-blue-700',
  Completed: 'bg-green-50 text-green-700',
  Cancelled: 'bg-red-50 text-red-700',
};

// ─── Main Page ────────────────────────────────────────────────────────────────
type Tab = 'orders' | 'recipes';

export default function ProductionPage() {
  const [tab, setTab] = useState<Tab>('orders');
  const [orders, setOrders] = useState<ProductionOrder[]>([]);
  const [recipes, setRecipes] = useState<Recipe[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState<'order' | 'recipe' | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [o, r, p] = await Promise.all([
        api.get<ProductionOrder[]>('/api/production-orders'),
        api.get<Recipe[]>('/api/Recipes'),
        api.get<Product[]>('/api/Products'),
      ]);
      setOrders(o.data);
      setRecipes(r.data);
      setProducts(p.data);
    } catch (e) { console.error(e); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  const tabs: { key: Tab; label: string }[] = [
    { key: 'orders', label: `Orders (${orders.length})` },
    { key: 'recipes', label: `Recipes (${recipes.length})` },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Production</h1>
        <div className="flex gap-2">
          {tab === 'orders' && (
            <button onClick={() => setModal('order')}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg">
              + New Order
            </button>
          )}
          {tab === 'recipes' && (
            <button onClick={() => setModal('recipe')}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg">
              + New Recipe
            </button>
          )}
        </div>
      </div>

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
        {/* Orders Tab */}
        {tab === 'orders' && (
          loading ? <div className="p-8 text-center text-gray-400 text-sm">Loading orders...</div>
            : orders.length === 0 ? <div className="p-8 text-center text-gray-400 text-sm">No production orders yet — click + New Order</div>
            : (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                    <th className="px-4 py-3">Recipe</th>
                    <th className="px-4 py-3 text-right">Planned Qty</th>
                    <th className="px-4 py-3">Scheduled</th>
                    <th className="px-4 py-3">Completed</th>
                    <th className="px-4 py-3">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {orders.map(o => (
                    <tr key={o.id} className="hover:bg-gray-50 transition-colors">
                      <td className="px-4 py-3 font-medium text-gray-900">{o.recipeName}</td>
                      <td className="px-4 py-3 text-right text-gray-700">{o.plannedQuantity}</td>
                      <td className="px-4 py-3 text-gray-500">{new Date(o.scheduledDate).toLocaleDateString()}</td>
                      <td className="px-4 py-3 text-gray-500">
                        {o.completedDate ? new Date(o.completedDate).toLocaleDateString() : '—'}
                      </td>
                      <td className="px-4 py-3">
                        <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${statusColor[o.status] ?? 'bg-gray-100 text-gray-600'}`}>
                          {o.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )
        )}

        {/* Recipes Tab */}
        {tab === 'recipes' && (
          loading ? <div className="p-8 text-center text-gray-400 text-sm">Loading recipes...</div>
            : recipes.length === 0 ? <div className="p-8 text-center text-gray-400 text-sm">No recipes yet — click + New Recipe</div>
            : (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                    <th className="px-4 py-3">Recipe Name</th>
                    <th className="px-4 py-3">Output Product</th>
                    <th className="px-4 py-3 text-right">Output Qty</th>
                    <th className="px-4 py-3 text-right">Ingredients</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {recipes.map(r => (
                    <tr key={r.id} className="hover:bg-gray-50 transition-colors">
                      <td className="px-4 py-3 font-medium text-gray-900">{r.name}</td>
                      <td className="px-4 py-3 text-gray-600">{r.outputProductName}</td>
                      <td className="px-4 py-3 text-right text-gray-700">{r.outputQuantity}</td>
                      <td className="px-4 py-3 text-right text-gray-500">{r.lines?.length ?? 0}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )
        )}
      </div>

      {modal === 'order' && (
        <Modal title="New Production Order" onClose={() => setModal(null)}>
          <ProductionOrderForm recipes={recipes} onSaved={load} onClose={() => setModal(null)} />
        </Modal>
      )}
      {modal === 'recipe' && (
        <Modal title="New Recipe" onClose={() => setModal(null)}>
          <RecipeForm products={products} onSaved={load} onClose={() => setModal(null)} />
        </Modal>
      )}
    </div>
  );
}
