'use client';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { auth } from '@/lib/auth';

const nav = [
  { href: '/dashboard',      label: 'Dashboard',    icon: '▦' },
  { href: '/products',       label: 'Products',     icon: '📦' },
  { href: '/inventory',      label: 'Inventory',    icon: '🏭' },
  { href: '/production',     label: 'Production',   icon: '⚙️' },
  { href: '/distribution',   label: 'Distribution', icon: '🚚' },
  { href: '/invoicing',      label: 'Invoicing',    icon: '🧾' },
  { href: '/employees',      label: 'Employees',    icon: '👥' },
  { href: '/reports',        label: 'Reports',      icon: '📊' },
];

export default function Sidebar() {
  const pathname = usePathname();
  const router = useRouter();

  const logout = () => {
    auth.clearToken();
    router.push('/login');
  };

  return (
    <aside className="w-60 min-h-screen bg-gray-900 flex flex-col">
      {/* Logo */}
      <div className="px-6 py-5 border-b border-gray-700">
        <span className="text-white font-bold text-lg tracking-tight">🍽 FoodERP</span>
      </div>

      {/* Nav */}
      <nav className="flex-1 px-3 py-4 space-y-0.5">
        {nav.map(({ href, label, icon }) => {
          const active = pathname === href || (href !== '/dashboard' && pathname.startsWith(href));
          return (
            <Link
              key={href}
              href={href}
              className={`flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors ${
                active
                  ? 'bg-blue-600 text-white font-medium'
                  : 'text-gray-400 hover:bg-gray-800 hover:text-white'
              }`}
            >
              <span className="text-base">{icon}</span>
              {label}
            </Link>
          );
        })}
      </nav>

      {/* Logout */}
      <div className="px-3 py-4 border-t border-gray-700">
        <button
          onClick={logout}
          className="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-gray-400 hover:bg-gray-800 hover:text-white transition-colors w-full"
        >
          <span>🚪</span> Sign out
        </button>
      </div>
    </aside>
  );
}
