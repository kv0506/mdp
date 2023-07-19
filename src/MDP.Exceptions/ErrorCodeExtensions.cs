namespace MDP.Exceptions;

public static class ErrorCodeExtensions
{
    public static int ToHttpStatusCode(this ErrorCode errorCode)
    {
        switch (errorCode)
        {
            case ErrorCode.NotFound:
                return 404;
        }

        return 400;
    }
}