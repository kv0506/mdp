namespace MDP.Exceptions
{
    public class MDPException : Exception
    {
        public MDPException(ErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public MDPException(ErrorCode errorCode, Exception exception) : base(exception.Message, exception)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode ErrorCode { get; set; }
    }
}