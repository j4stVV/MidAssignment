
import { Link } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { Button } from '@/components/ui/button';
import { BookX } from 'lucide-react';

const NotFound = () => {
  const { isAuthenticated } = useAuth();
  
  return (
    <div className="flex flex-col items-center justify-center min-h-screen px-4 text-center">
      <BookX size={64} className="text-muted-foreground mb-4" />
      <h1 className="text-4xl font-bold mb-2">404</h1>
      <h2 className="text-xl font-semibold mb-4">Page not found</h2>
      <p className="text-muted-foreground max-w-md mb-8">
        Sorry, we couldn't find the page you're looking for. The book might have been misplaced on our shelves.
      </p>
      <Button asChild>
        <Link to={isAuthenticated ? '/dashboard' : '/login'}>
          Return to {isAuthenticated ? 'Dashboard' : 'Login'}
        </Link>
      </Button>
    </div>
  );
};

export default NotFound;
