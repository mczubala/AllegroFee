namespace MFC.Responses;

public class ServiceResponse<T>
{
    public T Data { get; private set; }
    public string Message { get; private set; }
    public ServiceStatusCodes.StatusCode ResponseStatus { get; private set; }
    
    public ServiceResponse(T data, string message, ServiceStatusCodes.StatusCode responseStatus = ServiceStatusCodes.StatusCode.Success)
    {
        Data = data;
        Message = message;
        ResponseStatus = responseStatus;
    }
    public ServiceResponse(T data, ServiceStatusCodes.StatusCode responseStatus = ServiceStatusCodes.StatusCode.Success)
    {
        Data = data;
        ResponseStatus = responseStatus;
    }
    
    public ServiceResponse(string message, ServiceStatusCodes.StatusCode responseStatus)
    {
        Message = message;
        ResponseStatus = responseStatus;
    }
}

public class ServiceStatusCodes
{
    public enum StatusCode
    {
        Success = 0,
        Error = 1,
        NotFound = 2,
        ValidationError = 3,
    }
}