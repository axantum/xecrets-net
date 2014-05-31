This is Json.NET by James Newton-King version 6.0.3

Json60r3.zip
MD5 adc001d9c0681d864396cfda910d577f
SHA1 b676186555ad2987f4f32cd52b69d25089291ee4

The source code is minimally modified in order to compile. The project is modified to
target the .NET 4.0 profile instead of the client profile to be consistent with the
other projects in the Windows build.

For consistency, the assembly is signed with the Axantum strong name key if available.

The unit tests are not included, since AxCrypt has it's own unit tests, and to keep the
payload to a reasonable level. It's essentially a binary dependency, the only reason to
include the source code is to ensure that it can be rebuilt from source code only, which
is important for a cryptographic application.

Axantum Software AB
2014-05-31