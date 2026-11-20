'use client';
import { useEffect, useState, useCallback } from 'react';
import api from '@/lib/api';
import Modal from '@/components/ui/Modal';
import type { Employee } from '@/types';

function EmployeeForm({ onSaved, onClose }: { onSaved: () => void; onClose: () => void }) {
  const [form, setForm] = useState({
    employeeCode: '', firstName: '', lastName: '', email: '', phone: '',
    department: '', position: '', employmentType: 'FullTime',
    hireDate: new Date().toISOString().split('T')[0], salary: '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const f = (field: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => setForm(p => ({ ...p, [field]: e.target.value }));

  const submit = async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true); setError('');
    try {
      await api.post('/api/Employees', { ...form, salary: parseFloat(form.salary) });
      onSaved(); onClose();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setError(e?.response?.data?.error ?? 'Failed to save.');
    } finally { setSaving(false); }
  };

  return (
    <form onSubmit={submit} className="space-y-4">
      <div className="grid grid-cols-3 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Employee Code</label>
          <input required value={form.employeeCode} onChange={f('employeeCode')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="EMP-001" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">First Name</label>
          <input required value={form.firstName} onChange={f('firstName')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="John" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
          <input required value={form.lastName} onChange={f('lastName')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Doe" />
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <input type="email" value={form.email} onChange={f('email')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="john@company.com" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Phone</label>
          <input value={form.phone} onChange={f('phone')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="+91 98765 43210" />
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Department</label>
          <input required value={form.department} onChange={f('department')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Production" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Position</label>
          <input required value={form.position} onChange={f('position')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Production Manager" />
        </div>
      </div>
      <div className="grid grid-cols-3 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Employment Type</label>
          <select value={form.employmentType} onChange={f('employmentType')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            {['FullTime', 'PartTime', 'Contract', 'Daily'].map(t => <option key={t}>{t}</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Hire Date</label>
          <input type="date" value={form.hireDate} onChange={f('hireDate')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Salary (₹/month)</label>
          <input required type="number" min="0" value={form.salary} onChange={f('salary')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="25000" />
        </div>
      </div>
      {error && <p className="text-red-600 text-sm">{error}</p>}
      <div className="flex gap-3 pt-2">
        <button type="button" onClick={onClose} className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50">Cancel</button>
        <button type="submit" disabled={saving} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg text-sm font-medium">{saving ? 'Saving...' : 'Save Employee'}</button>
      </div>
    </form>
  );
}

export default function EmployeesPage() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try { const r = await api.get<Employee[]>('/api/Employees'); setEmployees(r.data); }
    catch (e) { console.error(e); } finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Employees</h1>
        <button onClick={() => setModal(true)} className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg">+ New Employee</button>
      </div>
      <div className="bg-white rounded-xl border border-gray-200">
        {loading ? <div className="p-8 text-center text-gray-400 text-sm">Loading...</div>
          : employees.length === 0 ? <div className="p-8 text-center text-gray-400 text-sm">No employees yet — click + New Employee</div>
          : <table className="w-full text-sm">
            <thead><tr className="border-b border-gray-100 text-left text-gray-500 text-xs uppercase tracking-wide">
              <th className="px-4 py-3">Code</th><th className="px-4 py-3">Name</th>
              <th className="px-4 py-3">Department</th><th className="px-4 py-3">Position</th>
              <th className="px-4 py-3">Type</th><th className="px-4 py-3 text-right">Salary</th>
              <th className="px-4 py-3">Status</th>
            </tr></thead>
            <tbody className="divide-y divide-gray-50">
              {employees.map(e => (
                <tr key={e.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-mono text-gray-500">{e.employeeCode}</td>
                  <td className="px-4 py-3 font-medium text-gray-900">{e.firstName} {e.lastName}</td>
                  <td className="px-4 py-3 text-gray-600">{e.department}</td>
                  <td className="px-4 py-3 text-gray-600">{e.position}</td>
                  <td className="px-4 py-3 text-gray-600">{e.employmentType}</td>
                  <td className="px-4 py-3 text-right text-gray-700">₹{e.salary.toLocaleString()}</td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${e.isActive ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                      {e.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>}
      </div>
      {modal && <Modal title="New Employee" onClose={() => setModal(false)}><EmployeeForm onSaved={load} onClose={() => setModal(false)} /></Modal>}
    </div>
  );
}
