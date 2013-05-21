using System;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Crypto;

namespace Axantum.AxCrypt.Core.MacOsx
{
	public class RuntimeEnvironment : Mono.RuntimeEnvironment
	{
		public RuntimeEnvironment () : base(TimeSpan.FromSeconds(1))
		{
		}

		public override ILauncher Launch(string filePath) {
			return new Launcher(filePath);
		}
	}
}

