using Grpc.Core.Interceptors;
using Grpc.Core;
using Ozon.Route256.Practice.OrdersService.Exceptions;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure;

internal sealed class LoggerInterceptor : Interceptor
{
    private readonly ILogger<LoggerInterceptor> _logger;

    public LoggerInterceptor(ILogger<LoggerInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest,
            TResponse> continuation)
    {
        _logger.LogInformation("Request {request}", request);

        try
        {
            var response = await base.UnaryServerHandler(request, context, continuation);

            _logger.LogInformation("Response {response}", response);

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, $"{nameof(RpcException)} happened");
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, $"{nameof(NotFoundException)} happened");
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (UnprocessableException ex)
        {
            _logger.LogError(ex, $"{nameof(UnprocessableException)} happened");
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (InvalidArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(InvalidArgumentException)} happened");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, $"{nameof(OperationCanceledException)} happened");
            throw new RpcException(new Status(StatusCode.Cancelled, ex.Message));
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Some exception happened");
            throw new RpcException(new Status(StatusCode.Unknown, ex.Message));
        }
    }
}
