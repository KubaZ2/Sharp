using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

using Sharp.CompilationResponse;
using Sharp.Responding;

namespace Sharp.Modules;

public class DiagnosticsButtonInteraction(ICompilationFormatter compilationResponseProvider, IResponseProvider responseProvider) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("diagnostics")]
    public InteractionCallback Diagnostics(bool success, int page)
    {
        var message = Context.Message;
        if (message.MessageReference is not { MessageId: var operationId })
            return InteractionCallback.Message(responseProvider.UnknownError<InteractionMessageProperties>("Failed to find reference message."));

        var compilationFormatResult = compilationResponseProvider.CompilationResponse(operationId, page, success);

        return compilationFormatResult switch
        {
            CompilationFormatResult.Success { Embed: var embed, Components: var components }
                => InteractionCallback.ModifyMessage(m =>
                {
                    m.Embeds = [embed];
                    m.Components = components;
                }),
            CompilationFormatResult.Expired => InteractionCallback.Message(responseProvider.UnknownError<InteractionMessageProperties>("The diagnostics are not available anymore.")),
            _ => throw new InvalidOperationException("The compilation response result is invalid."),
        };
    }
}
