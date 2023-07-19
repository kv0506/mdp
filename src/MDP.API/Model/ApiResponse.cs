namespace MDP.API.Model;

public class ApiResponse
{
    public bool IsSuccess { get; set; }

    public IEnumerable<Error> Errors { get; set; }
}

public class ApiResponse<T> : ApiResponse
{
    public T Result { get; set; }
}

public class Error
{
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Message { get; }
    public string Code { get; }
}