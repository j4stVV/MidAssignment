import { useState, useEffect } from 'react';
import { Category } from '@/models/Category';
import { Book } from '@/models/Book';
import { useAuth } from '@/contexts/AuthContext';
import { toast } from 'sonner';
import BookCard from '@/components/books/BookCard';
import FilterBar from '@/components/books/FilterBar';
import { Button } from '@/components/ui/button';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { BookOpen, CheckCircle } from 'lucide-react';
import { filterBooks } from '@/services/BookService';
import { getCategories } from '@/services/CategoryService';
import { createBorrowingRequest } from '@/services/BorrowingService';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

const BrowseBooks = () => {
    const { user } = useAuth();
    const [books, setBooks] = useState<Book[]>([]);
    const [categories, setCategories] = useState<Category[]>([]);
    const [selectedBooks, setSelectedBooks] = useState<Book[]>([]);
    const [borrowDialogOpen, setBorrowDialogOpen] = useState(false);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalItems, setTotalItems] = useState(0);
    const [pageSize, setPageSize] = useState(4);
    const pageSizeOptions = [4, 8, 12];

    const [filters, setFilters] = useState<{
        searchTitle: string;
        searchAuthor: string;
        category: string;
        availability: string;
    }>({
        searchTitle: '',
        searchAuthor: '',
        category: 'all',
        availability: '',
    });

    useEffect(() => {
        const fetchData = async () => {
            try {
                const searchTitle = filters.searchTitle.trim() || undefined;
                const searchAuthor = filters.searchAuthor.trim() || undefined;
                const categoryId = filters.category !== 'all' ? filters.category : undefined;
                const available = filters.availability === 'available' ? true : filters.availability === 'unavailable' ? false : undefined;

                const bookData = await filterBooks(
                    currentPage,
                    pageSize,
                    searchTitle,
                    searchAuthor,
                    categoryId,
                    available
                );

                setBooks(bookData.items);
                setTotalItems(bookData.totalItems);
                setCurrentPage(bookData.pageNumber);

                const allCategories = await getCategories();
                setCategories(allCategories);
            } catch (error) {
                toast.error('Failed to load books or categories');
            }
        };

        fetchData();
    }, [currentPage, pageSize, filters]);

    const handleFilter = (newFilters: {
        searchTitle: string;
        searchAuthor: string;
        category: string;
        availability: string;
    }) => {
        setFilters(newFilters);
        setCurrentPage(1);
    };

    const toggleBookSelection = (book: Book) => {
        if (selectedBooks.some(b => b.id === book.id)) {
            setSelectedBooks(selectedBooks.filter(selectBook => selectBook.id !== book.id));
        } else {
            if (selectedBooks.length >= 5) {
                toast.error("You can only borrow up to 5 books in one request");
                return;
            }
            setSelectedBooks([...selectedBooks, book]);
        }
    };

    const handleBorrowSubmit = async () => {
        if (!user) {
            toast.error("You must be logged in to borrow books");
            return;
        }

        if (selectedBooks.length === 0) {
            toast.error("Please select at least one book to borrow");
            return;
        }

        try {
            await createBorrowingRequest({ bookIds: selectedBooks.map(b => b.id) });
            toast.success("Borrowing request submitted successfully");
            setSelectedBooks([]);
            setBorrowDialogOpen(false);

            const bookData = await filterBooks(
                currentPage,
                pageSize,
                filters.searchTitle.trim() || undefined,
                filters.searchAuthor.trim() || undefined,
                filters.category !== 'all' ? filters.category : undefined,
                filters.availability === 'available' ? true : filters.availability === 'unavailable' ? false : undefined
            );
            setBooks(bookData.items);
            setTotalItems(bookData.totalItems);
        } catch (error) {
            toast.error(error.response?.data?.error || 'Failed to submit borrowing request');
        }
    };

    const handlePageSizeChange = (value: string) => {
        const newPageSize = parseInt(value);
        setPageSize(newPageSize);
        setCurrentPage(1);
    };

    const handlePageChange = (page: number) => {
        if (page >= 1 && page <= Math.ceil(totalItems / pageSize)) {
            setCurrentPage(page);
        }
    };

    const totalPages = Math.ceil(totalItems / pageSize);

    return (
        <div>
            <div className="flex flex-col md:flex-row md:justify-between md:items-center mb-6">
                <div>
                    <h1 className="text-3xl font-bold">Browse Books</h1>
                    <p className="text-muted-foreground">
                        Explore our collection and borrow books
                    </p>
                </div>

                {selectedBooks.length > 0 && (
                    <Button
                        onClick={() => setBorrowDialogOpen(true)}
                        className="mt-4 md:mt-0"
                    >
                        <BookOpen className="mr-2 h-4 w-4" />
                        Borrow Selected Books
                        <Badge variant="outline" className="ml-2 bg-primary/10">
                            {selectedBooks.length}/5
                        </Badge>
                    </Button>
                )}
            </div>

            <FilterBar categories={categories} onFilter={handleFilter} />

            {books.length > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
                    {books.map(book => (
                        <BookCard
                            key={book.id}
                            book={book}
                            onBorrow={toggleBookSelection}
                            isSelected={selectedBooks.some(b => b.id === book.id)}
                            isSelectable={selectedBooks.length < 5 || selectedBooks.some(b => b.id === book.id)}
                        />
                    ))}
                </div>
            ) : (
                <div className="text-center py-12 bg-muted/30 rounded-lg border">
                    <BookOpen className="mx-auto h-12 w-12 text-muted-foreground/50" />
                    <h3 className="mt-2 text-lg font-medium">No books found</h3>
                    <p className="text-muted-foreground">Try adjusting your filters</p>
                </div>
            )}

            {/* Pagination */}
            <div className="mt-4 flex items-center justify-between">
                <div className="flex items-center gap-4">
                    <div className="text-sm text-muted-foreground">
                        Showing {(currentPage - 1) * pageSize + 1} to {Math.min(currentPage * pageSize, totalItems)} of {totalItems} books
                    </div>
                    <div className="flex items-center gap-2">
                        <Label htmlFor="pageSize">Books per page</Label>
                        <Select
                            value={pageSize.toString()}
                            onValueChange={handlePageSizeChange}
                        >
                            <SelectTrigger id="pageSize" className="w-[100px]">
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                {pageSizeOptions.map((size) => (
                                    <SelectItem key={size} value={size.toString()}>
                                        {size}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>
                </div>
                <div className="flex items-center gap-2">
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handlePageChange(currentPage - 1)}
                        disabled={currentPage === 1}
                    >
                        Previous
                    </Button>
                    {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                        <Button
                            key={page}
                            variant={currentPage === page ? "default" : "outline"}
                            size="sm"
                            onClick={() => handlePageChange(page)}
                        >
                            {page}
                        </Button>
                    ))}
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handlePageChange(currentPage + 1)}
                        disabled={currentPage === totalPages}
                    >
                        Next
                    </Button>
                </div>
            </div>

            {/* Borrow confirmation dialog */}
            <Dialog open={borrowDialogOpen} onOpenChange={setBorrowDialogOpen}>
                <DialogContent className="sm:max-w-[500px]">
                    <DialogHeader>
                        <DialogTitle>Confirm Borrowing Request</DialogTitle>
                    </DialogHeader>

                    <div className="max-h-[200px] overflow-y-auto py-4">
                        <h3 className="font-medium mb-2">Selected Books:</h3>
                        <ul className="space-y-2">
                            {selectedBooks.map(book => (
                                <li key={book.id} className="flex items-center gap-2 p-2 bg-muted/20 rounded">
                                    <CheckCircle size={16} className="text-primary" />
                                    <div>
                                        <p className="font-medium">{book.title}</p>
                                        <p className="text-xs text-muted-foreground">by {book.author}</p>
                                    </div>
                                </li>
                            ))}
                        </ul>
                    </div>

                    <DialogFooter>
                        <Button variant="outline" onClick={() => setBorrowDialogOpen(false)}>
                            Cancel
                        </Button>
                        <Button onClick={handleBorrowSubmit}>
                            Submit Request
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default BrowseBooks;