import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { loginAsCustomer } from '../../store/slices/authSlice.js';
import * as api from '../../api/client.js';
import ConfirmDialog from '../Alerts/ConfirmDialog.jsx';

export default function CustomerCard({ customer, onDeleted }) {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const [showDelete, setShowDelete] = useState(false);

  const handleLogin = () => {
    dispatch(loginAsCustomer(customer));
    navigate(`/dashboard/${customer.id}`);
  };

  const handleDelete = async () => {
    try {
      await api.deleteCustomer(customer.id);
      onDeleted?.(customer.id);
    } catch {
      // error handled silently
    }
    setShowDelete(false);
  };

  return (
    <>
      <div className="flex items-center justify-between rounded-xl border border-gray-200 bg-white p-5 shadow-sm hover:shadow-md transition-shadow">
        <div className="min-w-0 flex-1">
          <h3 className="text-base font-semibold text-gray-900 truncate">
            {customer.fullName}
          </h3>
          <p className="mt-1 text-sm text-gray-500 truncate">{customer.email}</p>
          <div className="mt-2 flex items-center gap-3 text-xs text-gray-400">
            <span>📞 {customer.phoneNumber}</span>
            <span>•</span>
            <span>{customer.activeSubscriptionCount} active subscription{customer.activeSubscriptionCount !== 1 ? 's' : ''}</span>
          </div>
        </div>
        <div className="ml-4 shrink-0 flex items-center gap-2">
          <button
            onClick={() => setShowDelete(true)}
            className="rounded-lg border border-red-200 p-2 text-red-400 hover:bg-red-50 hover:text-red-600 transition-colors cursor-pointer"
            title="Delete customer"
          >
            🗑️
          </button>
          <button
            onClick={handleLogin}
            className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 active:bg-indigo-800 transition-colors cursor-pointer"
          >
            Login
          </button>
        </div>
      </div>

      {showDelete && (
        <ConfirmDialog
          title="Delete Customer"
          message={`Are you sure you want to delete "${customer.fullName}"? All subscriptions and payments will be permanently removed.`}
          onConfirm={handleDelete}
          onCancel={() => setShowDelete(false)}
        />
      )}
    </>
  );
}
