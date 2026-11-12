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
public class TestInMemoryOrFileStream
{
    private MemoryStream _file = new();

    private bool _isFileOpened;

    [SetUp]
    public void Setup()
    {
        _file = new MemoryStream();
        _isFileOpened = false;
    }

    private Stream OpenFile()
    {
        _isFileOpened = true;
        return _file;
    }

    [Test]
    public void SmallTextIsKeptInMemory()
    {
        using InMemoryOrFileStream stream = new("notes.txt", new Dictionary<FileCategory, long> { { FileCategory.Text, 100 } }, OpenFile);

        stream.Write(Encoding.UTF8.GetBytes("Some text"));

        stream.Commit();
        Assert.That(stream.InMemoryCategory, Is.EqualTo(FileCategory.Text));
        Assert.That(stream.Category, Is.EqualTo(FileCategory.Text));
        Assert.That(_isFileOpened, Is.False);
        Assert.That(stream.Data.ToArray(), Is.EqualTo(Encoding.UTF8.GetBytes("Some text")));
    }

    [TestCase("README")]
    [TestCase("notes.xyz")]
    public void TextWithoutRecognizedExtensionIsKeptInMemory(string fileName)
    {
        using InMemoryOrFileStream stream = new(fileName, new Dictionary<FileCategory, long> { { FileCategory.Text, 100 } }, OpenFile);

        stream.Write(Encoding.UTF8.GetBytes("Some text"));

        stream.Commit();
        Assert.That(stream.InMemoryCategory, Is.EqualTo(FileCategory.Text));
    }

    [Test]
    public void TextOverTheLimitIsWrittenCompletelyToTheFile()
    {
        byte[] text = Encoding.UTF8.GetBytes(new string('a', 10000));
        using InMemoryOrFileStream stream = new("notes.txt", new Dictionary<FileCategory, long> { { FileCategory.Text, 5000 } }, OpenFile);

        stream.Write(text.AsSpan(0, 3000));
        stream.Write(text.AsSpan(3000));

        stream.Commit();
        Assert.That(stream.InMemoryCategory, Is.Null);
        Assert.That(stream.Category, Is.EqualTo(FileCategory.Text));
        Assert.That(stream.Data.IsEmpty, Is.True);
        Assert.That(_file.ToArray(), Is.EqualTo(text));
    }

    [Test]
    public void CategoryThatIsNotRequestedIsWrittenToTheFile()
    {
        using InMemoryOrFileStream stream = new("document.pdf", new Dictionary<FileCategory, long> { { FileCategory.Text, 100 } }, OpenFile);

        stream.Write(Encoding.ASCII.GetBytes("%PDF-1.4"));

        stream.Commit();
        Assert.That(stream.InMemoryCategory, Is.Null);
        Assert.That(stream.Category, Is.EqualTo(FileCategory.Pdf));
        Assert.That(_file.ToArray(), Is.EqualTo(Encoding.ASCII.GetBytes("%PDF-1.4")));
    }

    [Test]
    public void WithoutCategoriesEverythingGoesDirectlyToTheFile()
    {
        using InMemoryOrFileStream stream = new("notes.txt", new Dictionary<FileCategory, long>(), OpenFile);

        stream.Write(Encoding.UTF8.GetBytes("Some"));

        Assert.That(_isFileOpened, Is.True);

        stream.Write(Encoding.UTF8.GetBytes(" text"));

        stream.Commit();
        Assert.That(stream.InMemoryCategory, Is.Null);
        Assert.That(_file.ToArray(), Is.EqualTo(Encoding.UTF8.GetBytes("Some text")));
    }

    [Test]
    public void WithoutCategoriesEmptyDataStillCreatesTheFile()
    {
        using InMemoryOrFileStream stream = new("notes.txt", new Dictionary<FileCategory, long>(), OpenFile);

        stream.Commit();
        Assert.That(stream.InMemoryCategory, Is.Null);
        Assert.That(_isFileOpened, Is.True);
        Assert.That(_file.ToArray(), Is.Empty);
    }

    [Test]
    public void OtherAcceptsAnything()
    {
        byte[] data = [0x00, 0xFF, 0xFE, 0x01];
        using InMemoryOrFileStream stream = new("data.bin", new Dictionary<FileCategory, long> { { FileCategory.Text, 100 }, { FileCategory.Other, 100 } }, OpenFile);

        stream.Write(data);

        stream.Commit();
        Assert.That(stream.InMemoryCategory, Is.EqualTo(FileCategory.Other));
        Assert.That(stream.Data.ToArray(), Is.EqualTo(data));
    }

    [TestCase("utf-16")]
    [TestCase("utf-16BE")]
    [TestCase("utf-32")]
    [TestCase("utf-32BE")]
    [TestCase("utf-8")]
    public void TextIsKeptAsItWasWritten(string encodingName)
    {
        Encoding encoding = Encoding.GetEncoding(encodingName);
        byte[] text = [.. encoding.Preamble, .. encoding.GetBytes("Räksmörgås – 日本語")];
        using InMemoryOrFileStream stream = new("notes.txt", new Dictionary<FileCategory, long> { { FileCategory.Text, 1000 } }, OpenFile);

        stream.Write(text);

        stream.Commit();
        Assert.That(stream.InMemoryCategory, Is.EqualTo(FileCategory.Text));
        Assert.That(stream.Data.ToArray(), Is.EqualTo(text));
    }

    [Test]
    public void TextThatIsNotTextAfterTheSampleIsWrittenToTheFile()
    {
        byte[] data = [.. Encoding.UTF8.GetBytes(new string('a', 5000)), 0xFF, 0xFE, 0xFD];
        using InMemoryOrFileStream stream = new("notes.txt", new Dictionary<FileCategory, long> { { FileCategory.Text, 10000 } }, OpenFile);

        stream.Write(data.AsSpan(0, 5000));
        stream.Write(data.AsSpan(5000));

        Assert.That(_isFileOpened, Is.False);

        stream.Commit();
        Assert.That(stream.InMemoryCategory, Is.Null);
        Assert.That(stream.Category, Is.EqualTo(FileCategory.Other));
        Assert.That(_file.ToArray(), Is.EqualTo(data));
    }

    [Test]
    public void TextThatIsNotTextAfterTheSampleIsKeptInMemoryAsOtherWhenRequested()
    {
        byte[] data = [.. Encoding.UTF8.GetBytes(new string('a', 5000)), 0xFF, 0xFE, 0xFD];
        using InMemoryOrFileStream stream = new("notes.txt", new Dictionary<FileCategory, long> { { FileCategory.Text, 10000 }, { FileCategory.Other, 10000 } }, OpenFile);

        stream.Write(data.AsSpan(0, 5000));
        stream.Write(data.AsSpan(5000));

        stream.Commit();
        Assert.That(stream.InMemoryCategory, Is.EqualTo(FileCategory.Other));
        Assert.That(stream.Category, Is.EqualTo(FileCategory.Other));
        Assert.That(_isFileOpened, Is.False);
        Assert.That(stream.Data.ToArray(), Is.EqualTo(data));
    }
}
