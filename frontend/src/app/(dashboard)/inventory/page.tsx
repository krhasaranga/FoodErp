'use client';
import { useEffect, useState, useCallback } from 'react';
import api from '@/lib/api';
import Modal from '@/components/ui/Modal';
import type { StockBalance, Warehouse, Product } from '@/types';

function WarehouseForm({ onSaved, onClose }: { onSaved: () => void; onClose: () => void }) {
  const [form, setForm] = useState({ name: '', location: '' });
  const [saving, setSaving] = useState(false);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try { await api.post('/api/Warehouses', form); onSaved(); onClose(); }
    catch { } finally { setSaving(false); }
  };

  return (
    <form onSubmit={submit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Warehouse Name</label>
        <input required value={form.name} onChange={e => setForm({ ...form, name: e.target.value })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="e.g. Main Warehouse" />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Location</label>
        <input value={form.location} onChange={e => setForm({ ...form, location: e.target.value })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="e.g. Building A, Floor 2" />
      </div>
      <div className="flex gap-3 pt-2">
        <button type="button" onClick={onClose} className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50">Cancel</button>
        <button type="submit" disabled={saving} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg text-sm font-medium">
          {saving ? 'Saving...' : 'Save'}
        </button>
      </div>
    </form>
  );
}

function StockMovementForm({ warehouses, products, onSaved, onClose }: {
  warehouses: Warehouse[]; products: Product[];
  onSaved: () => void; onClose: () => void;
}) {
  const [form, setForm] = useState({
    productId: '', warehouseId: '', toWarehouseId: '', movementType: 'Receive',
    quantity: '', notes: '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      const qty = parseFloat(form.quantity);
      if (form.movementType === 'Receive') {
        await api.post('/api/Stock/receive', { warehouseId: form.warehouseId, productId: form.productId, quantity: qty, notes: form.notes });
      } else if (form.movementType === 'Issue') {
        await api.post('/api/Stock/issue', { warehouseId: form.warehouseId, productId: form.productId, quantity: qty, notes: form.notes });
      } else if (form.movementType === 'Adjust') {
        await api.post('/api/Stock/adjust', { warehouseId: form.warehouseId, productId: form.productId, newQuantity: qty, notes: form.notes });
      } else if (form.movementType === 'Transfer') {
        await api.post('/api/Stock/transfer', { fromWarehouseId: form.warehouseId, toWarehouseId: form.toWarehouseId, productId: form.productId, quantity: qty, notes: form.notes });
      }
      onSaved();
      onClose();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setError(e?.response?.data?.error ?? 'Failed to record movement.');
    } finally {
      setSaving(false);
    }
  };

  const f = (field: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  const movementTypes = ['Receive', 'Issue', 'Adjust', 'Transfer'];

  return (
    <form onSubmit={submit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Product</label>
        <select required value={form.productId} onChange={f('productId')}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
          <option value="">Select product</option>
          {products.map(p => <option key={p.id} value={p.id}>{p.name} ({p.sku})</option>)}
        </select>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Warehouse</label>
          <select required value={form.warehouseId} onChange={f('warehouseId')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            <option value="">Select warehouse</option>
            {warehouses.map(w => <option key={w.id} value={w.id}>{w.name}</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Movement Type</label>
          <select value={form.movementType} onChange={f('movementType')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            {movementTypes.map(t => <option key={t} value={t}>{t}</option>)}
          </select>
        </div>
      </div>
      {form.movementType === 'Transfer' && (
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">To Warehouse</label>
          <select required value={form.toWarehouseId} onChange={f('toWarehouseId')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            <option value="">Select destination</option>
            {warehouses.filter(w => w.id !== form.warehouseId).map(w => <option key={w.id} value={w.id}>{w.name}</option>)}
          </select>
        </div>
      )}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          {form.movementType === 'Adjust' ? 'New Quantity (target balance)' : 'Quantity'}
        </label>
        <input required type="number" min="0.01" step="0.01" value={form.quantity} onChange={f('quantity')}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="0.00" />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
        <input value={form.notes} onChange={f('notes')}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Optional reference or note" />
      </div>
      {error && <p className="text-red-600 text-sm">{error}</p>}
      <div className="flex gap-3 pt-2">
        <button type="button" onClick={onClose} className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50">Cancel</button>
        <button type="submit" disabled={saving} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg text-sm font-medium">
          {saving ? 'Recording...' : 'Record Movement'}
        </button>
      </div>
    </form>
  );
}

type Tab = 'stock' | 'warehouses';

export default function InventoryPage() {
  const [tab, setTab] = useState<Tab>('stock');
  const [stock, setStock] = useState<StockBalance[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [selectedWarehouse, setSelectedWarehouse] = useState('');
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState<'warehouse' | 'movement' | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [w, p] = await Promise.all([
        api.get<Warehouse[]>('/api/Warehouses'),
        api.get<Product[]>('/api/Products'),
      ]);
      const warehouseList = w.data;
      setWarehouses(warehouseList);
      setProducts(p.data);
      // Fetch stock balances per warehouse and merge
      const stockArrays = await Promise.all(
        warehouseList.map(wh => api.get<StockBalance[]>(`/api/Stock/balances/warehouse/${wh.id}`).then(r => r.data).catch(() => []))
      );
      setStock(stockArrays.flat());
    } catch (e) { console.error(e); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  const filtered = selectedWarehouse ? stock.filter(s => s.warehouseId === selectedWarehouse) : stock;

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Inventory</h1>
        <div className="flex gap-2">
          {tab === 'stock' && (
            <button onClick={() => setModal('movement')}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg">
              + Stock Movement
            </button>
          )}
          {tab === 'warehouses' && (
            <button onClick={() => setModal('warehouse')}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg">
              + New Warehouse
            </button>
          )}
        </div>
      </div>

      <div className="flex gap-1 mb-4 bg-gray-100 rounded-lg p-1 w-fit">
        {(['stock', 'warehouses'] as Tab[]).map(t => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-4 py-1.5 rounded-md text-sm font-medium transition-colors capitalize ${tab === t ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}>
            {t === 'stock' ? `Stock (${stock.length})` : `Warehouses (${warehouses.length})`}
          </button>
        ))}
      </div>

      <div className="bg-white rounded-xl border border-gray-200">
        {tab === 'stock' && (
          <>
            <div className="p-4 border-b border-gray-100">
              <select value={selectedWarehouse} onChange={e => setSelectedWarehouse(e.target.value)}
                className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                <option value="">All Warehouses</option>
                {warehouses.map(w => <option key={w.id} value={w.id}>{w.name}</option>)}
              </select>
            </div>
            {loading ? <div className="p-8 text-center text-gray-400 text-sm">Loading...</div>
              : filtered.length === 0 ? <div className="p-8 text-center text-gray-400 text-sm">No stock — record a movement first</div>
              : (
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                      <th className="px-4 py-3">Product</th><th className="px-4 py-3">SKU</th>
                      <th className="px-4 py-3">Warehouse</th>
                      <th className="px-4 py-3 text-right">Qty</th><th className="px-4 py-3">Unit</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-50">
                    {filtered.map((s, i) => (
                      <tr key={i} className="hover:bg-gray-50">
                        <td className="px-4 py-3 font-medium text-gray-900">{s.productName}</td>
                        <td className="px-4 py-3 font-mono text-gray-500">{s.productCode}</td>
                        <td className="px-4 py-3 text-gray-600">{s.warehouseName}</td>
                        <td className={`px-4 py-3 text-right font-semibold ${s.quantity <= 0 ? 'text-red-600' : 'text-gray-900'}`}>{s.quantity}</td>
                        <td className="px-4 py-3 text-gray-500">{s.unitSymbol}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
          </>
        )}
        {tab === 'warehouses' && (
          loading ? <div className="p-8 text-center text-gray-400 text-sm">Loading...</div>
            : warehouses.length === 0 ? <div className="p-8 text-center text-gray-400 text-sm">No warehouses yet</div>
            : (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                    <th className="px-4 py-3">Name</th><th className="px-4 py-3">Location</th><th className="px-4 py-3">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {warehouses.map(w => (
                    <tr key={w.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 font-medium text-gray-900">{w.name}</td>
                      <td className="px-4 py-3 text-gray-600">{w.location || '—'}</td>
                      <td className="px-4 py-3">
                        <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${w.isActive ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                          {w.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )
        )}
      </div>

      {modal === 'warehouse' && (
        <Modal title="New Warehouse" onClose={() => setModal(null)}>
          <WarehouseForm onSaved={load} onClose={() => setModal(null)} />
        </Modal>
      )}
      {modal === 'movement' && (
        <Modal title="Record Stock Movement" onClose={() => setModal(null)}>
          <StockMovementForm warehouses={warehouses} products={products} onSaved={load} onClose={() => setModal(null)} />
        </Modal>
      )}
    </div>
  );
}
