using System;

namespace SteamKeyActivator.Client.Models
{
    public sealed class ErrorResponse : Response
    {
        public override bool IsSuccess => false;

        public string Message { get; }

        public ErrorResponse()
        {
        }

        public ErrorResponse(string message)
        {
            Message = message;
        }

        public ErrorResponse(Exception exception)
        {
            Message = exception.Message;
        }
    }
}
