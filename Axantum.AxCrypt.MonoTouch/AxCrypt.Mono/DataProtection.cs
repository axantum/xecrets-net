using System;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.Mono
{
	internal class DataProtection : IDataProtection
	{
		public byte[] Protect(byte[] unprotectedData)
		{
			return unprotectedData;
		}
		
		public byte[] Unprotect(byte[] protectedData)
		{
			return protectedData;
		}
	}
}

