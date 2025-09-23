export interface Result<T> {
    resultModel: T;
    errorCode: string;
    errorMessage: string;
}