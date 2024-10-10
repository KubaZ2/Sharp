using Microsoft.Extensions.Options;

using NetCord;

namespace Sharp.Attachments;

public class AttachmentCodeProvider(IHttpClientFactory httpClientFactory, IOptions<Options> options) : IAttachmentCodeProvider
{
    public async ValueTask<AttachmentCodeResult> GetCodeAsync(IReadOnlyList<Attachment> attachments)
    {
        if (attachments.Count is 0)
            return new AttachmentCodeResult.CodeNotFound();

        var attachment = attachments[0];

        if (attachment.Size > options.Value.MaxFileSize)
            return new AttachmentCodeResult.FileTooLarge();

        var extension = Path.GetExtension(attachment.FileName);

        int extensionLength = extension.Length;
        if (extensionLength is 0 || extensionLength is 4 && extension.EndsWith("txt"))
            extension = null;
        else
            extension = extension[1..];

        string code;
        using (var client = httpClientFactory.CreateClient())
            code = await client.GetStringAsync(attachment.Url);

        return new AttachmentCodeResult.Success(extension, code);
    }
}
