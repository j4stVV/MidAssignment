import { BorrowingRequestDto, CreateBorrowingRequestDto } from "@/models/BorrowingRequest";
import api from "./api";

export const createBorrowingRequest = async (requestDto: CreateBorrowingRequestDto): Promise<void> => {
    await api.post("/BorrowingRequest/user/post-request", requestDto);
};

export const getUserBorrowingRequests = async (): Promise<BorrowingRequestDto[]> => {
    const response = await api.get("/BorrowingRequest/user/get-request");
    return response.data;
};

export const getAllBorrowingRequests = async (): Promise<BorrowingRequestDto[]> => {
    const response = await api.get("/BorrowingRequest/superuser/get-all-request");
    return response.data;
};

export const approveBorrowingRequest = async (requestId: string): Promise<void> => {
    await api.post("/BorrowingRequest/approve", requestId);
};

export const rejectBorrowingRequest = async (requestId: string): Promise<void> => {
    await api.post("/BorrowingRequest/reject", requestId);
};