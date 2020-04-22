using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class PublicLicenseAuthority : ILicenseAuthority
    {
        private static readonly string _publicKeyPem = "-----BEGIN PUBLIC KEY-----\r\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAripHo21lsui04PyeqU7+\r\nvB1f/yW+eUDmrKxtCofSnheZo1/fjSsce5weFhWl1JEnMgvGSLByH5yCHadOPt/+\r\ncZDRScdMVI+9ukvsHXoT0Za99wcOxIuXixOrD5jh8SmtrICxjkEMu6pRecG1Rzh+\r\nK41yxDs47AFMXMMLCH8nSKxtk4qyuRkpkahq/r+sz1EEZabSqUM5q0FmAKx8UhUd\r\nY9PGbrurwFwJf/dZ4AxDUJGZBtNvs+oBMADEFWNaUhHKEgL0EydTmtmPTDaFdMbS\r\n3uCWwJUMWKXQjOGNtikvVX2aZOOs3pKu58fZ6tSHk29HfNrPQc0SxIDr57wH2H4J\r\nivgaee06BkRKCHdfhLAdHOaijWezYXPzM6RGZm/E4yL2LnR8ybwq8MAid2eKcxnn\r\nETT1IJh8hRajvVJeJlwncD2/Pqt4A1GC+JvJW31WG9WiPp/BWdkkDPsS8K9CSdqA\r\nOyUUCDC0ZTaqXRWG4AfuymCGCzbmxtogILb8DEOr2hmftc7L2QPOXn6anHLKqI7c\r\n4vS9MdVbMs2lhs4K/33CHYy7irn3QoO9sLGrNrWuBzegOJGQsmqwzYXXazj0lqPk\r\nS8pY0hyvcCRkzGf/y0H4Fk+mIG31XsoO85f28yMCX4aN8lBdQUFi+Fr5fwL9yr60\r\ns9IoqmpyrY4fHYYjJkL+ALUCAwEAAQ==\r\n-----END PUBLIC KEY-----\r\n";

        private static readonly string _testPublicKeyPem = "-----BEGIN PUBLIC KEY-----\r\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAj7wiNM2JcP/dTNU4CZv7\r\naWR9XjdreIdjBq50hzRl8zIDX1qyank3BS6Z02TdVv8RHATIeb1AKAGgCHMllHc7\r\nqQ7Rg3vcySNRjGDZkHMGwCMqxnHZyDb7ZAQtiOMuwbSyeio3ia62Vj8uR94eMHfW\r\nTxLbfMVnWiMvelinckNUSGnmejY0m9QcbCN2tVca01qX98ZY99FR+n1COLBIIA+k\r\n9s5UEomN6jm/EoLeU7v/3F/6fATST/m5zlYz4cnb3LpWx725P/I8SU7U+EXUytHS\r\nCw9oijgWbMgNz+1cGP4zT7SEveUM0JqYDiGMkUKdJxGcJtDxW9xVBUrghuCbQRNj\r\naSMaeBPVUhPy+Jll0wrTH7+ZBqjpchDw7W+gJyWr8fn94f77TRQ8kbLOMz4bJsrA\r\nWaL7o4qm/ckXDer3s1Baqa8YdEQHrS68dPW0InQfKPSBnqwqJPBEyiDBi3vV0B+6\r\nqHu35+b80Uk34MWqZGB792J85LW8kjhpmdb3NWL0g6WHgW5Da7kXMAezuVGTMkDn\r\nF25TyzbTkhVMBJmjZLU5ZOAViPYJV9rN83sISuDTBSBIxWBikyBv9MzlN8xEPCUG\r\noHlUJGp8HiA5J5yUPHTGc6lQarAeUHdHfc75IZF2IswNbpySsdYfFlht+mvoN8EO\r\nVlBMnNxwEz2yTEoa1xwIv2cCAwEAAQ==\r\n-----END PUBLIC KEY-----\r\n";

        private Lazy<IAsymmetricPublicKey> _publicKey = new Lazy<IAsymmetricPublicKey>(() => New<IAsymmetricFactory>().CreatePublicKey(_publicKeyPem));

        private Lazy<IAsymmetricPublicKey> _testPublicKey = new Lazy<IAsymmetricPublicKey>(() => New<IAsymmetricFactory>().CreatePublicKey(_testPublicKeyPem));

        public Task<IAsymmetricPrivateKey> PrivateKeyAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IAsymmetricPublicKey> PublicKeyAsync()
        {
            if (Resolve.UserSettings.RestApiBaseUrl.ToString() == "https://test.axcrypt.net/api/")
            {
                return Task.FromResult(_testPublicKey.Value);
            }

            return Task.FromResult(_publicKey.Value);
        }
    }
}