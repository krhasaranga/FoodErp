'use client';
import { useEffect, useState } from 'react';
import api from '@/lib/api';

interface StockReport {
  productName: string;
  productSku: string;
  warehouseName: string;
  quantity: number;
  unitName: string;
  reorderLevel: number;
}

export default function StockReportPage() {
  const [data, setData] = useState<StockReport[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get<StockReport[]>('/api/Reports/stock')
      .then((r) => setData(r.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Stock Report</h1>
      <div className="bg-white rounded-xl border border-gray-200">
        {loading ? (
          <div className="p-8 text-center text-gray-400 text-sm">Loading...</div>
        ) : data.length === 0 ? (
          <div className="p-8 text-center text-gray-400 text-sm">No stock data</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
                <th className="px-4 py-3">Product</th>
                <th className="px-4 py-3">SKU</th>
                <th className="px-4 py-3">Warehouse</th>
                <th className="px-4 py-3 text-right">Qty</th>
                <th className="px-4 py-3 text-right">Reorder At</th>
                <th className="px-4 py-3">Alert</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {data.map((row, i) => (
                <tr key={i} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-900">{row.productName}</td>
                  <td className="px-4 py-3 font-mono text-gray-500">{row.productSku}</td>
                  <td className="px-4 py-3 text-gray-600">{row.warehouseName}</td>
                  <td className={`px-4 py-3 text-right font-semibold ${row.quantity <= row.reorderLevel ? 'text-red-600' : 'text-gray-900'}`}>
                    {row.quantity} {row.unitName}
                  </td>
                  <td className="px-4 py-3 text-right text-gray-500">{row.reorderLevel}</td>
                  <td className="px-4 py-3">
                    {row.quantity <= row.reorderLevel && (
                      <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-red-50 text-red-700">
                        Low Stock
                      </span>
                    )}
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
