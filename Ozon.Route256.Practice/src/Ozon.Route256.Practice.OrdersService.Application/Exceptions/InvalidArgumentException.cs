namespace Ozon.Route256.Practice.OrdersService.Application.Exceptions;

public class InvalidArgumentException : Exception
{
    public InvalidArgumentException(string exceptionMessage)
    {
        Message = exceptionMessage;
    }

    public override string Message { get; }
}
