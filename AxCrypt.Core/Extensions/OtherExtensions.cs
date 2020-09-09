using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Extensions
{
    public static class OtherExtensions
    {
        public static string Messages(this Exception exception)
        {
            StringBuilder msg = new StringBuilder();
            while (exception != null)
            {
                if (msg.Length > 0)
                {
                    msg.Append(" -> ");
                }
                msg.Append(exception.Message);
                exception = exception.InnerException;
            }
            return msg.ToString();
        }

        public static Exception Innermost(this Exception exception)
        {
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }
            return exception;
        }

        public static void UpdateListTo(this IList<object> existing, IList<object> updated)
        {
            int i = 0;
            int j = 0;
            while (i < existing.Count && j < updated.Count)
            {
                if (existing[i].Equals(updated[j]))
                {
                    ++i;
                    ++j;
                    continue;
                }

                int nextExistingMatch = FindNextIn(existing, i, updated[j]);
                if (nextExistingMatch == existing.Count)
                {
                    existing.Insert(i, updated[j]);
                }
                else
                {
                    while (nextExistingMatch > i)
                    {
                        existing.RemoveAt(i);
                        --nextExistingMatch;
                    }
                }
                ++j;
                ++i;
            }
            if (i == existing.Count)
            {
                while (j < updated.Count)
                {
                    existing.Add(updated[j]);
                    ++j;
                    ++i;
                }
            }
            if (j == updated.Count)
            {
                while (existing.Count > j)
                {
                    existing.RemoveAt(existing.Count - 1);
                }
            }
        }

        private static int FindNextIn(IList<object> existing, int i, object next)
        {
            while (i < existing.Count)
            {
                if (existing[i].Equals(next))
                {
                    return i;
                }
                ++i;
            }
            return i;
        }
    }
}