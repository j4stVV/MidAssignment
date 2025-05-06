import { Category, CategoryAPI } from "@/models/Category";
import api from "./api"

export const getCategoriesWithPage = async (pageNumber: number, pageSize: number): Promise<CategoryAPI> => {
    const allCategories = await api.get(`/Category?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    return allCategories.data;
}

export const getCategories = async (): Promise<Category[]> => {
    const allCategories = await api.get(`/Category/all`);
    return allCategories.data;
}

export const getCategoryById = async (id: string): Promise<Category> => {
    const response = await api.get(`/Category/${id}`);
    return response.data;
};

export const createCategory = async (categoryData: { name: string }): Promise<void> => {
    await api.post(`/Category`, categoryData);
};

export const updateCategory = async (category: Category): Promise<void> => {
    await api.put("/Category", category);
};

export const deleteCategory = async (id: string): Promise<void> => {
    await api.delete(`/Category/${id}`);
};