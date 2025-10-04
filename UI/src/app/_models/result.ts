export interface Result<T> {
    resultModel: T;
    errorCode: string;
    errorMessage: string;
    pager?: Pager;
}

export interface Pager {
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}