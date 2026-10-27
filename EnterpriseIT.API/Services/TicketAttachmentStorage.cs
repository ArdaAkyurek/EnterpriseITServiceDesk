using System.Text;

namespace EnterpriseIT.API.Services;

public sealed class TicketAttachmentStorage
{
    public const long MaxFileSize = 10 * 1024 * 1024;
    public const long MaxRequestSize = MaxFileSize + 64 * 1024;
    private readonly string _root;

    public TicketAttachmentStorage(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(configuration["Attachments:StoragePath"] ??
            Path.Combine(environment.ContentRootPath, "App_Data", "TicketAttachments")));
        var webRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot")));
        if (_root.Equals(webRoot, StringComparison.OrdinalIgnoreCase) ||
            _root.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Attachment storage must be outside the public web root.");
    }

    public async Task<(string Key, string FileName)> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var name = file.FileName.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 255 || name.IndexOfAny(['/', '\\']) >= 0 ||
            name.Any(char.IsControl) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidDataException("Geçerli, en fazla 255 karakterlik bir dosya adı kullanın.");
        var extension = Path.GetExtension(name).ToLowerInvariant();
        if (extension is not (".pdf" or ".png" or ".jpg" or ".jpeg" or ".txt"))
            throw new InvalidDataException("Yalnızca PDF, PNG, JPG ve UTF-8 TXT dosyaları yüklenebilir.");
        if (file.Length <= 0 || file.Length > MaxFileSize)
            throw new InvalidDataException("Dosya boş olamaz ve 10 MB sınırını aşamaz.");

        using var content = new MemoryStream();
        await using (var source = file.OpenReadStream())
        {
            var buffer = new byte[81920];
            int count;
            while ((count = await source.ReadAsync(buffer, cancellationToken)) != 0)
            {
                if (content.Length + count > MaxFileSize)
                    throw new InvalidDataException("Dosya 10 MB sınırını aşamaz.");
                await content.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
            }
        }
        if (content.Length != file.Length || !MatchesType(extension, content.ToArray()))
            throw new InvalidDataException("Dosya içeriği seçilen dosya türüyle uyuşmuyor veya dosya geçersiz.");

        Directory.CreateDirectory(_root);
        var key = Guid.NewGuid().ToString("N");
        var path = Resolve(key);
        try
        {
            await using var destination = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            content.Position = 0;
            await content.CopyToAsync(destination, cancellationToken);
            await destination.FlushAsync(cancellationToken);
        }
        catch
        {
            File.Delete(path); // No database reference exists yet.
            throw;
        }
        return (key, name);
    }

    public Stream? OpenRead(string key)
    {
        if (!Guid.TryParseExact(key, "N", out _)) return null;
        try { return new FileStream(Resolve(key), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true); }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    public void DeleteUnreferenced(string key) => File.Delete(Resolve(key));

    private string Resolve(string key)
    {
        if (!Guid.TryParseExact(key, "N", out _)) throw new InvalidDataException("Invalid storage key.");
        return Path.Combine(_root, key);
    }

    private static bool MatchesType(string extension, byte[] content)
    {
        if (extension == ".pdf") return content.AsSpan().StartsWith("%PDF-"u8);
        if (extension == ".png") return content.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        if (extension is ".jpg" or ".jpeg") return content.AsSpan().StartsWith(new byte[] { 255, 216, 255 });
        try
        {
            var text = new UTF8Encoding(false, true).GetString(content);
            return text.All(c => !char.IsControl(c) || c is '\r' or '\n' or '\t');
        }
        catch (DecoderFallbackException) { return false; }
    }
}
