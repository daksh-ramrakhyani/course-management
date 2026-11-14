namespace CourseManagement.Common;


public class Result<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public T? Data { get; set; }


    public static Result<T> Success(string message, T data)
    {
        return new Result<T>
        {
            IsSuccess= true,
            Message = message,
            Data = data
        };
    }

    public static Result<T> Failure(string message, string errorCode)
    {
        return new Result<T>
        {
            IsSuccess= false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}