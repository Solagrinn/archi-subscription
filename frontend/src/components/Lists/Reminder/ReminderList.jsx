import { useState } from 'react';
import * as api from '../../../api/client.js';
import SubscriptionIcon from '../../Resources/SubscriptionIcon.jsx';
import LoadingSpinner from '../../Resources/LoadingSpinner.jsx';

export default function ReminderList({ reminders, onPayClick }) {
  const [debtLoading, setDebtLoading] = useState(null); // "subId|period"

  const handlePayFlow = async (subscriptionId, period) => {
    const key = `${subscriptionId}|${period}`;
    try {
      setDebtLoading(key);
      const data = await api.fetchDebt(subscriptionId, period);
      if (data.hasDebt) {
        onPayClick?.(data);
      }
    } catch {
      // silent
    } finally {
      setDebtLoading(null);
    }
  };

  if (!reminders || reminders.length === 0) {
    return (
      <div className="rounded-xl border border-dashed border-gray-300 bg-white p-8 text-center">
        <p className="text-2xl">🎉</p>
        <p className="mt-2 text-sm font-medium text-gray-600">All caught up!</p>
        <p className="mt-1 text-xs text-gray-400">No pending payment reminders.</p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {reminders.map((r) => {
        const loadingKey = `${r.subscriptionId}|${r.period}`;
        const isLoading = debtLoading === loadingKey;
        return (
          <div
            key={`${r.subscriptionId}-${r.period}`}
            className="flex items-start gap-4 rounded-xl border border-amber-200 bg-amber-50 p-4"
          >
            <div className="mt-0.5">
              <SubscriptionIcon type={r.subscriptionType} />
            </div>
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2">
                <span className="text-sm font-semibold text-gray-900">{r.serviceProvider}</span>
                <span className="text-xs text-gray-400">#{r.subscriberNumber}</span>
              </div>
              <p className="mt-1 text-sm text-amber-800">{r.message}</p>
              <div className="mt-2 flex flex-wrap gap-3 text-xs text-gray-500">
                <span>📅 Due day: {r.paymentDayOfMonth}</span>
                <span>📆 Period: {r.period}</span>
              </div>
              <div className="mt-1 flex flex-wrap gap-3 text-xs text-gray-400">
                <span>📧 {r.customerEmail}</span>
                <span>📞 {r.customerPhone}</span>
              </div>
            </div>
            {onPayClick && (
              <div className="shrink-0 self-center">
                <button
                  onClick={() => handlePayFlow(r.subscriptionId, r.period)}
                  disabled={isLoading}
                  className="rounded-lg bg-indigo-600 px-4 py-2 text-xs font-medium text-white hover:bg-indigo-700 active:bg-indigo-800 transition-colors disabled:opacity-50 cursor-pointer"
                >
                  {isLoading ? <LoadingSpinner size="sm" /> : '💳 Pay'}
                </button>
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
