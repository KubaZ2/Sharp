using NetCord;

namespace Sharp.Attachments;

public class AttachmentCodeProvider(IHttpClientFactory httpClientFactory) : IAttachmentCodeProvider
{
    public async ValueTask<AttachmentCodeResult> GetCodeAsync(IEnumerable<Attachment> attachments)
    {
        var attachment = attachments.FirstOrDefault();

        if (attachment is null)
            return new AttachmentCodeResult.CodeNotFound();

        var extension = Path.GetExtension(attachment.FileName);

        int extensionLength = extension.Length;
        if (extensionLength is 0 || extensionLength is 4 && extension.EndsWith("txt"))
            extension = null;
        else
            extension = extension[1..];

        string code;
        using (var client = httpClientFactory.CreateClient())
        using (StreamReader reader = new(await client.GetStreamAsync(attachment.Url)))
            code = await reader.ReadToEndAsync();

        return new AttachmentCodeResult.Success(extension, code);
    }
}
