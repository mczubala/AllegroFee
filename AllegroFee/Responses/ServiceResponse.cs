using System.Net;

namespace AllegroFee.Responses;

public class ServiceResponse<T>
{
    public T Data { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string ErrorMessage { get; set; }

    public ServiceResponse(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Data = data;
        StatusCode = statusCode;
    }

    public ServiceResponse(string errorMessage, HttpStatusCode statusCode)
    {
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }
}