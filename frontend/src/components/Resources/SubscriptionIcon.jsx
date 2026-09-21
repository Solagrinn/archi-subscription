const TYPE_ICONS = {
  Electricity: '⚡',
  Water: '💧',
  Internet: '🌐',
  NaturalGas: '🔥',
  GSM: '📱',
  Insurance: '🛡️',
};

export default function SubscriptionIcon({ type }) {
  return <span className="text-lg">{TYPE_ICONS[type] || '📄'}</span>;
}

