import { useState } from 'react';
import * as api from '../../api/client.js';
import LoadingSpinner from '../Resources/LoadingSpinner.jsx';

export default function PaymentModal({ debtInfo, onClose, onSuccess }) {
  const [amount, setAmount] = useState(debtInfo.debtAmount);
  const [loading, setLoading] = useState(false);
  const [paymentResult, setPaymentResult] = useState(null);
  const [error, setError] = useState(null);

  const handlePay = async () => {
    try {
      setError(null);
      setLoading(true);
      const result = await api.createPayment({
        subscriptionId: debtInfo.subscriptionId,
        amount: Number(amount),
        period: debtInfo.period,
      });
      setPaymentResult(result);
      await onSuccess?.();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // Show result screen after payment
  if (paymentResult) {
    const isSuccess = paymentResult.status === 'Successful';
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
        <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl text-center">
          <div className="text-4xl mb-3">{isSuccess ? '✅' : '❌'}</div>
          <h3 className={`text-lg font-semibold ${isSuccess ? 'text-emerald-700' : 'text-red-700'}`}>
            {isSuccess ? 'Payment Successful' : 'Payment Failed'}
          </h3>
          <p className="mt-2 text-sm text-gray-500">
            {isSuccess
              ? `₺${paymentResult.amount.toFixed(2)} paid to ${paymentResult.serviceProvider} for period ${paymentResult.period}.`
              : 'The payment was declined by the gateway. Please try again.'}
          </p>
          {paymentResult.transactionReference && (
            <p className="mt-2 text-xs text-gray-400 font-mono">Ref: {paymentResult.transactionReference}</p>
          )}
          <button
            onClick={onClose}
            className={`mt-6 w-full rounded-lg px-4 py-2.5 text-sm font-medium text-white transition-colors cursor-pointer ${
              isSuccess ? 'bg-emerald-600 hover:bg-emerald-700' : 'bg-red-600 hover:bg-red-700'
            }`}
          >
            {isSuccess ? 'Done' : 'Close'}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold text-gray-900">Make Payment</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 transition-colors cursor-pointer">✕</button>
        </div>

        <div className="mt-4 space-y-4">
          <div className="rounded-lg bg-gray-50 p-4 space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-gray-500">Provider</span><span className="font-medium">{debtInfo.serviceProvider}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Subscriber #</span><span className="font-mono text-xs">{debtInfo.subscriberNumber}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Period</span><span className="font-medium">{debtInfo.period}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Due Date</span><span>{new Date(debtInfo.dueDate).toLocaleDateString('tr-TR')}</span></div>
          </div>

          <div>
            <label htmlFor="amount" className="block text-sm font-medium text-gray-700">Amount (₺)</label>
            <input id="amount" type="number" step="0.01" min="0" value={amount} onChange={(e) => setAmount(e.target.value)}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 outline-none" />
            <p className="mt-1 text-xs text-gray-400">Suggested amount: ₺{debtInfo.debtAmount.toFixed(2)}</p>
          </div>

          {error && <p className="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</p>}

          <div className="flex gap-3 pt-2">
            <button onClick={onClose} disabled={loading}
              className="flex-1 rounded-lg border border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors disabled:opacity-50 cursor-pointer">Cancel</button>
            <button onClick={handlePay} disabled={loading || !amount || Number(amount) <= 0}
              className="flex-1 rounded-lg bg-emerald-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-emerald-700 active:bg-emerald-800 transition-colors disabled:opacity-50 cursor-pointer">
              {loading ? <LoadingSpinner size="sm" className="text-white" /> : 'Confirm Payment'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
