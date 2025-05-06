
import { useState, useEffect } from 'react';
import { Book } from '@/models/Book';
import { Category } from "@/models/Category";
import { createBook, deleteBook, getBooks, updateBook } from '@/services/BookService';
import { toast } from '@/components/ui/sonner';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from '@/components/ui/textarea';
import { Switch } from '@/components/ui/switch';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { getCategories } from '@/services/CategoryService';

const ManageBooks = () => {
  const [books, setBooks] = useState<Book[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);

  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [formMode, setFormMode] = useState<'add' | 'edit'>('add');
  const [selectedBook, setSelectedBook] = useState<Book | null>(null);
  const [bookToDelete, setBookToDelete] = useState<Book | null>(null);

  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [totalItems, setTotalItems] = useState(0);
  const pageSizeOptions = [5, 10, 20, 50];

  const [formData, setFormData] = useState<Partial<Book>>({
    title: '',
    author: '',
    description: '',
    isbn: '',
    publishedDate: '',
    categoryId: '',
    quantity: 1,
    available: 1
  });


  const loadBookData = async () => {
    try {
      const response = await getBooks(currentPage, pageSize);
      setBooks(response.items);
      setTotalPages(response.totalPages);
      setTotalItems(response.totalItems);
    }
    catch (error) {
      toast.error('Error loading books');
    }
  }
  const loadCategoryData = async () => {
    try {
      const categories = await getCategories();
      if (!categories || categories.length === 0) {
        toast.error('No categories available. Please add categories first.');
        setCategories([]);
      } else {
        setCategories(categories);
      }
    } catch (error) {
      toast.error('Error loading categories');
      setCategories([]);
    }
  };

  useEffect(() => {
    loadBookData();
    loadCategoryData();
  }, [currentPage, pageSize]);

  const handleOpenAddDialog = () => {
    setFormData({
      title: '',
      author: '',
      isbn: '',
      description: '',
      publishedDate: '',
      categoryId: '',
      quantity: 1,
      available: 1
    });
    setFormMode('add');
    setSelectedBook(null);
    setIsDialogOpen(true);
  };

  const handleOpenEditDialog = (book: Book) => {
    setFormData({ ...book });
    setFormMode('edit');
    setSelectedBook(book);
    setIsDialogOpen(true);
  };

  const handleOpenDeleteDialog = (book: Book) => {
    setBookToDelete(book);
    setIsDeleteDialogOpen(true);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSwitchChange = (name: string, checked: boolean) => {
    setFormData({ ...formData, [name]: checked });
  };

  const handleSelectChange = (name: string, value: string) => {
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = async () => {
    // Validation
    if (!formData.title || !formData.author || !formData.isbn || !formData.categoryId || !formData.publishedDate) {
      toast.error('Please fill in all required fields');
      return;
    }

    try {
      const categoryName = categories.find(c => c.id === formData.categoryId)?.name || '';

      if (formMode === 'add') {
        await createBook({
          ...formData as Omit<Book, 'id'>,
          categoryName,
        });
        toast.success('Book added successfully');
      } else if (formMode === 'edit' && selectedBook) {
        await updateBook({
          ...formData as Book,
          id: selectedBook.id,
          categoryName,
        });
        toast.success('Book updated successfully');
      }

      setIsDialogOpen(false);
      await loadBookData();
    } catch (error) {
      toast.error(error.response.data.details.join('\n'));
    }
  };

  const handleDelete = async () => {
    if (bookToDelete) {
      try {
        await deleteBook(bookToDelete.id);
        toast.success('Book deleted successfully');
        setIsDeleteDialogOpen(false);
        await loadBookData();
      } catch (error) {
        toast.error(error.response.data.error);
      }
    }
  };

  const handlePageSizeChange = (value: string) => {
    const newPageSize = parseInt(value);
    setPageSize(newPageSize);
    setCurrentPage(1); // Reset về trang đầu khi thay đổi pageSize
  };

  const handlePageChange = (page: number) => {
    if (page >= 1 && page <= totalPages) {
      setCurrentPage(page);
    }
  };

  return (
    <div>
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold">Manage Books</h1>
          <p className="text-muted-foreground">
            Add, edit, or remove books from the library
          </p>
        </div>
        <Button onClick={handleOpenAddDialog} className="flex items-center gap-2">
          <Plus size={18} /> Add New Book
        </Button>
      </div>

      <div className="bg-card border rounded-lg shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">Title</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">Author</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">Category</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">ISBN</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">Available</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">Quantity</th>
                <th className="px-4 py-3 text-xs font-medium text-muted-foreground uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {books.map((book) => (
                <tr key={book.id}>
                  <td className="px-4 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <div>
                        <div className="text-sm font-medium text-foreground">{book.title}</div>
                        <div className="text-sm text-muted-foreground">{new Date(book.publishedDate).toLocaleDateString()}</div>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-4 whitespace-nowrap text-sm text-foreground">{book.author}</td>
                  <td className="px-4 py-4 whitespace-nowrap text-sm text-foreground">{book.categoryName}</td>
                  <td className="px-4 py-4 whitespace-nowrap text-sm text-muted-foreground">{book.isbn}</td>
                  <td className="px-4 py-4 whitespace-nowrap">
                    <div className="flex items-center gap-2">
                      <span className={`inline-flex rounded-full h-2 w-2 ${(book.available > 0) ? 'bg-green-400' : 'bg-red-400'}`}></span>
                      <span className="text-sm">{book.available}</span>
                    </div>
                  </td>
                  <td className="px-4 py-4 whitespace-nowrap text-sm text-muted-foreground">{book.quantity}</td>
                  <td className="px-4 py-4 whitespace-nowrap text-sm font-medium text-right space-x-2">
                    <Button variant="ghost" size="sm" onClick={() => handleOpenEditDialog(book)}>
                      <Pencil size={16} />
                      <span className="sr-only">Edit</span>
                    </Button>
                    <Button variant="ghost" size="sm" className="text-destructive hover:text-destructive/80" onClick={() => handleOpenDeleteDialog(book)}>
                      <Trash2 size={16} />
                      <span className="sr-only">Delete</span>
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

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

      {/* Add/Edit Book Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="sm:max-w-[600px]">
          <DialogHeader>
            <DialogTitle>{formMode === 'add' ? 'Add New Book' : 'Edit Book'}</DialogTitle>
            <DialogDescription>
              {formMode === 'add'
                ? 'Add a new book to the library catalog.'
                : 'Update the information for this book.'}
            </DialogDescription>
          </DialogHeader>

          <div className="grid grid-cols-2 gap-4 py-4">
            <div className="col-span-2">
              <Label htmlFor="title">Title</Label>
              <Input
                id="title"
                name="title"
                value={formData.title}
                onChange={handleInputChange}
              />
            </div>

            <div>
              <Label htmlFor="author">Author</Label>
              <Input
                id="author"
                name="author"
                value={formData.author}
                onChange={handleInputChange}
              />
            </div>

            <div>
              <Label htmlFor="isbn">ISBN</Label>
              <Input
                id="isbn"
                name="isbn"
                value={formData.isbn}
                onChange={handleInputChange}
              />
            </div>

            <div>
              <Label htmlFor="publishedDate">Published Year</Label>
              <Input
                id="publishedDate"
                name="publishedDate"
                type="date"
                value={formData.publishedDate}
                onChange={handleInputChange}
              />
            </div>

            <div>
              <Label htmlFor="categoryId">Category</Label>
              <Select
                value={formData.categoryId?.toString()}
                onValueChange={(value) => handleSelectChange("categoryId", value)}
              >
                <SelectTrigger id="categoryId">
                  <SelectValue placeholder="Select category" />
                </SelectTrigger>
                <SelectContent>
                  {categories.map((category) => (
                    <SelectItem key={category.id} value={category.id.toString()}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label htmlFor="Quantity">Quantity</Label>
              <Input
                id="quantity"
                name="quantity"
                type="number"
                value={formData.quantity}
                onChange={handleInputChange}
              />
            </div>

            <div className="col-span-2">
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleInputChange}
              />
            </div>

            <div className="flex items-center space-x-2">
              <Switch
                id="isAvailable"
                checked={formData.quantity > 0}
                onCheckedChange={(checked) => handleSwitchChange("isAvailable", checked)}
              />
              <Label htmlFor="isAvailable">Available for borrowing</Label>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setIsDialogOpen(false)}>Cancel</Button>
            <Button onClick={handleSubmit}>Save</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete <span className="font-semibold">{bookToDelete?.title}</span> and remove it from the library catalog.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default ManageBooks;