using System.Net;
using System.Text.Json.Serialization;

namespace WasteFree.Shared.Shared;

public sealed class Result<T>
{    
    public T? ResultModel { get; set; }
    public string ErrorMessage { get; } = string.Empty;
    public HttpStatusCode ResponseCode { get; } = HttpStatusCode.OK;

    [JsonIgnore]
    public bool IsValid => string.IsNullOrEmpty(ErrorMessage) && ResponseCode is HttpStatusCode.OK;

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error, HttpStatusCode errorCode = HttpStatusCode.InternalServerError) => new(error, errorCode);

    public Result(T resultModel)
    {
        ResultModel = resultModel;
    }

    public Result(string errorMessage, HttpStatusCode errorCode)
    {
        ErrorMessage = errorMessage;
        ResponseCode = errorCode;
    }
}