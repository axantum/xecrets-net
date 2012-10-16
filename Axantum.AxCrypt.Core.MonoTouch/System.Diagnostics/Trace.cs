using System;

namespace System.Diagnostics
{
	public class Trace
	{
		public Trace ()
		{
		}

		public static event Action<string> InformationEvent;

		public static void WriteLine (string str)
		{
			Console.WriteLine(str);
		}
		
		public static void TraceInformation (string message)
		{
			WriteLine("INFO: " + message);
			if (InformationEvent != null) InformationEvent(message);
		}

		public static void TraceError (string message)
		{
			WriteLine("ERROR: " + message);
		}

		public static void TraceWarning (string message)
		{
			WriteLine("WARNING: " + message);
		}
	}
}

