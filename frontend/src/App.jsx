import { Routes, Route, Navigate } from 'react-router-dom';
import SeedPage from './pages/SeedPage';
import DashboardPage from './pages/DashboardPage';

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<SeedPage />} />
      <Route path="/dashboard/:customerId" element={<DashboardPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

