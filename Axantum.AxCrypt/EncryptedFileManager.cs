using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    public class EncryptedFileManager
    {
        public static void Open(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                Open(file);
            }
        }

        public static void Open(string file)
        {
        }
    }
}