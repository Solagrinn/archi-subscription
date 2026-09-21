import { useState } from 'react';
import * as api from '../../../api/client.js';
import StatusBadge from '../../Resources/StatusBadge.jsx';
import SubscriptionIcon from '../../Resources/SubscriptionIcon.jsx';
import LoadingSpinner from '../../Resources/LoadingSpinner.jsx';
import ConfirmDialog from '../../Alerts/ConfirmDialog.jsx';
import EditSubscriptionModal from '../../Modals/EditSubscriptionModal.jsx';
import PaymentHistoryTable from '../../PaymentHistoryTable.jsx';

export default function SubscriptionList({ subscriptions, periodStatuses, onPayClick, onMutate }) {
  const [debtLoading, setDebtLoading] = useState(null); // "subId|period"
  const [subPayments, setSubPayments] = useState([]);
  const [activeHistoryId, setActiveHistoryId] = useState(null);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [editTarget, setEditTarget] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const handlePayFlow = async (subscriptionId, period) => {
    const key = `${subscriptionId}|${period}`;
    try {
      setDebtLoading(key);
      const data = await api.fetchDebt(subscriptionId, period);
      if (data.hasDebt) {
        onPayClick(data);
      }
    } catch {
      // silent
    } finally {
      setDebtLoading(null);
    }
  };

  const handleViewPayments = async (subscriptionId) => {
    if (activeHistoryId === subscriptionId) {
      setActiveHistoryId(null);
      setSubPayments([]);
      return;
    }
    try {
      setHistoryLoading(true);
      setActiveHistoryId(subscriptionId);
      const data = await api.fetchPaymentsBySubscription(subscriptionId);
      setSubPayments(data);
    } catch {
      setSubPayments([]);
    } finally {
      setHistoryLoading(false);
    }
  };

  const handleDelete = async () => {
    if (deleteTarget) {
      try {
        await api.deleteSubscription(deleteTarget.id);
        onMutate?.();
      } catch { /* silent */ }
      setDeleteTarget(null);
    }
  };

  if (subscriptions.length === 0) {
    return (
      <div className="rounded-xl border border-dashed border-gray-300 bg-white p-8 text-center">
        <p className="text-sm text-gray-500">No subscriptions found.</p>
      </div>
    );
  }

  return (
    <>
      <div className="space-y-3">
        {subscriptions.map((sub) => {
          const isActive = sub.status === 'Active';
          const periods = periodStatuses?.[sub.id] || [];
          const unpaidPeriods = periods.filter((p) => !p.paid);
          const paidPeriods = periods.filter((p) => p.paid);
          const allPaid = periods.length > 0 && unpaidPeriods.length === 0;

          return (
            <div
              key={sub.id}
              className={`rounded-xl border bg-white p-4 transition-shadow hover:shadow-sm ${
                isActive ? 'border-gray-200' : 'border-gray-100 opacity-60'
              }`}
            >
              <div className="flex items-start justify-between gap-3">
                <div className="flex items-start gap-3 min-w-0 flex-1">
                  <div className="mt-0.5">
                    <SubscriptionIcon type={sub.type} />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className="text-sm font-semibold text-gray-900">{sub.serviceProvider}</span>
                      <StatusBadge status={sub.status} />
                    </div>
                    <p className="mt-1 text-xs text-gray-500">{sub.type} · #{sub.subscriberNumber}</p>
                    <p className="mt-0.5 text-xs text-gray-400">Payment day: {sub.paymentDayOfMonth} of each month</p>
                  </div>
                </div>

                <div className="shrink-0 flex items-center gap-1.5">
                  <button
                    onClick={() => handleViewPayments(sub.id)}
                    className={`rounded-lg border px-2.5 py-1.5 text-xs font-medium transition-colors cursor-pointer ${
                      activeHistoryId === sub.id
                        ? 'border-emerald-300 bg-emerald-50 text-emerald-700'
                        : 'border-gray-200 text-gray-400 hover:bg-gray-50 hover:text-gray-600'
                    }`}
                    title="View payment history"
                  >
                    📜
                  </button>
                  <button onClick={() => setEditTarget(sub)} className="rounded-lg border border-gray-200 p-1.5 text-xs text-gray-400 hover:bg-gray-50 hover:text-gray-600 transition-colors cursor-pointer" title="Edit">✏️</button>
                  <button onClick={() => setDeleteTarget(sub)} className="rounded-lg border border-red-200 p-1.5 text-xs text-red-400 hover:bg-red-50 hover:text-red-600 transition-colors cursor-pointer" title="Delete">🗑️</button>
                </div>
              </div>

              {/* Multi-period payment status */}
              {isActive && periods.length > 0 && (
                <div className="mt-3 space-y-1.5">
                  {/* All paid — single green bar */}
                  {allPaid && (
                    <div className="rounded-lg bg-emerald-50 border border-emerald-100 p-3 flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-emerald-700">✅ All payments up to date</p>
                        <p className="text-xs text-emerald-600 mt-0.5">
                          {paidPeriods.map((p) => p.period).join(', ')}
                        </p>
                      </div>
                      <span className="rounded-full bg-emerald-100 px-3 py-1 text-xs font-semibold text-emerald-800">Paid</span>
                    </div>
                  )}

                  {/* Unpaid periods — one row each with Pay button */}
                  {unpaidPeriods.map((p) => {
                    const loadingKey = `${sub.id}|${p.period}`;
                    const isLoading = debtLoading === loadingKey;
                    return (
                      <div
                        key={p.period}
                        className="rounded-lg bg-amber-50 border border-amber-100 p-3 flex items-center justify-between"
                      >
                        <div>
                          <p className="text-sm font-medium text-amber-800">⚠️ Unpaid — {p.period}</p>
                          <p className="text-xs text-amber-600 mt-0.5">Due on the {sub.paymentDayOfMonth}th</p>
                        </div>
                        <button
                          onClick={() => handlePayFlow(sub.id, p.period)}
                          disabled={isLoading}
                          className="rounded-lg bg-indigo-600 px-4 py-2 text-xs font-medium text-white hover:bg-indigo-700 active:bg-indigo-800 transition-colors disabled:opacity-50 cursor-pointer"
                        >
                          {isLoading ? <LoadingSpinner size="sm" /> : '💳 Pay'}
                        </button>
                      </div>
                    );
                  })}

                  {/* Paid periods summary (only show when there are also unpaid) */}
                  {!allPaid && paidPeriods.length > 0 && (
                    <div className="rounded-lg bg-emerald-50/60 border border-emerald-100/60 p-2.5 flex items-center gap-2 flex-wrap">
                      <span className="text-xs text-emerald-700">✅ Paid:</span>
                      {paidPeriods.map((p) => (
                        <span key={p.period} className="text-xs text-emerald-700">
                          <span className="font-medium">{p.period}</span>
                          <span className="text-emerald-500 ml-0.5">(₺{p.payment.amount.toFixed(2)})</span>
                        </span>
                      ))}
                    </div>
                  )}
                </div>
              )}

              {/* Subscription payment history */}
              {activeHistoryId === sub.id && (
                <div className="mt-3 rounded-lg border border-gray-100 bg-gray-50/50 p-3">
                  <p className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-2">Payment History for {sub.serviceProvider}</p>
                  {historyLoading ? <LoadingSpinner className="py-4" size="sm" /> : <PaymentHistoryTable payments={subPayments} />}
                </div>
              )}
            </div>
          );
        })}
      </div>

      {editTarget && (
        <EditSubscriptionModal subscription={editTarget} onClose={() => setEditTarget(null)} onSuccess={onMutate} />
      )}

      {deleteTarget && (
        <ConfirmDialog
          title="Delete Subscription"
          message={`Are you sure you want to delete the ${deleteTarget.type} subscription for "${deleteTarget.serviceProvider}"?`}
          onConfirm={handleDelete}
          onCancel={() => setDeleteTarget(null)}
        />
      )}
    </>
  );
}
