import { Book, BookAPI } from "@/models/Book";
import api from "./api"

export const getBooks = async (pageNumber: number, pageSize: number): Promise<BookAPI> => {
    const allBooks = await api.get(`/Book/all?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    return allBooks.data;
}

export const createBook = async (bookData: Omit<Book, 'id'>): Promise<void> => {
    await api.post(`/Book`, bookData);
};

export const updateBook = async (book: Book): Promise<void> => {
    await api.put(`/Book`, book);
};

export const deleteBook = async (id: string): Promise<void> => {
    await api.delete(`/Book/${id}`);
};

export const filterBooks = async (
    pageNumber: number,
    pageSize: number,
    title?: string,
    author?: string,
    categoryId?: string,
    available?: boolean
): Promise<BookAPI> => {
    const queryParams = new URLSearchParams();
    queryParams.append("pageNumber", pageNumber.toString());
    queryParams.append("pageSize", pageSize.toString());
    if (title) queryParams.append("title", title);
    if (author) queryParams.append("author", author);
    if (categoryId) queryParams.append("categoryId", categoryId);
    if (available !== undefined) queryParams.append("available", available.toString());

    const response = await api.get(`/Book/filter?${queryParams.toString()}`);
    return response.data;
};
