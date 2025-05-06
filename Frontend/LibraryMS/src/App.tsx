
import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./contexts/AuthContext";
import MainLayout from "./components/layout/MainLayout";
import RouteGuard from "./components/RouteGuard";

// Pages
import Login from "./pages/Login";
import Dashboard from "./pages/Dashboard";
import MyBooks from "./pages/MyBooks";
import ManageBooks from "./pages/ManageBooks";
import ManageCategories from "./pages/ManageCategories";
import BorrowingRequests from "./pages/BorrowingRequests";
import NotFound from "./pages/NotFound";
import BrowseBooks from "./pages/BrowseBooks";
import Register from "./pages/Register";

const queryClient = new QueryClient();

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <BrowserRouter>
        <AuthProvider>
          <MainLayout>
            <Routes>
              {/* Public routes */}
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />

              {/* Redirect from index to dashboard or login */}
              <Route path="/" element={<Navigate to="/dashboard" replace />} />

              {/* Protected routes for all authenticated users */}
              <Route
                path="/dashboard"
                element={
                  <RouteGuard requireAuth>
                    <Dashboard />
                  </RouteGuard>
                }
              />

              <Route
                path="/browse-books"
                element={
                  <RouteGuard requireAuth>
                    <BrowseBooks />
                  </RouteGuard>
                }
              />

              <Route
                path="/my-books"
                element={
                  <RouteGuard requireAuth>
                    <MyBooks />
                  </RouteGuard>
                }
              />

              {/* Admin-only routes */}
              <Route
                path="/manage-books"
                element={
                  <RouteGuard requireAuth requireAdmin>
                    <ManageBooks />
                  </RouteGuard>
                }
              />

              <Route
                path="/manage-categories"
                element={
                  <RouteGuard requireAuth requireAdmin>
                    <ManageCategories />
                  </RouteGuard>
                }
              />

              <Route
                path="/borrowing-requests"
                element={
                  <RouteGuard requireAuth requireAdmin>
                    <BorrowingRequests />
                  </RouteGuard>
                }
              />

              {/* 404 Route */}
              <Route path="*" element={<NotFound />} />
            </Routes>
          </MainLayout>
          <Toaster />
          <Sonner />
        </AuthProvider>
      </BrowserRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
