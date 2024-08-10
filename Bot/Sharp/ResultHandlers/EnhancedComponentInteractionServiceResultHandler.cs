
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord.Gateway;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ComponentInteractions;

using Sharp.Responding;

namespace Sharp.ResultHandlers;

public class EnhancedComponentInteractionServiceResultHandler<TContext> : IComponentInteractionResultHandler<TContext> where TContext : IComponentInteractionContext
{
    public ValueTask HandleResultAsync(IExecutionResult result, TContext context, GatewayClient? client, ILogger logger, IServiceProvider services)
    {
        if (result is not IFailResult failResult)
            return default;

        var resultMessage = failResult.Message;

        var interaction = context.Interaction;

        if (failResult is IExceptionResult exceptionResult)
            logger.LogError(exceptionResult.Exception, "Execution of an interaction of custom ID '{Id}' failed with an exception", interaction.Id);
        else
            logger.LogDebug("Execution of an interaction of custom ID '{Id}' failed with '{Message}'", interaction.Id, resultMessage);

        var responseProvider = services.GetRequiredService<IResponseProvider>();

        var response = responseProvider.UnknownError<InteractionMessageProperties>(resultMessage);

        return new(interaction.SendResponseAsync(InteractionCallback.Message(response)));
    }
}
