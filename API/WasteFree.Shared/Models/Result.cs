using System.Net;
using System.Text.Json.Serialization;

namespace WasteFree.Shared.Models;

/// <summary>
/// Wrapper used by API responses to return either a successful result or an error.
/// The generic <typeparamref name="T"/> contains the payload when the request succeeds.
/// </summary>
public sealed class Result<T>
{    
    /// <summary>
    /// The payload returned when the operation succeeded. Null when the result represents a failure.
    /// </summary>
    public T? ResultModel { get; set; }

    /// <summary>
    /// Human-readable error message. When empty the result is considered successful.
    /// This value is typically localized on the server before being returned to the client.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Machine-readable error code identifying the failure type. Useful for client-side handling and localization lookup.
    /// </summary>
    public string ErrorCode { get; } = string.Empty;
    
    [JsonIgnore]
    public HttpStatusCode ResponseCode { get; } = HttpStatusCode.OK;


    public Pager? Pager { get; set; }


    [JsonIgnore]
    public bool IsValid => string.IsNullOrEmpty(ErrorMessage) && ResponseCode is HttpStatusCode.OK;

    /// <summary>
    /// Create a successful result containing the provided value.
    /// </summary>
    /// <param name="value">The result payload.</param>
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