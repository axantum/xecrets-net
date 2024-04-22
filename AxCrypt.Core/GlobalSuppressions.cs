// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the
// Code Analysis results, point to "Suppress Message", and click
// "In Suppression File".
// You do not need to add suppressions to this file manually.

using System.Runtime.CompilerServices;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "AxCrypt.Core.Algorithm.Implementation")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "bouncycastle", Scope = "resource", Target = "AxCrypt.Core.Properties.Resources.resources")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HMACSHA", Scope = "member", Target = "AxCrypt.Core.Algorithm.Implementation.BouncyCastleCryptoFactory.#HMACSHA512()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CBC", Scope = "member", Target = "AxCrypt.Core.Algorithm.CipherMode.#CBC")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ECB", Scope = "member", Target = "AxCrypt.Core.Algorithm.CipherMode.#ECB")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OFB", Scope = "member", Target = "AxCrypt.Core.Algorithm.CipherMode.#OFB")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CFB", Scope = "member", Target = "AxCrypt.Core.Algorithm.CipherMode.#CFB")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CTS", Scope = "member", Target = "AxCrypt.Core.Algorithm.CipherMode.#CTS")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HMACSHA", Scope = "member", Target = "AxCrypt.Core.Algorithm.Implementation.BouncyCastleCryptoFactory.#AxCryptHMACSHA1()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HMAC", Scope = "type", Target = "AxCrypt.Core.Algorithm.HMAC")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HMACSHA", Scope = "type", Target = "AxCrypt.Core.Algorithm.HMACSHA1")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HMACSHA", Scope = "type", Target = "AxCrypt.Core.Algorithm.HMACSHA512")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ANSIX", Scope = "member", Target = "AxCrypt.Core.Algorithm.PaddingMode.#ANSIX923")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ISO", Scope = "member", Target = "AxCrypt.Core.Algorithm.PaddingMode.#ISO10126")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ios", Scope = "member", Target = "AxCrypt.Core.Runtime.Platform.#AppleIos")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Osx", Scope = "member", Target = "AxCrypt.Core.Runtime.Platform.#MacOsx")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "PKCS", Scope = "member", Target = "AxCrypt.Core.Algorithm.PaddingMode.#PKCS7")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "AxCrypt.Core.Service")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "AxCrypt.Core")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.Algorithm.CryptoStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.Algorithm.CryptoTransformingStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.ChainedStream`1", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.LockedStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.LookAheadStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.NonClosingStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.PipelineStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.ProgressStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.V1AxCryptDataStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.V1HmacStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.V2AxCryptDataStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.V2AxCryptDataStream`1", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.V2HmacStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.V2HmacStream`1", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "AxCrypt.Core.IO.WrappedBaseStream", Justification = "False positive.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "AxCrypt.Core.Algorithm")]

[assembly: InternalsVisibleTo("Xecrets.Net.Core.Test")]
