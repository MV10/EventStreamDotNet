using System;

namespace Demo
{
    public class APIResult<T> 
    {
        public APIResult()
        { }

        public APIResult(T output)
        {
            Output = output;
            Success = true;
        }

        public APIResult(string message)
        {
            Message = message;
            Success = false;
        }

        public APIResult(bool success, T successfulOutput, string unsuccessfulMessage)
        {
            Success = success;
            if (success) Output = successfulOutput;
            if (!success) Message = unsuccessfulMessage;
        }

        public APIResult(Exception ex)
        {
            var message = ex.Message;
            if (ex.InnerException != null) message += "; InnerException " + ex.InnerException.Message;
            Message = message;
            Success = false;
        }

        public T Output { get; }
        public bool Success { get; }
        public string Message { get; }
    }
}
