import { useEffect, useState, useCallback } from 'react';
import * as api from '../api/client';
import TopBar from '../components/Layout/TopBar.jsx';
import CustomerCard from '../components/Cards/CustomerCard.jsx';
import CreateCustomerModal from '../components/Modals/CreateCustomerModal.jsx';
import LoadingSpinner from '../components/Resources/LoadingSpinner.jsx';
import ErrorAlert from '../components/Alerts/ErrorAlert.jsx';

export default function SeedPage() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [showCreateCustomer, setShowCreateCustomer] = useState(false);

  const loadCustomers = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await api.fetchCustomers();
      setCustomers(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadCustomers();
  }, [loadCustomers]);

  const handleSeed = async () => {
    try {
      setLoading(true);
      setError(null);
      await api.seedDatabase();
      await loadCustomers();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleClear = async () => {
    try {
      setLoading(true);
      setError(null);
      await api.clearDatabase();
      setCustomers([]);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleCustomerCreated = () => {
    loadCustomers();
  };

  const handleCustomerDeleted = (id) => {
    setCustomers((prev) => prev.filter((c) => c.id !== id));
  };

  const hasCustomers = customers.length > 0;

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-indigo-50">
      {/* Header */}
      <TopBar>
        <header className="border-b border-gray-200 bg-white/80 backdrop-blur-sm">
          <div className="mx-auto max-w-4xl px-4 py-4 sm:px-6">
            <div className="flex items-center gap-3">
              <span className="text-2xl">🏗️</span>
              <div>
                <h1 className="text-xl font-bold text-gray-900">Archi Subscription</h1>
                <p className="text-xs text-gray-500">Subscription & Payment Management System</p>
              </div>
            </div>
          </div>
        </header>
      </TopBar>

      <main className="mx-auto max-w-4xl px-4 py-8 sm:px-6">
        {/* Hero / Seed Section */}
        <section className="rounded-2xl border border-gray-200 bg-white p-8 shadow-sm text-center">
          <h2 className="text-2xl font-bold text-gray-900">
            {hasCustomers ? 'Database Ready' : 'Welcome'}
          </h2>
          <p className="mt-2 text-sm text-gray-500 max-w-md mx-auto">
            {hasCustomers
              ? 'Select a customer to log in, create a new customer, or seed more test data.'
              : 'Start by creating a customer or seeding the database with test data.'}
          </p>

          <div className="mt-6 flex items-center justify-center gap-3 flex-wrap">
            <button
              onClick={() => setShowCreateCustomer(true)}
              className="inline-flex items-center gap-2 rounded-xl bg-emerald-600 px-6 py-3 text-sm font-semibold text-white shadow-sm hover:bg-emerald-700 active:bg-emerald-800 transition-colors cursor-pointer"
            >
              <span>➕</span>
              New Customer
            </button>

            <button
              onClick={handleSeed}
              disabled={loading}
              className="inline-flex items-center gap-2 rounded-xl bg-indigo-600 px-6 py-3 text-sm font-semibold text-white shadow-sm hover:bg-indigo-700 active:bg-indigo-800 transition-colors disabled:opacity-50 cursor-pointer"
            >
              {loading ? (
                <LoadingSpinner size="sm" />
              ) : (
                <span>🌱</span>
              )}
              Seed Test Data
            </button>

            {hasCustomers && (
              <button
                onClick={handleClear}
                disabled={loading}
                className="inline-flex items-center gap-2 rounded-xl border border-red-200 bg-red-50 px-5 py-3 text-sm font-medium text-red-700 hover:bg-red-100 transition-colors disabled:opacity-50 cursor-pointer"
              >
                🗑️ Clear All
              </button>
            )}
          </div>

          {error && (
            <div className="mt-4 max-w-md mx-auto">
              <ErrorAlert message={error} />
            </div>
          )}
        </section>

        {/* Customers List */}
        {loading && !hasCustomers ? (
          <LoadingSpinner className="mt-12" />
        ) : hasCustomers ? (
          <section className="mt-8">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900">
                Customers ({customers.length})
              </h3>
              <p className="text-xs text-gray-400">Click login to view account</p>
            </div>
            <div className="grid gap-3 sm:grid-cols-2">
              {customers.map((customer) => (
                <CustomerCard
                  key={customer.id}
                  customer={customer}
                  onDeleted={handleCustomerDeleted}
                />
              ))}
            </div>
          </section>
        ) : null}
      </main>

      {/* Create Customer Modal */}
      {showCreateCustomer && (
        <CreateCustomerModal
          onClose={() => setShowCreateCustomer(false)}
          onCreated={handleCustomerCreated}
        />
      )}
    </div>
  );
}
