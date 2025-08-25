using System.Net;
using System.Text.Json.Serialization;

namespace BouncerBot.Rest;
public class RestException : Exception
{
    public RestException(HttpStatusCode statusCode, string? reasonPhrase) : base(GetMessage(statusCode, reasonPhrase))
    {
        StatusCode = statusCode;
        ReasonPhrase = reasonPhrase;
    }

    public RestException(HttpStatusCode statusCode, string? reasonPhrase, RestError error) : base(GetMessage(statusCode, reasonPhrase, error.Error.Message))
    {
        StatusCode = statusCode;
        ReasonPhrase = reasonPhrase;
        Error = error;
    }

    private static string GetMessage(HttpStatusCode statusCode, string? reasonPhrase)
    {
        return string.IsNullOrEmpty(reasonPhrase)
            ? $"Response status code does not indicate success: {(int)statusCode}."
            : $"Response status code does not indicate success: {(int)statusCode} ({reasonPhrase}).";
    }

    private static string GetMessage(HttpStatusCode statusCode, string? reasonPhrase, string errorMessage)
    {
        var errorMessageSpan = errorMessage.AsSpan();
        if (string.IsNullOrEmpty(reasonPhrase))
        {
            return errorMessageSpan is [.., '.']
                ? $"Response status code does not indicate success: {(int)statusCode}. {errorMessage}"
                : $"Response status code does not indicate success: {(int)statusCode}. {errorMessage}.";
        }
        else
        {
            return errorMessageSpan is [.., '.']
                ? $"Response status code does not indicate success: {(int)statusCode} ({reasonPhrase}). {errorMessage}"
                : $"Response status code does not indicate success: {(int)statusCode} ({reasonPhrase}). {errorMessage}.";
        }
    }

    public HttpStatusCode StatusCode { get; }

    public string? ReasonPhrase { get; }

    public RestError? Error { get; }
}

public class RestError
{
    [JsonPropertyName("error")]
    public required Error Error { get; set; }
}

public class Error
{
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("code")]
    public required int Code { get; set; }
}
