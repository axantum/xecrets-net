using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Reader
{
    public static class Extensions
    {
        /// <summary>
        /// Naive implementation of IndexOf - optimize only if it proves necessary. Look for Boyer Moore.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="pattern"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int Locate(this byte[] buffer, byte[] pattern, int offset, int count)
        {
            int candidatePosition = offset;
            while (candidatePosition - offset + pattern.Length <= count)
            {
                int i = 0;
                for (; i < pattern.Length; ++i)
                {
                    if (buffer[candidatePosition + i] != pattern[i])
                    {
                        break;
                    }
                }
                if (i == pattern.Length)
                {
                    return candidatePosition;
                }
                ++candidatePosition;
            }
            return -1;
        }
    }
}