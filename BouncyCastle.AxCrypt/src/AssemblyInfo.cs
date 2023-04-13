// Start with no permissions
//[assembly: PermissionSet(SecurityAction.RequestOptional, Unrestricted=false)]
//...and explicitly add those we need

// see Org.BouncyCastle.Crypto.Encodings.Pkcs1Encoding.StrictLengthEnabledProperty
//[assembly: EnvironmentPermission(SecurityAction.RequestOptional, Read="Org.BouncyCastle.Pkcs1.Strict")]
using System.Reflection;

internal class AssemblyInfo
{
    private static string version;

    public static string Version
    {
        get
        {
            if (version == null)
            {
                var ver = (AssemblyVersionAttribute)typeof(AssemblyInfo).GetTypeInfo().GetCustomAttributes(typeof(AssemblyVersionAttribute), false).FirstOrDefault();
                if (ver != null)
                {
                    version = ver.Version;
                }

                // if we're still here, then don't try again
                if (version == null)
                    version = string.Empty;
            }

            return version;
        }
    }
}