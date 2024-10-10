using NetCord;

namespace Sharp.Attachments;

public abstract record AttachmentCodeResult
{
    public record Success(string? Language, string Code) : AttachmentCodeResult;

    public record CodeNotFound : AttachmentCodeResult;

    public record FileTooLarge : AttachmentCodeResult;
}

public interface IAttachmentCodeProvider
{
    public ValueTask<AttachmentCodeResult> GetCodeAsync(IReadOnlyList<Attachment> attachments);
}
