namespace Ozon.Route256.Practice.OrdersService.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string exceptionMessage)
    {
        Message = exceptionMessage;
    }

    public override string Message { get; }
}
