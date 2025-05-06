import { useState } from 'react';
import { Book } from '@/models/Book';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { BookOpen, CheckCircle, Info, PlusCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import BookDetailsDialog from './BookDetailsDialog.tsx';

interface BookCardProps {
    book: Book;
    onBorrow: (book: Book) => void;
    isSelected: boolean;
    isSelectable: boolean;
}

const BookCard = ({ book, onBorrow, isSelected, isSelectable }: BookCardProps) => {
    const [showDetails, setShowDetails] = useState(false);

    return (
        <>
            <Card className="h-full flex flex-col transition-all duration-200 hover:shadow-md">
                <CardHeader className="pb-2">
                    <div className="flex justify-between items-start">
                        <CardTitle className="text-lg line-clamp-1" title={book.title}>{book.title}</CardTitle>
                    </div>
                    <p className="text-sm text-muted-foreground">by {book.author}</p>
                </CardHeader>

                <CardContent className="flex-grow">
                    <div className="flex items-center mb-2">
                        <Badge className="bg-primary/10 text-primary hover:bg-primary/20 border-none">
                            {book.categoryName}
                        </Badge>
                    </div>
                    <p className="text-sm line-clamp-3 text-muted-foreground">
                        {book.description || 'No description available.'}
                    </p>
                    <p className="text-xs mt-2 text-muted-foreground">
                        Published: {new Date(book.publishedDate).toLocaleDateString()}
                        <br />ISBN: {book.isbn}
                    </p>
                </CardContent>

                <CardFooter className="pt-2 flex justify-between border-t">
                    <Button
                        variant="ghost"
                        size="sm"
                        className="px-2"
                        onClick={() => setShowDetails(true)}
                    >
                        <Info size={16} className="mr-1" /> Details
                    </Button>

                    {book.available > 0 ? (
                        <Button
                            variant={isSelected ? "secondary" : "outline"}
                            size="sm"
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
                        <Button variant="ghost" size="sm" disabled>
                            <BookOpen size={16} className="mr-1" /> Borrowed
                        </Button>
                    )}
                </CardFooter>
            </Card>

            <BookDetailsDialog
                book={book}
                isOpen={showDetails}
                onClose={() => setShowDetails(false)}
                onBorrow={onBorrow}
                isSelected={isSelected}
                isSelectable={isSelectable}
            />
        </>
    );
};

export default BookCard;