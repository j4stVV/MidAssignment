import { useState, useEffect } from 'react';
import { BookOpen, Book, Users, Calendar } from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';
import { getCategoriesWithPage } from '@/services/CategoryService';
import { toast } from '@/components/ui/sonner';
import { getBooks } from '@/services/BookService';
import { getUserBorrowingRequests, getAllBorrowingRequests } from '@/services/BorrowingService';

interface StatCardProps {
  title: string;
  value: number | string;
  icon: React.ElementType;
  description?: string;
  className?: string;
}

const StatCard = ({ title, value, icon: Icon, description, className }: StatCardProps) => (
  <div className={`bg-card rounded-lg shadow-sm p-6 ${className}`}>
    <div className="flex items-center gap-4">
      <div className="p-3 rounded-full bg-primary/10 text-primary">
        <Icon size={24} />
      </div>
      <div>
        <p className="text-sm text-muted-foreground">{title}</p>
        <h3 className="text-2xl font-bold">{value}</h3>
        {description && <p className="text-xs text-muted-foreground mt-1">{description}</p>}
      </div>
    </div>
  </div>
);

const Dashboard = () => {
  const { isAdmin, user } = useAuth();
  const [recentBooks, setRecentBooks] = useState([]);
  const [stats, setStats] = useState({
    admin: {
      totalBooks: 0,
      totalCategories: 0,
      pendingRequests: 0,
      activeUsers: 0,
    },
    user: {
      availableRequests: 0,
      booksCurrentlyBorrowed: 0,
      totalBorrowedBooks: 0,
    },
  });

  useEffect(() => {
    loadRecentBooks();
    if (isAdmin) {
      loadAdminStats();
    } else if (user) {
      loadUserStats();
    }
  }, [isAdmin]);

  const loadRecentBooks = async () => {
    try {
      const bookResponse = await getBooks(1, 3);
      setRecentBooks(
        bookResponse.items.map((book: any) => ({
          id: book.id,
          title: book.title,
          author: book.author,
          category: book.categoryName,
        }))
      );
    } catch (error) {
      console.error('Error loading recent books:', error);
      toast.error('Error loading recent books');
    }
  };

  const loadAdminStats = async () => {
    try {
      const [categoryResponse, bookResponse, borrowingRequests] = await Promise.all([
        getCategoriesWithPage(1, 1000),
        getBooks(1, 1000),
        getAllBorrowingRequests(),
      ]);

      const totalCategories = categoryResponse.totalItems;
      const totalBooks = bookResponse.totalItems;
      const pendingRequests = borrowingRequests.filter(
        (req: any) => req.status === 2
      ).length;

      setStats((prev) => ({
        ...prev,
        admin: {
          ...prev.admin,
          totalBooks,
          totalCategories,
          pendingRequests,
          activeUsers: 0,
        },
      }));
    } catch (error: any) {
      console.error('Error loading admin stats:', error.message, error.response?.data);
      toast.error('Error loading dashboard statistics: ' + (error.message || 'Unknown error'));
    }
  };

  const loadUserStats = async () => {
    try {
      const borrowingRequests = await getUserBorrowingRequests();

      const approvedRequests = borrowingRequests.filter((req: any) => req.status === 0); // Approved

      const totalBorrowedBooks = approvedRequests.reduce(
        (total: number, req: any) => total + (req.details?.length || 0),
        0
      );

      const mostRecentApprovedRequest = approvedRequests.sort(
        (a: any, b: any) => new Date(b.requestedDate).getTime() - new Date(a.requestedDate).getTime()
      )[0];
      const booksCurrentlyBorrowed = mostRecentApprovedRequest?.details?.length || 0;

      const availableRequests = 3 - borrowingRequests.length;

      setStats((prev) => ({
        ...prev,
        user: {
          availableRequests: availableRequests < 0 ? 0 : availableRequests,
          booksCurrentlyBorrowed,
          totalBorrowedBooks,
        },
      }));
    } catch (error: any) {
      toast.error('Error loading user statistics');
    }
  };

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-foreground">Welcome, {user?.name}</h1>
        <p className="text-muted-foreground">Here's what's happening with your library today.</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-10">
        {isAdmin ? (
          <>
            <StatCard title="Total Books" value={stats.admin.totalBooks} icon={Book} />
            <StatCard title="Categories" value={stats.admin.totalCategories} icon={BookOpen} />
            <StatCard
              title="Pending Requests"
              value={stats.admin.pendingRequests}
              icon={Calendar}
              className="bg-secondary/10"
            />
            <StatCard title="Active Users" value={stats.admin.activeUsers} icon={Users} />
          </>
        ) : (
          <>
            <StatCard
              title="Available Requests"
              value={`${stats.user.availableRequests}/3`}
              icon={Calendar}
              description="Monthly limit"
            />
            <StatCard
              title="Books Borrowed"
              value={stats.user.booksCurrentlyBorrowed}
              icon={Book}
              description="Currently borrowed"
            />
            <StatCard
              title="Total Books"
              value={stats.user.totalBorrowedBooks}
              icon={BookOpen}
              description="All time"
            />
          </>
        )}
      </div>

      <div>
        <h2 className="text-xl font-semibold mb-4">Recently Added Books</h2>
        <div className="bg-card shadow-sm rounded-lg overflow-hidden">
          <table className="w-full">
            <thead className="bg-muted/50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">Title</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">Author</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">Category</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {recentBooks.map((book) => (
                <tr key={book.id}>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-foreground">{book.title}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{book.author}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{book.category}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;