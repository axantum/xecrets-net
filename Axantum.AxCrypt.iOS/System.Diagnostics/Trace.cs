using System;

namespace System.Diagnostics
{
	public class Trace
	{
		public static void TraceInformation (string message)
		{
			WriteLine("INFO: " + message);
		}

		public static void TraceWarning (string message)
		{
			WriteLine ("WARN: " + message);
		}

		public static void TraceError (string message)
		{
			Console.Error.WriteLine(message);
		}

		public static void WriteLine (string log)
		{
			Console.Out.WriteLine (log);
		}

		public Trace ()
		{
		}
	}
}

