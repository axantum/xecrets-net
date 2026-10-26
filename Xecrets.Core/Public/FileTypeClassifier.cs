#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 *
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
 */

#endregion Coypright and GPL License

using System.Text;

namespace Xecrets.Core.Public;

/// <summary>
/// Classifies files by their name extension, verified by a sample of their content.
/// </summary>
public static class FileTypeClassifier
{
    /// <summary>
    /// The content type used when the extension is not recognized.
    /// </summary>
    public const string UnknownContentType = "application/octet-stream";

    private static readonly Dictionary<string, string> _contentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".3gp", "video/3gpp" },
        { ".aac", "audio/aac" },
        { ".adoc", "text/plain" },
        { ".amr", "audio/amr" },
        { ".asc", "text/plain" },
        { ".asm", "text/plain" },
        { ".astro", "text/plain" },
        { ".avi", "video/x-msvideo" },
        { ".avif", "image/avif" },
        { ".bash", "application/x-sh" },
        { ".bat", "text/plain" },
        { ".bib", "text/plain" },
        { ".bmp", "image/bmp" },
        { ".c", "text/x-c" },
        { ".cc", "text/x-c++" },
        { ".cfg", "text/plain" },
        { ".cjs", "text/javascript" },
        { ".clj", "text/plain" },
        { ".cljc", "text/plain" },
        { ".cljs", "text/plain" },
        { ".cmd", "text/plain" },
        { ".cnf", "text/plain" },
        { ".conf", "text/plain" },
        { ".config", "text/plain" },
        { ".cpp", "text/x-c++" },
        { ".cs", "text/plain" },
        { ".csproj", "application/xml" },
        { ".css", "text/css" },
        { ".csv", "text/csv" },
        { ".cts", "text/plain" },
        { ".cxx", "text/x-c++" },
        { ".d", "text/plain" },
        { ".dart", "text/plain" },
        { ".diff", "text/x-diff" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".editorconfig", "text/plain" },
        { ".env", "text/plain" },
        { ".epub", "application/epub+zip" },
        { ".erl", "text/plain" },
        { ".ex", "text/plain" },
        { ".exs", "text/plain" },
        { ".f", "text/plain" },
        { ".f77", "text/plain" },
        { ".f90", "text/plain" },
        { ".f95", "text/plain" },
        { ".fish", "text/plain" },
        { ".flac", "audio/flac" },
        { ".for", "text/plain" },
        { ".fs", "text/plain" },
        { ".fsi", "text/plain" },
        { ".fsproj", "application/xml" },
        { ".fsx", "text/plain" },
        { ".gif", "image/gif" },
        { ".go", "text/x-go" },
        { ".gql", "text/plain" },
        { ".gradle", "text/plain" },
        { ".graphql", "text/plain" },
        { ".groovy", "text/plain" },
        { ".h", "text/x-c" },
        { ".heic", "image/heic" },
        { ".heif", "image/heif" },
        { ".hh", "text/x-c++" },
        { ".hpp", "text/x-c++" },
        { ".hrl", "text/plain" },
        { ".hs", "text/plain" },
        { ".htm", "text/html" },
        { ".html", "text/html" },
        { ".hxx", "text/x-c++" },
        { ".ico", "image/x-icon" },
        { ".ini", "text/plain" },
        { ".java", "text/x-java-source" },
        { ".jfif", "image/jpeg" },
        { ".jl", "text/plain" },
        { ".jpeg", "image/jpeg" },
        { ".jpg", "image/jpeg" },
        { ".js", "text/javascript" },
        { ".json", "application/json" },
        { ".jsonc", "application/json" },
        { ".jsx", "text/javascript" },
        { ".kt", "text/plain" },
        { ".kts", "text/plain" },
        { ".less", "text/plain" },
        { ".lhs", "text/plain" },
        { ".log", "text/plain" },
        { ".lua", "text/plain" },
        { ".m", "text/plain" },
        { ".m4a", "audio/mp4" },
        { ".m4v", "video/mp4" },
        { ".markdown", "text/markdown" },
        { ".md", "text/markdown" },
        { ".mid", "audio/midi" },
        { ".midi", "audio/midi" },
        { ".mjs", "text/javascript" },
        { ".mkv", "video/x-matroska" },
        { ".mm", "text/plain" },
        { ".mov", "video/quicktime" },
        { ".mp3", "audio/mpeg" },
        { ".mp4", "video/mp4" },
        { ".mpeg", "video/mpeg" },
        { ".mpg", "video/mpeg" },
        { ".mts", "text/plain" },
        { ".ndjson", "application/x-ndjson" },
        { ".nim", "text/plain" },
        { ".odp", "application/vnd.oasis.opendocument.presentation" },
        { ".ods", "application/vnd.oasis.opendocument.spreadsheet" },
        { ".odt", "application/vnd.oasis.opendocument.text" },
        { ".oga", "audio/ogg" },
        { ".ogg", "audio/ogg" },
        { ".opus", "audio/ogg" },
        { ".org", "text/plain" },
        { ".pas", "text/plain" },
        { ".patch", "text/x-diff" },
        { ".pdf", "application/pdf" },
        { ".php", "application/x-httpd-php" },
        { ".pl", "text/plain" },
        { ".pm", "text/plain" },
        { ".png", "image/png" },
        { ".pp", "text/plain" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".properties", "text/plain" },
        { ".props", "application/xml" },
        { ".proto", "text/plain" },
        { ".ps1", "text/plain" },
        { ".psd1", "text/plain" },
        { ".psm1", "text/plain" },
        { ".py", "text/x-python" },
        { ".pyw", "text/x-python" },
        { ".r", "text/plain" },
        { ".rb", "text/plain" },
        { ".reg", "text/plain" },
        { ".rs", "text/plain" },
        { ".rst", "text/x-rst" },
        { ".rtf", "application/rtf" },
        { ".s", "text/plain" },
        { ".sass", "text/plain" },
        { ".sc", "text/plain" },
        { ".scala", "text/plain" },
        { ".scss", "text/plain" },
        { ".sh", "application/x-sh" },
        { ".sln", "text/plain" },
        { ".slnx", "application/xml" },
        { ".sql", "application/sql" },
        { ".svelte", "text/plain" },
        { ".svg", "image/svg+xml" },
        { ".swift", "text/plain" },
        { ".targets", "application/xml" },
        { ".tex", "application/x-tex" },
        { ".text", "text/plain" },
        { ".thrift", "text/plain" },
        { ".tif", "image/tiff" },
        { ".tiff", "image/tiff" },
        { ".toml", "application/toml" },
        { ".ts", "text/plain" },
        { ".tsv", "text/tab-separated-values" },
        { ".tsx", "text/plain" },
        { ".txt", "text/plain" },
        { ".vb", "text/plain" },
        { ".vbproj", "application/xml" },
        { ".vue", "text/plain" },
        { ".wav", "audio/wav" },
        { ".webm", "video/webm" },
        { ".webp", "image/webp" },
        { ".wma", "audio/x-ms-wma" },
        { ".wmv", "video/x-ms-wmv" },
        { ".xaml", "application/xml" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".xml", "application/xml" },
        { ".yaml", "application/yaml" },
        { ".yml", "application/yaml" },
        { ".zig", "text/plain" },
        { ".zip", "application/zip" },
        { ".zsh", "application/x-sh" },
    };

    private static readonly HashSet<string> _textualApplicationTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/json",
        "application/x-ndjson",
        "application/xml",
        "application/yaml",
        "application/toml",
        "application/x-httpd-php",
        "application/x-sh",
        "application/sql",
        "application/x-tex",
    };

    /// <summary>
    /// Get the MIME content type for a file name, based on its extension.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The content type, or <see cref="UnknownContentType"/> if the extension is not recognized.</returns>
    public static string DetectContentType(string fileName)
    {
        string extension = Path.GetExtension(fileName);
        return _contentTypes.GetValueOrDefault(extension, UnknownContentType);
    }

    /// <summary>
    /// Check if a file name has a recognized text extension.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns><see langword="true"/> if the extension is a recognized text extension.</returns>
    public static bool IsTextExtension(string fileName)
    {
        string contentType = DetectContentType(fileName);
        return contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) || _textualApplicationTypes.Contains(contentType);
    }

    /// <summary>
    /// Check if a file name has a recognized image extension.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns><see langword="true"/> if the extension is a recognized image extension.</returns>
    public static bool IsImageExtension(string fileName) => DetectContentType(fileName).StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Classify a file by its name extension, verified by a sample of the start of its content. A file without an
    /// extension is classified as text if the sample looks like text.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="sample">The start of the content.</param>
    /// <returns>The category, or <see cref="FileCategory.Other"/> if the sample contradicts the extension.</returns>
    public static FileCategory Classify(string fileName, ReadOnlySpan<byte> sample)
    {
        string extension = Path.GetExtension(fileName);
        if (extension.Length == 0 || IsTextExtension(fileName))
        {
            return LooksLikeText(sample) ? FileCategory.Text : FileCategory.Other;
        }
        if (IsImageExtension(fileName))
        {
            return HasImageSignature(sample) ? FileCategory.Image : FileCategory.Other;
        }
        if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return sample.StartsWith("%PDF-"u8) ? FileCategory.Pdf : FileCategory.Other;
        }
        return FileCategory.Other;
    }

    /// <summary>
    /// Check if a sample can be decoded as text without errors, in the encoding indicated by any byte order mark, or
    /// otherwise as UTF-8. An incomplete character at the end of the sample is not an error.
    /// </summary>
    /// <param name="sample">The start of the content.</param>
    /// <returns><see langword="true"/> if the sample decodes without errors.</returns>
    public static bool LooksLikeText(ReadOnlySpan<byte> sample)
    {
        Encoding encoding = Encoding.GetEncoding(DetectEncoding(sample).CodePage, EncoderFallback.ReplacementFallback, DecoderFallback.ExceptionFallback);
        try
        {
            _ = encoding.GetDecoder().GetCharCount(sample, flush: false);
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Detect the text encoding from a byte order mark, defaulting to UTF-8 without one.
    /// </summary>
    /// <param name="bytes">The start of the content.</param>
    /// <returns>The detected encoding. Its preamble is the byte order mark, if any.</returns>
    private static Encoding DetectEncoding(ReadOnlySpan<byte> bytes)
    {
        using StreamReader reader = new(new MemoryStream(bytes[..Math.Min(bytes.Length, 4)].ToArray()), new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);
        _ = reader.Peek();
        return reader.CurrentEncoding;
    }

    /// <summary>
    /// Check if a sample starts with the signature of any recognized image format.
    /// </summary>
    /// <param name="sample">The start of the content.</param>
    /// <returns><see langword="true"/> if an image signature is found.</returns>
    public static bool HasImageSignature(ReadOnlySpan<byte> sample)
    {
        return sample.StartsWith(PngSignature)
            || sample.StartsWith(JpegSignature)
            || sample.StartsWith("GIF87a"u8)
            || sample.StartsWith("GIF89a"u8)
            || sample.StartsWith("BM"u8)
            || (sample.StartsWith("RIFF"u8) && sample.Length >= 12 && sample[8..12].SequenceEqual("WEBP"u8))
            || sample.StartsWith(TiffLittleEndianSignature)
            || sample.StartsWith(TiffBigEndianSignature)
            || sample.StartsWith(IcoSignature)
            || (sample.Length >= 8 && sample[4..8].SequenceEqual("ftyp"u8)); // HEIC, HEIF, AVIF
    }

    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static ReadOnlySpan<byte> JpegSignature => [0xFF, 0xD8, 0xFF];

    private static ReadOnlySpan<byte> TiffLittleEndianSignature => [0x49, 0x49, 0x2A, 0x00];

    private static ReadOnlySpan<byte> TiffBigEndianSignature => [0x4D, 0x4D, 0x00, 0x2A];

    private static ReadOnlySpan<byte> IcoSignature => [0x00, 0x00, 0x01, 0x00];
}
