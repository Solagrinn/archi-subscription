import { useState } from 'react';
import * as api from '../../api/client.js';
import LoadingSpinner from '../Resources/LoadingSpinner.jsx';

const SUBSCRIPTION_TYPES = [
  { value: 1, label: 'Electricity', icon: '⚡' },
  { value: 2, label: 'Water', icon: '💧' },
  { value: 3, label: 'Internet', icon: '🌐' },
  { value: 4, label: 'GSM', icon: '📱' },
  { value: 5, label: 'NaturalGas', icon: '🔥' },
  { value: 6, label: 'Insurance', icon: '🛡️' },
  { value: 99, label: 'Other', icon: '📄' },
];

export default function CreateSubscriptionModal({ customerId, onClose, onSuccess }) {
  const [form, setForm] = useState({
    type: 3,
    serviceProvider: '',
    subscriberNumber: '',
    paymentDayOfMonth: 15,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleChange = (e) => {
    const isNumeric = e.target.type === 'number' || e.target.tagName === 'SELECT';
    const value = isNumeric ? Number(e.target.value) : e.target.value;
    setForm((prev) => ({ ...prev, [e.target.name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);

    if (!form.serviceProvider.trim() || !form.subscriberNumber.trim()) {
      setError('All fields are required.');
      return;
    }

    if (form.paymentDayOfMonth < 1 || form.paymentDayOfMonth > 28) {
      setError('Payment day must be between 1 and 28.');
      return;
    }

    try {
      setLoading(true);
      await api.createSubscription({ customerId, ...form });
      onSuccess?.();
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold text-gray-900">New Subscription</h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors cursor-pointer"
          >
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit} className="mt-4 space-y-4">
          <div>
            <label htmlFor="type" className="block text-sm font-medium text-gray-700">
              Subscription Type
            </label>
            <select
              id="type"
              name="type"
              value={form.type}
              onChange={handleChange}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 outline-none"
            >
              {SUBSCRIPTION_TYPES.map((t) => (
                <option key={t.value} value={t.value}>
                  {t.icon} {t.label}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label htmlFor="serviceProvider" className="block text-sm font-medium text-gray-700">
              Service Provider
            </label>
            <input
              id="serviceProvider"
              name="serviceProvider"
              type="text"
              value={form.serviceProvider}
              onChange={handleChange}
              placeholder="Türk Telekom"
              maxLength={200}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 outline-none"
            />
          </div>

          <div>
            <label htmlFor="subscriberNumber" className="block text-sm font-medium text-gray-700">
              Subscriber / Account Number
            </label>
            <input
              id="subscriberNumber"
              name="subscriberNumber"
              type="text"
              value={form.subscriberNumber}
              onChange={handleChange}
              placeholder="1234567890"
              maxLength={50}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 outline-none"
            />
          </div>

          <div>
            <label htmlFor="paymentDayOfMonth" className="block text-sm font-medium text-gray-700">
              Payment Day of Month (1-28)
            </label>
            <input
              id="paymentDayOfMonth"
              name="paymentDayOfMonth"
              type="number"
              min={1}
              max={28}
              value={form.paymentDayOfMonth}
              onChange={handleChange}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 outline-none"
            />
          </div>

          {error && (
            <p className="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</p>
          )}

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              disabled={loading}
              className="flex-1 rounded-lg border border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors disabled:opacity-50 cursor-pointer"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="flex-1 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-indigo-700 active:bg-indigo-800 transition-colors disabled:opacity-50 cursor-pointer"
            >
              {loading ? <LoadingSpinner size="sm" className="text-white" /> : 'Create Subscription'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
