using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

using Sharp.Diagnostics;
using Sharp.Responding;

namespace Sharp.Modules;

public class DiagnosticsButtonInteraction(IDiagnosticsFormatter diagnosticsFormatter, IResponseProvider responseProvider) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("diagnostics")]
    public InteractionCallback Diagnostics(int page)
    {
        var message = Context.Message;
        if (message.MessageReference is not { MessageId: var operationId })
            return InteractionCallback.Message(responseProvider.Error<InteractionMessageProperties>("Failed to find reference message."));

        var formatResult = diagnosticsFormatter.FormatDiagnostics(operationId, page);

        if (formatResult.Expired)
            return InteractionCallback.Message(responseProvider.Error<InteractionMessageProperties>("The diagnostics are not available anymore."));

        return InteractionCallback.ModifyMessage(m =>
        {
            m.Embeds = formatResult.Embeds;
            m.Components = formatResult.Components;
        });
    }
}
