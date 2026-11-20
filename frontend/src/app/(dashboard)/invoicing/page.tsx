'use client';
import { useEffect, useState } from 'react';
import api from '@/lib/api';
import type { Invoice } from '@/types';

const statusColor: Record<string, string> = {
  Draft: 'bg-gray-100 text-gray-600',
  Issued: 'bg-blue-50 text-blue-700',
  PartiallyPaid: 'bg-amber-50 text-amber-700',
  Paid: 'bg-green-50 text-green-700',
  Cancelled: 'bg-red-50 text-red-700',
};

export default function InvoicingPage() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get<Invoice[]>('/api/Invoices')
      .then((r) => setInvoices(r.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const fmt = (n: number) => `₹${n.toFixed(2)}`;

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Invoices</h1>
      </div>

      <div className="bg-white rounded-xl border border-gray-200">
        {loading ? (
          <div className="p-8 text-center text-gray-400 text-sm">Loading invoices...</div>
        ) : invoices.length === 0 ? (
          <div className="p-8 text-center text-gray-400 text-sm">No invoices yet</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                <th className="px-4 py-3">Invoice #</th>
                <th className="px-4 py-3">Customer</th>
                <th className="px-4 py-3">Date</th>
                <th className="px-4 py-3">Due Date</th>
                <th className="px-4 py-3 text-right">Total</th>
                <th className="px-4 py-3 text-right">Balance Due</th>
                <th className="px-4 py-3">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {invoices.map((inv) => (
                <tr key={inv.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-3 font-mono font-medium text-gray-900">{inv.invoiceNumber}</td>
                  <td className="px-4 py-3 text-gray-700">{inv.customerName}</td>
                  <td className="px-4 py-3 text-gray-500">{new Date(inv.invoiceDate).toLocaleDateString()}</td>
                  <td className={`px-4 py-3 ${inv.isOverdue ? 'text-red-600 font-medium' : 'text-gray-500'}`}>
                    {new Date(inv.dueDate).toLocaleDateString()}
                    {inv.isOverdue && ' ⚠️'}
                  </td>
                  <td className="px-4 py-3 text-right text-gray-700">{fmt(inv.totalAmount)}</td>
                  <td className="px-4 py-3 text-right font-semibold text-gray-900">{fmt(inv.balanceDue)}</td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${statusColor[inv.status] ?? 'bg-gray-100 text-gray-600'}`}>
                      {inv.status}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
