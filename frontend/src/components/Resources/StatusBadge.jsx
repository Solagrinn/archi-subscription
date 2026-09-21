const STATUS_MAP = {
  Active: 'bg-emerald-100 text-emerald-800',
  Passive: 'bg-gray-100 text-gray-600',
  Successful: 'bg-emerald-100 text-emerald-800',
  Failed: 'bg-red-100 text-red-800',
  Pending: 'bg-amber-100 text-amber-800',
};

export default function StatusBadge({ status }) {
  const classes = STATUS_MAP[status] || 'bg-gray-100 text-gray-600';

  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold ${classes}`}>
      {status}
    </span>
  );
}

