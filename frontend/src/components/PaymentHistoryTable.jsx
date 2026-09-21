import StatusBadge from './Resources/StatusBadge.jsx';

export default function PaymentHistoryTable({ payments }) {
  if (payments.length === 0) {
    return (
      <div className="rounded-xl border border-dashed border-gray-300 bg-white p-8 text-center">
        <p className="text-sm text-gray-500">No payment history yet.</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-gray-100 bg-gray-50/50">
            <th className="px-4 py-3 font-medium text-gray-500">Provider</th>
            <th className="px-4 py-3 font-medium text-gray-500">Subscriber #</th>
            <th className="px-4 py-3 font-medium text-gray-500">Period</th>
            <th className="px-4 py-3 font-medium text-gray-500 text-right">Amount</th>
            <th className="px-4 py-3 font-medium text-gray-500">Status</th>
            <th className="px-4 py-3 font-medium text-gray-500">Date</th>
            <th className="px-4 py-3 font-medium text-gray-500">Reference</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-50">
          {payments.map((p) => (
            <tr key={p.id} className="hover:bg-gray-50/50 transition-colors">
              <td className="px-4 py-3 font-medium text-gray-900">{p.serviceProvider}</td>
              <td className="px-4 py-3 text-gray-500 font-mono text-xs">{p.subscriberNumber}</td>
              <td className="px-4 py-3 text-gray-600">{p.period}</td>
              <td className="px-4 py-3 text-right font-semibold text-gray-900">₺{p.amount.toFixed(2)}</td>
              <td className="px-4 py-3">
                <StatusBadge status={p.status} />
              </td>
              <td className="px-4 py-3 text-gray-500 text-xs">
                {new Date(p.paymentDate).toLocaleDateString('tr-TR')}
              </td>
              <td className="px-4 py-3 text-gray-400 font-mono text-xs truncate max-w-[140px]">
                {p.transactionReference || '—'}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

