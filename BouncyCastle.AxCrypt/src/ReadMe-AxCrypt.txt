This is The Legion of the Bouncy Castle Release 1.7 Thursday 7th April 2011

ccrypto-net-1.7-src.zip Source code, examples, tests, documentation.

Checksums:	md5	a4b116ac9fc50e9d495968514e15f5eb
sha1	41ec96c9e96d7c980bd7198347365323a639de6e

The source code is minimally modified in order to compile as a Portable Class Library.

The changes are essentially limited to using ToUpper() ToLower() overloads without culture
info, changing Stream.Close() to Stream.Dispose() and a minor trick with a ManualResetEvent()
to emulate Thread.Sleep(1).

The project file is generated from scratch, a few parts are excluded in order to compile
with minimal effort.

For consistency, the assembly is signed with the Axantum strong name key if available.

The unit tests are not included, since AxCrypt has it's own unit tests to validate the crypto.

Axantum Software AB
2014-05-31