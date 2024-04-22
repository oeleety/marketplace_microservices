namespace Ozon.Route256.Practice.OrdersService.Application.Exceptions;

public class UnprocessableException : Exception
{
    public UnprocessableException(string exceptionMessage)
    {
        Message = exceptionMessage;
    }

    public override string Message { get; }
}
