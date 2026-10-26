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

using NUnit.Framework;

using Xecrets.Core.Models;
using Xecrets.Core.Public;

// ReSharper disable once CheckNamespace
namespace AxCrypt.Core.Test;

[TestFixture]
public class TestFileTypeClassifier
{
    [TestCase("report.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [TestCase("report.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [TestCase("report.pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [TestCase("report.doc", "application/msword")]
    [TestCase("report.xls", "application/vnd.ms-excel")]
    [TestCase("report.ppt", "application/vnd.ms-powerpoint")]
    [TestCase("report.odt", "application/vnd.oasis.opendocument.text")]
    public void OfficeAndOpenDocumentExtensionsMapToTheirOwnContentType(string fileName, string expectedContentType)
    {
        Assert.That(FileTypeClassifier.DetectContentType(fileName), Is.EqualTo(expectedContentType));
    }

    [TestCase("clip.mp4", "video/mp4")]
    [TestCase("clip.MOV", "video/quicktime")]
    [TestCase("clip.webm", "video/webm")]
    [TestCase("song.mp3", "audio/mpeg")]
    [TestCase("song.m4a", "audio/mp4")]
    [TestCase("song.flac", "audio/flac")]
    [TestCase("photo.heic", "image/heic")]
    [TestCase("photo.HEIF", "image/heif")]
    [TestCase("photo.avif", "image/avif")]
    [TestCase("photo.jfif", "image/jpeg")]
    public void MediaExtensionsMapToTheirOwnContentType(string fileName, string expectedContentType)
    {
        Assert.That(FileTypeClassifier.DetectContentType(fileName), Is.EqualTo(expectedContentType));
    }

    [TestCase("utf-8")]
    [TestCase("utf-16")]
    [TestCase("utf-16BE")]
    [TestCase("utf-32")]
    [TestCase("utf-32BE")]
    public void TextWithByteOrderMarkLooksLikeText(string encodingName)
    {
        Encoding encoding = Encoding.GetEncoding(encodingName);
        byte[] bytes = [.. encoding.Preamble, .. encoding.GetBytes("Räksmörgås – 日本語")];

        Assert.That(FileTypeClassifier.LooksLikeText(bytes), Is.True);
    }

    [Test]
    public void Utf8WithoutByteOrderMarkLooksLikeText()
    {
        Assert.That(FileTypeClassifier.LooksLikeText(new UTF8Encoding(false).GetBytes("Räksmörgås\0\u001b")), Is.True);
    }

    [Test]
    public void EmptySampleLooksLikeText()
    {
        Assert.That(FileTypeClassifier.LooksLikeText([]), Is.True);
    }

    [Test]
    public void InvalidUtf8DoesNotLookLikeText()
    {
        Assert.That(FileTypeClassifier.LooksLikeText([0x41, 0xFF, 0xFE, 0xFD, 0x42]), Is.False);
    }

    [Test]
    public void IncompleteCharacterAtTheEndIsNotAnError()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("abc日");

        Assert.That(FileTypeClassifier.LooksLikeText(bytes[..^1]), Is.True);
    }

    [TestCase(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 })]
    [TestCase(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 })]
    [TestCase(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 })]
    [TestCase(new byte[] { 0x42, 0x4D, 0x00 })]
    [TestCase(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 })]
    [TestCase(new byte[] { 0x49, 0x49, 0x2A, 0x00 })]
    [TestCase(new byte[] { 0x00, 0x00, 0x01, 0x00 })]
    [TestCase(new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63 })]
    public void ImageSignaturesAreRecognized(byte[] sample)
    {
        Assert.That(FileTypeClassifier.HasImageSignature(sample), Is.True);
    }

    [TestCase("notes.txt", "Some text", FileCategory.Text)]
    [TestCase("README", "Some text", FileCategory.Text)]
    [TestCase("notes.xyz", "Some text", FileCategory.Other)]
    [TestCase("document.pdf", "Some text", FileCategory.Other)]
    [TestCase("document.pdf", "%PDF-1.4", FileCategory.Pdf)]
    [TestCase("drawing.svg", "<svg></svg>", FileCategory.Other)]
    [TestCase("photo.png", "Some text", FileCategory.Other)]
    public void ClassifyUsesTheExtensionVerifiedByTheContent(string fileName, string content, FileCategory expected)
    {
        Assert.That(FileTypeClassifier.Classify(fileName, Encoding.ASCII.GetBytes(content)), Is.EqualTo(expected));
    }

    [Test]
    public void ClassifyAcceptsAnyImageSignatureForAnImageExtension()
    {
        Assert.That(FileTypeClassifier.Classify("photo.png", [0xFF, 0xD8, 0xFF, 0xE0]), Is.EqualTo(FileCategory.Image));
    }

    [Test]
    public void ClassifyRejectsATextExtensionWithBinaryContent()
    {
        Assert.That(FileTypeClassifier.Classify("notes.txt", [0x41, 0xFF, 0xFE, 0xFD]), Is.EqualTo(FileCategory.Other));
    }
}
