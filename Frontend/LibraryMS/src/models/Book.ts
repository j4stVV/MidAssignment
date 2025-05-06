export interface Book {
  id: string;
  title: string;
  author: string;
  description: string;
  isbn: string;
  publishedDate: string;
  categoryId: string;
  categoryName: string;
  quantity: number;
  available: number;
}

export interface BookAPI {
  items: Book[];
  pageNumber: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}