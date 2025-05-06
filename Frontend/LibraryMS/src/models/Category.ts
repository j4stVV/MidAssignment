export interface Category {
    id: string;
    name: string;

}
export interface CategoryAPI {
    items: Category[];
    pageNumber: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
}