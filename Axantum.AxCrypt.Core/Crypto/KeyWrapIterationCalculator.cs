using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class KeyWrapIterationCalculator
    {
        /// <summary>
        /// Get the number of key wrap iterations we use by default. This is a calculated value intended to cause the wrapping
        /// operation to take approximately 1/10th of a second in the system where the code is run.
        /// A minimum of 20000 iterations are always guaranteed.
        /// </summary>
        public virtual long Iterations()
        {
            AesKey dummyKey = new AesKey();
            KeyWrapSalt dummySalt = new KeyWrapSalt(dummyKey.Length);
            DateTime startTime = OS.Current.UtcNow;
            DateTime endTime;
            long totalIterations = 0;
            int iterationsIncrement = 1000;
            using (KeyWrap keyWrap = new KeyWrap(dummyKey, dummySalt, iterationsIncrement, KeyWrapMode.AxCrypt))
            {
                do
                {
                    keyWrap.Wrap(dummyKey);
                    totalIterations += iterationsIncrement;
                    endTime = OS.Current.UtcNow;
                } while ((endTime - startTime).TotalMilliseconds < 500);
            }
            long iterationsPerSecond = totalIterations * 1000 / (long)(endTime - startTime).TotalMilliseconds;
            long defaultIterations = iterationsPerSecond / 10;

            if (defaultIterations < 20000)
            {
                defaultIterations = 20000;
            }

            return defaultIterations;
        }
    }
}