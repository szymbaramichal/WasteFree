using System.Net;
using System.Text.Json.Serialization;

namespace WasteFree.Shared.Models;

public sealed class Result<T>
{    
    public T? ResultModel { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; } = string.Empty;
    private HttpStatusCode ResponseCode { get; } = HttpStatusCode.OK;
    public Pager Pager { get; set; }

    [JsonIgnore]
    public bool IsValid => string.IsNullOrEmpty(ErrorMessage) && ResponseCode is HttpStatusCode.OK;

    public static Result<T> Success(T value) => new(value);
    public static Result<T> PaginatedSuccess(T value, Pager pager) => new(value) { Pager = pager };

    public static Result<T> Failure(string errorCode, HttpStatusCode responseCode = HttpStatusCode.InternalServerError) => new(errorCode, responseCode);

    private Result(T resultModel)
    {
        ResultModel = resultModel;
    }

    private Result(string errorCode, HttpStatusCode responseCode)
    {
        ErrorCode = errorCode;
        ResponseCode = responseCode;
    }
}