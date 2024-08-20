using NetCord;

namespace Sharp.Attachments;

public abstract record AttachmentCodeResult
{
    public record Success(string? Language, string Code) : AttachmentCodeResult;

    public record CodeNotFound : AttachmentCodeResult;
}

public interface IAttachmentCodeProvider
{
    public ValueTask<AttachmentCodeResult> GetCodeAsync(IEnumerable<Attachment> attachments);
}
