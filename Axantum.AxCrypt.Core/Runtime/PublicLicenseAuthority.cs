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

        private Lazy<IAsymmetricPublicKey> _publicKey = new Lazy<IAsymmetricPublicKey>(() => New<IAsymmetricFactory>().CreatePublicKey(_publicKeyPem));

        public Task<IAsymmetricPrivateKey> PrivateKeyAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IAsymmetricPublicKey> PublicKeyAsync()
        {
            return Task.FromResult(_publicKey.Value);
        }
    }
}