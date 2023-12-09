using System.Net;

namespace MFC.Responses;

public class ExternalApiResponse<T>
{
    public T Data { get; set; }
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    public ExternalApiResponse(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Data = data;
        StatusCode = statusCode;
    }
    
    public ExternalApiResponse(string message, HttpStatusCode statusCode)
    {
        Message = message;
        StatusCode = statusCode;
    }
}