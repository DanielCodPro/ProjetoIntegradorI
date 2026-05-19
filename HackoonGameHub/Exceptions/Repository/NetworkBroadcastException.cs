namespace Repository.Exceptions;

public class NetworkBroadcastException : Exception
{
    public NetworkBroadcastException()
    {
    }

    public NetworkBroadcastException(string? message) : base(message)
    {
    }

    public NetworkBroadcastException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}