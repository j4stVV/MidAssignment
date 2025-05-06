
import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { cn } from '@/lib/utils';
import {
  BookOpen,
  Users,
  LogOut,
  Settings,
  Menu,
  X,
  Book,
  BookPlus,
  User,
  Plus
} from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';

interface NavItemProps {
  to: string;
  icon: React.ElementType;
  label: string;
  isActive: boolean;
  onClick?: () => void;
}

const NavItem = ({ to, icon: Icon, label, isActive, onClick }: NavItemProps) => (
  <Link
    to={to}
    className={cn(
      "flex items-center gap-3 px-3 py-2 rounded-lg transition-colors",
      "hover:bg-sidebar-accent text-sidebar-foreground",
      isActive ? "bg-sidebar-accent font-medium" : "font-normal"
    )}
    onClick={onClick}
  >
    <Icon size={20} />
    <span>{label}</span>
  </Link>
);

const Navbar = () => {
  const [isOpen, setIsOpen] = useState(false);
  const location = useLocation();
  const { user, isAdmin, logout } = useAuth();

  const toggleNavbar = () => setIsOpen(!isOpen);
  const closeNavbar = () => setIsOpen(false);

  const isActive = (path: string) => location.pathname === path;

  return (
    <>
      <button
        onClick={toggleNavbar}
        className="fixed top-4 left-4 z-50 p-2 rounded-lg bg-primary text-white md:hidden"
        aria-label={isOpen ? "Close menu" : "Open menu"}
      >
        {isOpen ? <X size={16} /> : <Menu size={16} />}
      </button>

      {/* Navbar */}
      <nav
        className={cn(
          "fixed top-0 left-0 z-40 h-full bg-sidebar w-64 p-4 flex flex-col transition-transform duration-300 shadow-lg",
          "md:translate-x-0",
          isOpen ? "translate-x-0" : "-translate-x-full"
        )}
      >
        {/* Logo */}
        <div className="flex items-center gap-2 py-6">
          <BookOpen size={24} className="text-sidebar-foreground" />
          <h1 className="text-xl font-bold text-sidebar-foreground">BookWise</h1>
        </div>

        {/* User info */}
        {user && (
          <div className="bg-sidebar-accent/30 p-3 rounded-lg mb-6">
            <div className="flex items-center gap-3">
              <div className="w-9 h-9 rounded-full bg-sidebar-primary text-sidebar-primary-foreground flex items-center justify-center">
                {user.role === 'SuperUser' ? <Users size={18} /> : <User size={18} />}
              </div>
              <div>
                <p className="text-sm font-medium text-sidebar-foreground">{user.name}</p>
                <p className="text-xs text-sidebar-foreground/70">{user.role === 'SuperUser' ? 'Administrator' : 'User'}</p>
              </div>
            </div>
          </div>
        )}

        {/* Navigation Links */}
        <div className="space-y-1 flex-1">
          <NavItem
            to="/dashboard"
            icon={BookOpen}
            label="Dashboard"
            isActive={isActive("/dashboard")}
            onClick={closeNavbar}
          />

          {!isAdmin && (
            <NavItem
              to="/my-books"
              icon={Book}
              label="My Books"
              isActive={isActive("/my-books")}
              onClick={closeNavbar}
            />
          )}


          <NavItem
            to="/browse-books"
            icon={BookPlus}
            label="Browse Books"
            isActive={isActive("/browse-books")}
            onClick={closeNavbar}
          />

          {isAdmin && (
            <>
              <div className="mt-6 mb-2 px-3 text-xs font-semibold text-sidebar-foreground/70 uppercase tracking-wider">
                Admin
              </div>

              <NavItem
                to="/manage-books"
                icon={Book}
                label="Manage Books"
                isActive={isActive("/manage-books")}
                onClick={closeNavbar}
              />

              <NavItem
                to="/manage-categories"
                icon={Settings}
                label="Manage Categories"
                isActive={isActive("/manage-categories")}
                onClick={closeNavbar}
              />

              <NavItem
                to="/borrowing-requests"
                icon={Users}
                label="Borrowing Requests"
                isActive={isActive("/borrowing-requests")}
                onClick={closeNavbar}
              />
            </>
          )}
        </div>

        {/* Logout Button */}
        <button
          onClick={logout}
          className="flex items-center gap-3 px-3 py-2 mt-6 text-sidebar-foreground hover:bg-sidebar-accent rounded-lg transition-colors"
        >
          <LogOut size={20} />
          <span>Logout</span>
        </button>
      </nav>
    </>
  );
};

export default Navbar;