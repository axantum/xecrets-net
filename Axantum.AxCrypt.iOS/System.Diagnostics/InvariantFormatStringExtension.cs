using System;

namespace System.Diagnostics
{
	public static class InvariantFormatStringExtension
	{
		public static string InvariantFormat(this string format, string message, string appName) {
			return String.Format (format, message, appName);
		}
	}
}

