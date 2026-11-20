'use client';
import { useEffect, useState } from 'react';
import api from '@/lib/api';
import type { DashboardReport } from '@/types';

function StatCard({ label, value, sub, color }: {
  label: string; value: string | number; sub?: string; color: string;
}) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5">
      <p className="text-sm text-gray-500">{label}</p>
      <p className={`text-3xl font-bold mt-1 ${color}`}>{value}</p>
      {sub && <p className="text-xs text-gray-400 mt-1">{sub}</p>}
    </div>
  );
}

export default function DashboardPage() {
  const [data, setData] = useState<DashboardReport | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get<DashboardReport>('/api/Reports/dashboard')
      .then((r) => setData(r.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const fmt = (n: number) =>
    new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(n);

  if (loading) {
    return <div className="text-gray-400 text-sm">Loading dashboard...</div>;
  }

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Dashboard</h1>

      {/* Row 1 — Overview */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-4">
        <StatCard label="Products" value={data?.totalProducts ?? 0} color="text-gray-900" />
        <StatCard label="Customers" value={data?.totalCustomers ?? 0} color="text-gray-900" />
        <StatCard label="Employees" value={data?.totalEmployees ?? 0} color="text-gray-900" />
        <StatCard label="Warehouses" value={data?.totalWarehouses ?? 0} color="text-gray-900" />
      </div>

      {/* Row 2 — Financial */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-4">
        <StatCard
          label="Month Sales"
          value={fmt(data?.monthSalesAmount ?? 0)}
          sub={`${data?.monthInvoicesCount ?? 0} invoices`}
          color="text-green-600"
        />
        <StatCard
          label="Overdue Invoices"
          value={data?.overdueInvoicesCount ?? 0}
          sub={fmt(data?.overdueInvoicesAmount ?? 0)}
          color={(data?.overdueInvoicesCount ?? 0) > 0 ? 'text-red-600' : 'text-gray-900'}
        />
        <StatCard
          label="Pending Production"
          value={data?.pendingProductionOrders ?? 0}
          sub="orders in progress"
          color="text-blue-600"
        />
        <StatCard
          label="Low Stock Items"
          value={data?.lowStockProducts ?? 0}
          sub="below reorder level"
          color={(data?.lowStockProducts ?? 0) > 0 ? 'text-amber-600' : 'text-gray-900'}
        />
      </div>
    </div>
  );
}
