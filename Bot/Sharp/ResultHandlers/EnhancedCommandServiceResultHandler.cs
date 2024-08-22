
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord.Gateway;
using NetCord.Hosting.Services.Commands;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.Commands;

using Sharp.Responding;

namespace Sharp.ResultHandlers;

public class EnhancedCommandServiceResultHandler<TContext> : ICommandResultHandler<TContext> where TContext : ICommandContext
{
    public ValueTask HandleResultAsync(IExecutionResult result, TContext context, GatewayClient client, ILogger logger, IServiceProvider services)
    {
        if (result is not IFailResult failResult || result is NotFoundResult)
            return default;

        var resultMessage = failResult.Message;

        var message = context.Message;

        if (failResult is IExceptionResult exceptionResult)
            logger.LogError(exceptionResult.Exception, "Execution of a command with content '{Content}' failed with an exception", message.Content);
        else
            logger.LogDebug("Execution of a command with content '{Content}' failed with '{Message}'", message.Content, resultMessage);

        var responseProvider = services.GetRequiredService<IResponseProvider>();

        var response = responseProvider.UnknownError<ReplyMessageProperties>(resultMessage);

        return new(message.ReplyAsync(response));
    }
}
