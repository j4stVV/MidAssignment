
import React, { ReactNode } from 'react';
import Navbar from './Navbar';
import { useAuth } from '@/contexts/AuthContext';

interface MainLayoutProps {
  children: ReactNode;
}

const MainLayout = ({ children }: MainLayoutProps) => {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) return <>{children}</>;

  return (
    <div className="flex min-h-screen">
      <Navbar />
      <main className="flex-grow ml-0 md:ml-64 pt-16 md:pt-4 p-4 md:p-6 transition-all duration-300">
        {children}
      </main>
    </div>
  );
};

export default MainLayout;
