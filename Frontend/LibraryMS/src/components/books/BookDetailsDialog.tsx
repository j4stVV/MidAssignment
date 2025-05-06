import { Book } from '@/models/Book';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { CheckCircle, PlusCircle, BookOpen } from 'lucide-react';

interface BookDetailsDialogProps {
    book: Book;
    isOpen: boolean;
    onClose: () => void;
    onBorrow: (book: Book) => void;
    isSelected: boolean;
    isSelectable: boolean;
}

const BookDetailsDialog = ({
    book,
    isOpen,
    onClose,
    onBorrow,
    isSelected,
    isSelectable
}: BookDetailsDialogProps) => {
    return (
        <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle className="flex items-center justify-between">
                        <span>{book.title}</span>
                    </DialogTitle>
                    <DialogDescription className="text-foreground/70">
                        by {book.author}
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-4">
                    <div>
                        <h4 className="font-medium mb-1">Category</h4>
                        <Badge className="bg-primary/10 text-primary hover:bg-primary/20 border-none">
                            {book.categoryName}
                        </Badge>
                    </div>

                    <div>
                        <h4 className="font-medium mb-1">Description</h4>
                        <p className="text-sm text-muted-foreground">
                            {book.description || 'No description available.'}
                        </p>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <h4 className="font-medium mb-1">Published Year</h4>
                            <p className="text-sm text-muted-foreground">{book.publishedDate}</p>
                        </div>
                        <div>
                            <h4 className="font-medium mb-1">ISBN</h4>
                            <p className="text-sm text-muted-foreground">{book.isbn}</p>
                        </div>
                    </div>

                    <div>
                        <h4 className="font-medium mb-1">Availability</h4>
                        <p className="text-sm flex items-center">
                            {book.available ? (
                                <Badge className="bg-status-approved/10 text-status-approved">Available</Badge>
                            ) : (
                                <Badge className="bg-status-rejected/10 text-status-rejected">Borrowed</Badge>
                            )}
                        </p>
                    </div>
                </div>

                <div className="flex justify-end mt-4">
                    {book.available > 0 ? (
                        <Button
                            variant={isSelected ? "secondary" : "outline"}
                            className={isSelected ? "bg-primary/10 text-primary hover:bg-primary/20" : ""}
                            onClick={() => onBorrow(book)}
                            disabled={!isSelectable && !isSelected}
                        >
                            {isSelected ? (
                                <>
                                    <CheckCircle size={16} className="mr-1" /> Selected
                                </>
                            ) : (
                                <>
                                    <PlusCircle size={16} className="mr-1" /> Borrow
                                </>
                            )}
                        </Button>
                    ) : (
                        <Button variant="ghost" disabled>
                            <BookOpen size={16} className="mr-1" /> Borrowed
                        </Button>
                    )}
                </div>
            </DialogContent>
        </Dialog>
    );
};

export default BookDetailsDialog;