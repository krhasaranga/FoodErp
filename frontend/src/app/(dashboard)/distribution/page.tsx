'use client';
import { useEffect, useState, useCallback } from 'react';
import api from '@/lib/api';
import Modal from '@/components/ui/Modal';
import type { Customer, SalesOrder, Product } from '@/types';

function CustomerForm({ onSaved, onClose }: { onSaved: () => void; onClose: () => void }) {
  const [form, setForm] = useState({ name: '', contactPerson: '', email: '', phone: '', address: '', customerType: 'Retail' });
  const [saving, setSaving] = useState(false);
  const f = (field: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => setForm(p => ({ ...p, [field]: e.target.value }));

  const submit = async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true);
    try { await api.post('/api/Customers', form); onSaved(); onClose(); }
    catch { } finally { setSaving(false); }
  };

  return (
    <form onSubmit={submit} className="space-y-4">
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Company Name</label>
          <input required value={form.name} onChange={f('name')} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Acme Retail" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
          <select value={form.customerType} onChange={f('customerType')} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            {['Retail', 'Wholesale', 'Distributor', 'HoReCa'].map(t => <option key={t}>{t}</option>)}
          </select>
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Contact Person</label>
          <input value={form.contactPerson} onChange={f('contactPerson')} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="John Doe" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <input type="email" value={form.email} onChange={f('email')} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="john@acme.com" />
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Phone</label>
          <input value={form.phone} onChange={f('phone')} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="+91 98765 43210" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Address</label>
          <input value={form.address} onChange={f('address')} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="City, State" />
        </div>
      </div>
      <div className="flex gap-3 pt-2">
        <button type="button" onClick={onClose} className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50">Cancel</button>
        <button type="submit" disabled={saving} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg text-sm font-medium">{saving ? 'Saving...' : 'Save Customer'}</button>
      </div>
    </form>
  );
}

function SalesOrderForm({ customers, products, onSaved, onClose }: {
  customers: Customer[]; products: Product[]; onSaved: () => void; onClose: () => void;
}) {
  const [customerId, setCustomerId] = useState('');
  const [orderDate, setOrderDate] = useState(new Date().toISOString().split('T')[0]);
  const [lines, setLines] = useState([{ productId: '', quantity: '', unitPrice: '' }]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const addLine = () => setLines(l => [...l, { productId: '', quantity: '', unitPrice: '' }]);
  const removeLine = (i: number) => setLines(l => l.filter((_, idx) => idx !== i));
  const updateLine = (i: number, field: string, value: string) =>
    setLines(l => l.map((line, idx) => idx === i ? { ...line, [field]: value } : line));

  const handleProductChange = (i: number, productId: string) => {
    const product = products.find(p => p.id === productId);
    updateLine(i, 'productId', productId);
    if (product) setLines(l => l.map((line, idx) => idx === i ? { ...line, productId, unitPrice: product.sellingPrice.toString() } : line));
  };

  const submit = async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true); setError('');
    try {
      await api.post('/api/sales-orders', {
        customerId, orderDate,
        lines: lines.map(l => ({ productId: l.productId, quantity: parseFloat(l.quantity), unitPrice: parseFloat(l.unitPrice) })),
      });
      onSaved(); onClose();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setError(e?.response?.data?.error ?? 'Failed to create order.');
    } finally { setSaving(false); }
  };

  return (
    <form onSubmit={submit} className="space-y-4">
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Customer</label>
          <select required value={customerId} onChange={e => setCustomerId(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            <option value="">Select customer</option>
            {customers.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Order Date</label>
          <input type="date" value={orderDate} onChange={e => setOrderDate(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </div>
      </div>
      <div>
        <div className="flex items-center justify-between mb-2">
          <label className="text-sm font-medium text-gray-700">Order Lines</label>
          <button type="button" onClick={addLine} className="text-xs text-blue-600 hover:underline">+ Add Line</button>
        </div>
        <div className="space-y-2">
          {lines.map((line, i) => (
            <div key={i} className="grid grid-cols-12 gap-2 items-center">
              <div className="col-span-5">
                <select required value={line.productId} onChange={e => handleProductChange(i, e.target.value)}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="">Product</option>
                  {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                </select>
              </div>
              <div className="col-span-3">
                <input required type="number" min="0.01" step="0.01" value={line.quantity}
                  onChange={e => updateLine(i, 'quantity', e.target.value)}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Qty" />
              </div>
              <div className="col-span-3">
                <input required type="number" min="0" step="0.01" value={line.unitPrice}
                  onChange={e => updateLine(i, 'unitPrice', e.target.value)}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Price ₹" />
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
        <button type="submit" disabled={saving} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg text-sm font-medium">{saving ? 'Creating...' : 'Create Order'}</button>
      </div>
    </form>
  );
}

const statusColor: Record<string, string> = {
  Draft: 'bg-gray-100 text-gray-600', Confirmed: 'bg-blue-50 text-blue-700',
  Dispatched: 'bg-amber-50 text-amber-700', Delivered: 'bg-green-50 text-green-700',
  Cancelled: 'bg-red-50 text-red-700',
};

type Tab = 'orders' | 'customers';
export default function DistributionPage() {
  const [tab, setTab] = useState<Tab>('orders');
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [orders, setOrders] = useState<SalesOrder[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState<'customer' | 'order' | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [c, o, p] = await Promise.all([
        api.get<Customer[]>('/api/Customers'),
        api.get<SalesOrder[]>('/api/sales-orders'),
        api.get<Product[]>('/api/Products'),
      ]);
      setCustomers(c.data); setOrders(o.data); setProducts(p.data);
    } catch (e) { console.error(e); } finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Distribution</h1>
        <div className="flex gap-2">
          {tab === 'orders' && <button onClick={() => setModal('order')} className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg">+ New Order</button>}
          {tab === 'customers' && <button onClick={() => setModal('customer')} className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg">+ New Customer</button>}
        </div>
      </div>
      <div className="flex gap-1 mb-4 bg-gray-100 rounded-lg p-1 w-fit">
        {(['orders', 'customers'] as Tab[]).map(t => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-4 py-1.5 rounded-md text-sm font-medium transition-colors ${tab === t ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}>
            {t === 'orders' ? `Orders (${orders.length})` : `Customers (${customers.length})`}
          </button>
        ))}
      </div>
      <div className="bg-white rounded-xl border border-gray-200">
        {tab === 'orders' && (
          loading ? <div className="p-8 text-center text-gray-400 text-sm">Loading...</div>
            : orders.length === 0 ? <div className="p-8 text-center text-gray-400 text-sm">No orders yet — click + New Order</div>
            : <table className="w-full text-sm">
              <thead><tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                <th className="px-4 py-3">Customer</th><th className="px-4 py-3">Date</th>
                <th className="px-4 py-3 text-right">Total</th><th className="px-4 py-3">Status</th>
              </tr></thead>
              <tbody className="divide-y divide-gray-50">
                {orders.map(o => (
                  <tr key={o.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{o.customerName}</td>
                    <td className="px-4 py-3 text-gray-500">{new Date(o.orderDate).toLocaleDateString()}</td>
                    <td className="px-4 py-3 text-right font-medium">₹{o.totalAmount.toFixed(2)}</td>
                    <td className="px-4 py-3"><span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${statusColor[o.status] ?? 'bg-gray-100 text-gray-600'}`}>{o.status}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
        )}
        {tab === 'customers' && (
          loading ? <div className="p-8 text-center text-gray-400 text-sm">Loading...</div>
            : customers.length === 0 ? <div className="p-8 text-center text-gray-400 text-sm">No customers yet</div>
            : <table className="w-full text-sm">
              <thead><tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                <th className="px-4 py-3">Name</th><th className="px-4 py-3">Contact</th>
                <th className="px-4 py-3">Email</th><th className="px-4 py-3">Phone</th><th className="px-4 py-3">Type</th>
              </tr></thead>
              <tbody className="divide-y divide-gray-50">
                {customers.map(c => (
                  <tr key={c.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{c.name}</td>
                    <td className="px-4 py-3 text-gray-600">{c.contactPerson}</td>
                    <td className="px-4 py-3 text-gray-600">{c.email}</td>
                    <td className="px-4 py-3 text-gray-600">{c.phone}</td>
                    <td className="px-4 py-3"><span className="inline-flex px-2 py-0.5 rounded-full text-xs font-medium bg-blue-50 text-blue-700">{c.customerType}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
        )}
      </div>
      {modal === 'customer' && <Modal title="New Customer" onClose={() => setModal(null)}><CustomerForm onSaved={load} onClose={() => setModal(null)} /></Modal>}
      {modal === 'order' && <Modal title="New Sales Order" onClose={() => setModal(null)}><SalesOrderForm customers={customers} products={products} onSaved={load} onClose={() => setModal(null)} /></Modal>}
    </div>
  );
}
