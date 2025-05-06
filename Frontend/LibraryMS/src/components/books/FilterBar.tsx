import { useState } from 'react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Category } from '@/models/Category';
import { Search, X } from 'lucide-react';

interface FilterBarProps {
    categories: Category[];
    onFilter: (filters: {
        searchTitle: string;
        searchAuthor: string;
        category: string;
        availability: string;
    }) => void;
}

const FilterBar = ({ categories, onFilter }: FilterBarProps) => {
    const [filters, setFilters] = useState({
        searchTitle: '',
        searchAuthor: '',
        category: 'all',
        availability: 'all',
    });

    const handleChange = (key: string, value: string) => {
        const updatedFilters = { ...filters, [key]: value };
        setFilters(updatedFilters);
        onFilter(updatedFilters);
    };

    const resetFilters = () => {
        const defaultFilters = {
            searchTitle: '',
            searchAuthor: '',
            category: 'all',
            availability: 'all',
        };
        setFilters(defaultFilters);
        onFilter(defaultFilters);
    };

    return (
        <div className="bg-muted/20 p-4 rounded-lg border mb-6">
            <h2 className="text-lg font-medium mb-4">Filter Books</h2>

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <div className="space-y-1">
                    <label className="text-sm font-medium" htmlFor="search">
                        Search Title
                    </label>
                    <div className="relative">
                        <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                        <Input
                            id="searchTitle"
                            placeholder="Title"
                            className="pl-8"
                            value={filters.searchTitle}
                            onChange={(e) => handleChange('searchTitle', e.target.value)}
                        />
                    </div>
                </div>

                <div className="space-y-1">
                    <label className="text-sm font-medium" htmlFor="search">
                        Search Author
                    </label>
                    <div className="relative">
                        <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                        <Input
                            id="searchAuthor"
                            placeholder="Author"
                            className="pl-8"
                            value={filters.searchAuthor}
                            onChange={(e) => handleChange('searchAuthor', e.target.value)}
                        />
                    </div>
                </div>

                <div className="space-y-1">
                    <label className="text-sm font-medium" htmlFor="category">
                        Category
                    </label>
                    <Select
                        value={filters.category}
                        onValueChange={(value) => handleChange('category', value)}
                    >
                        <SelectTrigger id="category">
                            <SelectValue placeholder="Select category" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">All Categories</SelectItem>
                            {categories.map((category) => (
                                <SelectItem key={category.id} value={category.id.toString()}>
                                    {category.name}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>

                <div className="space-y-1">
                    <label className="text-sm font-medium" htmlFor="availability">
                        Availability
                    </label>
                    <Select
                        value={filters.availability}
                        onValueChange={(value) => handleChange('availability', value)}
                    >
                        <SelectTrigger id="availability">
                            <SelectValue placeholder="Select availability" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">All Books</SelectItem>
                            <SelectItem value="available">Available Only</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
            </div>

            <div className="flex justify-end mt-4">
                <Button variant="ghost" size="sm" onClick={resetFilters} className="flex items-center">
                    <X size={16} className="mr-1" /> Clear Filters
                </Button>
            </div>
        </div>
    );
};

export default FilterBar;