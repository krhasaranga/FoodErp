'use client';
import Link from 'next/link';

const reports = [
  { href: '/reports/stock',       label: 'Stock Report',         desc: 'Current stock levels across all warehouses', icon: '📦' },
  { href: '/reports/sales',       label: 'Sales Report',         desc: 'Sales orders and revenue summary',           icon: '💰' },
  { href: '/reports/production',  label: 'Production Report',    desc: 'Production orders and output summary',       icon: '⚙️' },
  { href: '/reports/receivables', label: 'Receivables Report',   desc: 'Outstanding invoices and aging analysis',    icon: '🧾' },
  { href: '/reports/payments',    label: 'Payments Report',      desc: 'Payments received and history',              icon: '💳' },
];

export default function ReportsPage() {
  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Reports</h1>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {reports.map((r) => (
          <Link
            key={r.href}
            href={r.href}
            className="bg-white rounded-xl border border-gray-200 p-5 hover:border-blue-300 hover:shadow-sm transition-all group"
          >
            <div className="flex items-start gap-4">
              <span className="text-2xl">{r.icon}</span>
              <div>
                <p className="font-semibold text-gray-900 group-hover:text-blue-600 transition-colors">{r.label}</p>
                <p className="text-sm text-gray-500 mt-0.5">{r.desc}</p>
              </div>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
