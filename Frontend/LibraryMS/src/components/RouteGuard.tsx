import { ReactNode, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { toast } from '@/components/ui/sonner';

interface RouteGuardProps {
  children: ReactNode;
  requireAuth?: boolean;
  requireAdmin?: boolean;
}

const RouteGuard = ({
  children,
  requireAuth = false,
  requireAdmin = false
}: RouteGuardProps) => {
  const { isAuthenticated, isAdmin, isLoading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    if (isLoading) return;
    if (requireAuth && !isAuthenticated) {
      toast.error("Please log in to access this page");
      navigate('/login', { state: { from: location.pathname } });
      return;
    }
    if (requireAdmin && !isAdmin) {
      toast.error("You don't have permission to access this page");
      navigate('/dashboard');
      return;
    }
  }, [requireAuth, requireAdmin, isAuthenticated, isAdmin, navigate, location.pathname]);

  return <>{children}</>;
};

export default RouteGuard;