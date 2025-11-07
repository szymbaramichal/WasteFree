export interface Result<T> {
    resultModel: T | null;
    errorCode: string | null;
    errorMessage: string | null;
}

export interface PaginatedResult<T> extends Result<T> {
    pager: Pager | null;
}

export interface Pager {
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}