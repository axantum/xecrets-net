using System;

namespace System.Diagnostics
{
	public class TraceSwitch
	{
		public TraceLevel Level {
			get;
			set;
		}

		private string axCryptSwitch;
		private string loggingLevelsForAxCrypt;

		public TraceSwitch (string axCryptSwitch, string loggingLevelsForAxCrypt)
		{
			this.axCryptSwitch = axCryptSwitch;
			this.loggingLevelsForAxCrypt = loggingLevelsForAxCrypt;
		}

		public TraceSwitch ()
		{
		}
	}
}

