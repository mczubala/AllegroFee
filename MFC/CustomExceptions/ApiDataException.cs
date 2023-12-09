using System;

public class ApiDataException : Exception
{
    public int ErrorCode { get; private set; }

    public ApiDataException(string message, int errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}