import { useEffect, useState, useCallback } from 'react';
import * as api from '../../api/client.js';

export default function TopBar({ children }) {
  const [timeInfo, setTimeInfo] = useState(null);
  const [loading, setLoading] = useState(false);

  const loadTime = useCallback(async () => {
    try {
      const data = await api.fetchCurrentTime();
      setTimeInfo(data);
    } catch {
      // ignore
    }
  }, []);

  useEffect(() => {
    loadTime();
    const interval = setInterval(loadTime, 30000); // refresh every 30s
    return () => clearInterval(interval);
  }, [loadTime]);

  const handleForward = async () => {
    setLoading(true);
    try {
      const data = await api.timeForward();
      setTimeInfo(data);
      window.dispatchEvent(new CustomEvent('timewarp'));
    } finally {
      setLoading(false);
    }
  };

  const handleBackward = async () => {
    setLoading(true);
    try {
      const data = await api.timeBackward();
      setTimeInfo(data);
      window.dispatchEvent(new CustomEvent('timewarp'));
    } finally {
      setLoading(false);
    }
  };

  const handleReset = async () => {
    setLoading(true);
    try {
      const data = await api.timeReset();
      setTimeInfo(data);
      window.dispatchEvent(new CustomEvent('timewarp'));
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return '...';
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-GB', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  return (
    <div className="sticky top-0 z-50">
      {/* Time Warp Bar */}
      <div className="bg-gray-900 text-white">
        <div className="mx-auto max-w-5xl px-4 sm:px-6">
          <div className="flex items-center justify-between h-9 text-xs">
            {/* Backend Date */}
            <div className="flex items-center gap-2">
              <span className="text-gray-400">🕐 Backend Date:</span>
              <span className="font-mono font-semibold text-emerald-400">
                {timeInfo ? formatDate(timeInfo.currentDate) : '...'}
              </span>
              {timeInfo?.offsetDays !== 0 && timeInfo?.offsetDays != null && (
                <span className="rounded bg-amber-500/20 text-amber-300 px-1.5 py-0.5 text-[10px] font-medium">
                  {timeInfo.offsetDays > 0 ? '+' : ''}{timeInfo.offsetDays}d
                </span>
              )}
            </div>

            {/* Time Controls */}
            <div className="flex items-center gap-1.5">
              <span className="text-gray-500 mr-1">Time Warp:</span>
              <button
                onClick={handleBackward}
                disabled={loading}
                className="rounded bg-gray-700 hover:bg-gray-600 px-2 py-0.5 text-xs font-medium transition-colors disabled:opacity-50 cursor-pointer"
                title="Go back 1 day"
              >
                ◀ −1d
              </button>
              <button
                onClick={handleForward}
                disabled={loading}
                className="rounded bg-gray-700 hover:bg-gray-600 px-2 py-0.5 text-xs font-medium transition-colors disabled:opacity-50 cursor-pointer"
                title="Go forward 1 day"
              >
                +1d ▶
              </button>
              {timeInfo?.offsetDays !== 0 && timeInfo?.offsetDays != null && (
                <button
                  onClick={handleReset}
                  disabled={loading}
                  className="rounded bg-red-700/60 hover:bg-red-600 px-2 py-0.5 text-xs font-medium transition-colors disabled:opacity-50 cursor-pointer ml-1"
                  title="Reset to real time"
                >
                  ↺ Reset
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
      {/* Page Header (children) */}
      {children}
    </div>
  );
}

