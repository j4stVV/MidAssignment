
import { useState, useEffect } from 'react';
import { Category } from "@/models/Category";
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
import { Textarea } from '@/components/ui/textarea';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { createCategory, deleteCategory, getCategoriesWithPage, updateCategory } from '@/services/CategoryService';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

const ManageCategories = () => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [formMode, setFormMode] = useState<'add' | 'edit'>('add');
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null);
  const [categoryToDelete, setCategoryToDelete] = useState<Category | null>(null);

  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(6);
  const pageSizeOptions = [6, 12, 24];

  const [formData, setFormData] = useState<Partial<Category>>({
    name: '',
  });

  useEffect(() => {
    loadCategoryData();
  }, [currentPage, pageSize]);

  const loadCategoryData = async () => {
    try {
      const response = await getCategoriesWithPage(currentPage, pageSize);
      setCategories(response.items);
      setTotalPages(response.totalPages);
    }
    catch (error) {
      toast.error('Error loading categories');
    }
  }

  const handleOpenAddDialog = () => {
    setFormData({
      name: ''
    });
    setFormMode('add');
    setSelectedCategory(null);
    setIsDialogOpen(true);
  };

  const handleOpenEditDialog = (category: Category) => {
    setFormData({ ...category });
    setFormMode('edit');
    setSelectedCategory(category);
    setIsDialogOpen(true);
  };

  const handleOpenDeleteDialog = (category: Category) => {
    setCategoryToDelete(category);
    setIsDeleteDialogOpen(true);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = async () => {
    if (!formData.name) {
      toast.error('Category name is required');
      return;
    }

    try {
      if (formMode === 'add') {
        await createCategory({
          name: formData.name!
        });
        toast.success('Category added successfully');
      } else if (formMode === 'edit' && selectedCategory) {
        await updateCategory({
          id: selectedCategory.id,
          name: formData.name!
        });
        toast.success('Category updated successfully');
      }

      setIsDialogOpen(false);
      await loadCategoryData();
    } catch (error) {
      toast.error('An error occurred while saving the category');
    }
  };

  const handleDelete = async () => {
    if (categoryToDelete) {
      try {
        await deleteCategory(categoryToDelete.id);
        toast.success('Category deleted successfully');
        setIsDeleteDialogOpen(false);
        await loadCategoryData();
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
          <h1 className="text-3xl font-bold">Manage Categories</h1>
          <p className="text-muted-foreground">
            Add, edit, or remove book categories
          </p>
        </div>
        <Button onClick={handleOpenAddDialog} className="flex items-center gap-2">
          <Plus size={18} /> Add New Category
        </Button>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
        {categories.map((category) => (
          <div key={category.id} className="bg-card border rounded-lg shadow-sm p-4">
            <div className="flex justify-between items-start mb-2">
              <h3 className="text-lg font-semibold">{category.name}</h3>
              <div className="flex space-x-1">
                <Button variant="ghost" size="sm" onClick={() => handleOpenEditDialog(category)}>
                  <Pencil size={16} />
                  <span className="sr-only">Edit</span>
                </Button>
                <Button variant="ghost" size="sm" className="text-destructive hover:text-destructive/80" onClick={() => handleOpenDeleteDialog(category)}>
                  <Trash2 size={16} />
                  <span className="sr-only">Delete</span>
                </Button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {categories.length === 0 && (
        <div className="text-center py-12 bg-muted/30 rounded-lg border border-border">
          <h3 className="text-lg font-medium">No categories yet</h3>
          <p className="text-muted-foreground mb-4">Start by adding your first book category</p>
          <Button onClick={handleOpenAddDialog} size="sm" className="flex items-center gap-2">
            <Plus size={14} /> Add Category
          </Button>
        </div>
      )}

      {/* Pagination */}
      <div className="mt-4 flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div className="text-sm text-muted-foreground">
            Showing {(currentPage - 1) * pageSize + 1} to {Math.min(currentPage * pageSize, categories.length)} of {totalPages * pageSize} categories
          </div>
          <div className="flex items-center gap-2">
            <Label htmlFor="pageSize">Categories per page</Label>
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

      {/* Add/Edit Category Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{formMode === 'add' ? 'Add New Category' : 'Edit Category'}</DialogTitle>
            <DialogDescription>
              {formMode === 'add'
                ? 'Add a new category to organize your books.'
                : 'Update this category\'s information.'}
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div>
              <Label htmlFor="name">Category Name</Label>
              <Input
                id="name"
                name="name"
                value={formData.name}
                onChange={handleInputChange}
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setIsDialogOpen(false)}>Cancel</Button>
            <Button onClick={handleSubmit}>{formMode === 'add' ? 'Add Category' : 'Save Changes'}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the category <span className="font-semibold">{categoryToDelete?.name}</span>.
              <br /><br />
              <span className="font-medium text-destructive">Warning:</span> Books in this category may be affected.
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

export default ManageCategories;
