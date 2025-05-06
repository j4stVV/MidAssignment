export interface BorrowingRequestDto {
    id: string; // Guid
    requestorId: string; // Guid
    requestorName?: string; // Từ Requestor
    requestedDate: string; // ISO date string
    status: RequestStatus; // Enum
    approverId?: string; // Guid, nullable
    approverName?: string; // Từ Approver, nullable
    details: BorrowingRequestDetailDto[];
}

export interface BorrowingRequestDetailDto {
    id: string; // Guid
    bookId: string; // Guid
    bookTitle?: string; // Từ Book.Title
}

export enum RequestStatus {
    Approved = 'Approved',
    Rejected = 'Rejected',
    Waiting = 'Waiting'
}

export interface CreateBorrowingRequestDto {
    bookIds: string[]; // Guid của các sách
}