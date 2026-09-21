import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { logout } from '../store/slices/authSlice';
import * as api from '../api/client';
import TopBar from '../components/Layout/TopBar.jsx';
import SubscriptionList from '../components/Lists/Subscription/SubscriptionList.jsx';
import PaymentHistoryTable from '../components/PaymentHistoryTable';
import ReminderList from '../components/Lists/Reminder/ReminderList.jsx';
import PaymentModal from '../components/Modals/PaymentModal.jsx';
import CreateSubscriptionModal from '../components/Modals/CreateSubscriptionModal.jsx';
import LoadingSpinner from '../components/Resources/LoadingSpinner.jsx';
import ErrorAlert from '../components/Alerts/ErrorAlert.jsx';

export default function DashboardPage() {
  const { customerId } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { currentCustomer } = useSelector((s) => s.auth);

  const [subscriptions, setSubscriptions] = useState([]);
  const [payments, setPayments] = useState([]);
  const [reminders, setReminders] = useState([]);
  const [periodStatuses, setPeriodStatuses] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [paymentTarget, setPaymentTarget] = useState(null);
  const [showCreateSub, setShowCreateSub] = useState(false);
  const [activeTab, setActiveTab] = useState('reminders');
  const [currentPeriod, setCurrentPeriod] = useState(() => {
    const now = new Date();
    return `${now.getUTCFullYear()}-${String(now.getUTCMonth() + 1).padStart(2, '0')}`;
  });
  const [notifyLoading, setNotifyLoading] = useState(false);

  const refreshAll = useCallback(async () => {
    try {
      setError(null);
      const [subs, pays, rems, timeInfo] = await Promise.all([
        api.fetchSubscriptionsByCustomer(customerId),
        api.fetchPaymentsByCustomer(customerId),
        api.fetchReminders(customerId),
        api.fetchCurrentTime(),
      ]);
      setSubscriptions(subs);
      setPayments(pays);
      setReminders(rems?.reminders || []);

      const backendDate = timeInfo?.currentDate
        ? new Date(timeInfo.currentDate)
        : new Date();
      // Use UTC methods consistently to avoid timezone shifting
      const period = `${backendDate.getUTCFullYear()}-${String(backendDate.getUTCMonth() + 1).padStart(2, '0')}`;
      setCurrentPeriod(period);

      // Build a lookup: subscriptionId|period → payment
      const paidLookup = {};
      for (const p of pays) {
        if (p.status === 'Successful') {
          paidLookup[`${p.subscriptionId}|${p.period}`] = p;
        }
      }

      // Helper: compute period string in UTC for a given year/month offset
      const utcPeriod = (year, month) => {
        // Handle month overflow/underflow
        const d = new Date(Date.UTC(year, month, 1));
        return `${d.getUTCFullYear()}-${String(d.getUTCMonth() + 1).padStart(2, '0')}`;
      };

      const baseYear = backendDate.getUTCFullYear();
      const baseMonth = backendDate.getUTCMonth();
      const todayMs = Date.UTC(baseYear, baseMonth, backendDate.getUTCDate());

      // For each active subscription, compute period statuses from month-3 to month+1
      const statusMap = {};
      for (const sub of subs) {
        if (sub.status !== 'Active') continue;

        const periods = [];
        for (let offset = -3; offset <= 1; offset++) {
          const prd = utcPeriod(baseYear, baseMonth + offset);
          const refDate = new Date(Date.UTC(baseYear, baseMonth + offset, 1));

          // Skip periods before subscription was created
          const subCreated = new Date(sub.createdAt);
          const subCreatedMonth = new Date(Date.UTC(subCreated.getUTCFullYear(), subCreated.getUTCMonth(), 1));
          if (refDate < subCreatedMonth) continue;

          // For next month (+1), only include if payment day is within 7 days
          if (offset === 1) {
            const daysInMonth = new Date(Date.UTC(baseYear, baseMonth + offset + 1, 0)).getUTCDate();
            const payDay = Math.min(sub.paymentDayOfMonth, daysInMonth);
            const nextDueMs = Date.UTC(refDate.getUTCFullYear(), refDate.getUTCMonth(), payDay);
            const daysUntil = Math.floor((nextDueMs - todayMs) / 86400000);
            if (daysUntil > 7) continue;
          }

          const key = `${sub.id}|${prd}`;
          const paidPayment = paidLookup[key];
          periods.push({ period: prd, paid: !!paidPayment, payment: paidPayment || null });
        }
        statusMap[sub.id] = periods;
      }
      setPeriodStatuses(statusMap);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [customerId]);

  useEffect(() => {
    if (!currentCustomer) {
      navigate('/', { replace: true });
      return;
    }
    refreshAll();
  }, [currentCustomer, refreshAll, navigate]);

  // Listen for time warp changes
  useEffect(() => {
    const handler = () => refreshAll();
    window.addEventListener('timewarp', handler);
    return () => window.removeEventListener('timewarp', handler);
  }, [refreshAll]);

  const handleLogout = () => {
    dispatch(logout());
    navigate('/');
  };

  const handlePaySuccess = () => {
    refreshAll();
  };

  const handleSendNotifications = async () => {
    try {
      setNotifyLoading(true);
      await api.sendNotifications(customerId);
      alert('📧📱 Mock notifications sent! Check the backend console for details.');
    } catch (err) {
      alert('Failed to send notifications: ' + err.message);
    } finally {
      setNotifyLoading(false);
    }
  };

  if (!currentCustomer) return null;

  const activeSubscriptions = subscriptions.filter((s) => s.status === 'Active');
  const passiveSubscriptions = subscriptions.filter((s) => s.status !== 'Active');

  const tabs = [
    { id: 'reminders', label: '🔔 Reminders', count: reminders.length },
    { id: 'subscriptions', label: '📋 Subscriptions', count: subscriptions.length },
    { id: 'payments', label: '💰 Payment History', count: payments.length },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-indigo-50">
      {/* Header */}
      <TopBar>
        <header className="border-b border-gray-200 bg-white/80 backdrop-blur-sm">
          <div className="mx-auto max-w-5xl px-4 py-3 sm:px-6">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <span className="text-2xl">🏗️</span>
                <div>
                  <h1 className="text-base font-bold text-gray-900">Archi Subscription</h1>
                  <p className="text-xs text-gray-500">Dashboard</p>
                </div>
              </div>
              <div className="flex items-center gap-4">
                <div className="text-right hidden sm:block">
                  <p className="text-sm font-medium text-gray-900">{currentCustomer.fullName}</p>
                  <p className="text-xs text-gray-500">{currentCustomer.email}</p>
                </div>
                <button
                  onClick={handleLogout}
                  className="rounded-lg border border-gray-200 px-3 py-1.5 text-xs font-medium text-gray-600 hover:bg-gray-50 transition-colors cursor-pointer"
                >
                  ← Back
                </button>
              </div>
            </div>
          </div>
        </header>
      </TopBar>

      <main className="mx-auto max-w-5xl px-4 py-6 sm:px-6">
        {error && (
          <div className="mb-4">
            <ErrorAlert message={error} />
          </div>
        )}

        {/* Customer Info Banner */}
        <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm mb-6">
          <div className="flex flex-wrap items-center gap-6">
            <div className="flex items-center justify-center h-12 w-12 rounded-full bg-indigo-100 text-xl">
              👤
            </div>
            <div className="flex-1 min-w-0">
              <h2 className="text-lg font-bold text-gray-900">{currentCustomer.fullName}</h2>
              <div className="mt-1 flex flex-wrap gap-4 text-xs text-gray-500">
                <span>📧 {currentCustomer.email}</span>
                <span>📞 {currentCustomer.phoneNumber}</span>
              </div>
            </div>
            <div className="flex gap-4 text-center">
              <div className="rounded-lg bg-indigo-50 px-4 py-2">
                <p className="text-xl font-bold text-indigo-700">{activeSubscriptions.length}</p>
                <p className="text-xs text-indigo-500">Active</p>
              </div>
              <div className="rounded-lg bg-amber-50 px-4 py-2">
                <p className="text-xl font-bold text-amber-700">{reminders.length}</p>
                <p className="text-xs text-amber-500">Unpaid</p>
              </div>
              <div className="rounded-lg bg-emerald-50 px-4 py-2">
                <p className="text-xl font-bold text-emerald-700">{payments.length}</p>
                <p className="text-xs text-emerald-500">Payments</p>
              </div>
            </div>
          </div>
        </div>

        {/* Tabs */}
        <div className="flex gap-1 rounded-xl bg-gray-100 p-1 mb-6">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex-1 rounded-lg px-3 py-2 text-sm font-medium transition-colors cursor-pointer ${
                activeTab === tab.id
                  ? 'bg-white text-gray-900 shadow-sm'
                  : 'text-gray-500 hover:text-gray-700'
              }`}
            >
              {tab.label}
              {tab.count > 0 && (
                <span className={`ml-1.5 inline-flex items-center justify-center rounded-full px-1.5 py-0.5 text-xs ${
                  activeTab === tab.id ? 'bg-indigo-100 text-indigo-700' : 'bg-gray-200 text-gray-500'
                }`}>
                  {tab.count}
                </span>
              )}
            </button>
          ))}
        </div>

        {loading ? (
          <LoadingSpinner className="py-12" />
        ) : (
          <>
            {/* Reminders Tab */}
            {activeTab === 'reminders' && (
              <section>
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-base font-semibold text-gray-900">Payment Reminders</h3>
                  <div className="flex items-center gap-3">
                    {reminders.length > 0 && (
                      <button
                        onClick={handleSendNotifications}
                        disabled={notifyLoading}
                        className="inline-flex items-center gap-1.5 rounded-lg bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-700 transition-colors cursor-pointer disabled:opacity-50"
                      >
                        {notifyLoading ? '...' : '📧 Send Notifications'}
                      </button>
                    )}
                    <button onClick={refreshAll} className="text-xs text-indigo-600 hover:text-indigo-800 transition-colors cursor-pointer">↻ Refresh</button>
                  </div>
                </div>
                <ReminderList reminders={reminders} onPayClick={setPaymentTarget} />
              </section>
            )}

            {/* Subscriptions Tab */}
            {activeTab === 'subscriptions' && (
              <section>
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-base font-semibold text-gray-900">Subscriptions</h3>
                  <div className="flex items-center gap-3">
                    <button
                      onClick={() => setShowCreateSub(true)}
                      className="inline-flex items-center gap-1.5 rounded-lg bg-indigo-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-indigo-700 transition-colors cursor-pointer"
                    >
                      ➕ Add Subscription
                    </button>
                    <button onClick={refreshAll} className="text-xs text-indigo-600 hover:text-indigo-800 transition-colors cursor-pointer">↻ Refresh</button>
                  </div>
                </div>
                {activeSubscriptions.length > 0 && (
                  <div className="mb-4">
                    <p className="text-xs font-medium text-gray-400 uppercase tracking-wider mb-2">Active ({activeSubscriptions.length})</p>
                    <SubscriptionList subscriptions={activeSubscriptions} periodStatuses={periodStatuses} onPayClick={setPaymentTarget} onMutate={refreshAll} />
                  </div>
                )}
                {passiveSubscriptions.length > 0 && (
                  <div>
                    <p className="text-xs font-medium text-gray-400 uppercase tracking-wider mb-2">Passive ({passiveSubscriptions.length})</p>
                    <SubscriptionList subscriptions={passiveSubscriptions} periodStatuses={periodStatuses} onPayClick={setPaymentTarget} onMutate={refreshAll} />
                  </div>
                )}
                {subscriptions.length === 0 && (
                  <div className="rounded-xl border border-dashed border-gray-300 bg-white p-8 text-center">
                    <p className="text-sm text-gray-500">No subscriptions yet. Add your first subscription above.</p>
                  </div>
                )}
              </section>
            )}

            {/* Payments Tab */}
            {activeTab === 'payments' && (
              <section>
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-base font-semibold text-gray-900">Payment History</h3>
                  <button onClick={refreshAll} className="text-xs text-indigo-600 hover:text-indigo-800 transition-colors cursor-pointer">↻ Refresh</button>
                </div>
                <PaymentHistoryTable payments={payments} />
              </section>
            )}
          </>
        )}
      </main>

      {paymentTarget && (
        <PaymentModal
          debtInfo={paymentTarget}
          onClose={() => setPaymentTarget(null)}
          onSuccess={refreshAll}
        />
      )}

      {showCreateSub && (
        <CreateSubscriptionModal
          customerId={customerId}
          onClose={() => setShowCreateSub(false)}
          onSuccess={refreshAll}
        />
      )}
    </div>
  );
}
