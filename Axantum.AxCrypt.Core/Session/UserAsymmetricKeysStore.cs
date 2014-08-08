using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// Persists a users asymmetric keys in the file system, encrypted with AxCrypt
    /// </summary>
    public class UserAsymmetricKeysStore
    {
        private static Regex _filePattern = new Regex(@"Keys-([\d]+)-txt\.axx", RegexOptions.Compiled);

        private const string _fileFormat = "Keys-{0}.txt";

        private IRuntimeFileInfo _folderPath;

        private KnownKeys _knownKeys;

        private UserAsymmetricKeys _userKeys;

        private IRuntimeFileInfo _file;

        private string _id;

        public UserAsymmetricKeysStore(IRuntimeFileInfo folderPath, KnownKeys knownKeys)
        {
            _folderPath = folderPath;
            _knownKeys = knownKeys;
        }

        public void Load(string userEmail)
        {
            _userKeys = null;
            if (!_knownKeys.IsLoggedOn)
            {
                return;
            }

            foreach (IRuntimeFileInfo file in _folderPath.Files)
            {
                Match match = _filePattern.Match(file.Name);
                if (!match.Success)
                {
                    continue;
                }
                UserAsymmetricKeys keys = TryLoadKeys(file);
                if (keys == null)
                {
                    continue;
                }
                if (String.Compare(userEmail, keys.UserEmail, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }
                _id = match.Groups[1].Value;
                _file = file;
                _userKeys = keys;
                return;
            }
            _userKeys = new UserAsymmetricKeys(userEmail, Instance.UserSettings.AsymmetricKeyBits);
        }

        public UserAsymmetricKeys Keys
        {
            get
            {
                return _userKeys;
            }
        }

        private string UniqueFilePart()
        {
            DateTime now = OS.Current.UtcNow;
            TimeSpan timeSince = now - new DateTime(now.Year, 1, 1);
            return ((int)timeSince.TotalSeconds).ToString();
        }

        public void Save()
        {
            if (_userKeys == null)
            {
                return;
            }

            if (_id == null)
            {
                _id = UniqueFilePart();
            }
            string originalName = _fileFormat.InvariantFormat(_id);
            if (_file == null)
            {
                _file = Factory.New<IRuntimeFileInfo>(Path.Combine(_folderPath.FullName, originalName).CreateEncryptedName());
            }

            string json = JsonConvert.SerializeObject(_userKeys);
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                Factory.New<AxCryptFile>().Encrypt(stream, originalName, _file, _knownKeys.DefaultEncryptionKey, AxCryptOptions.EncryptWithCompression, new ProgressContext());
            }
        }

        private UserAsymmetricKeys TryLoadKeys(IRuntimeFileInfo file)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                if (!Factory.New<AxCryptFile>().Decrypt(file, stream, _knownKeys.DefaultEncryptionKey))
                {
                    return null;
                }

                string json = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
                return JsonConvert.DeserializeObject<UserAsymmetricKeys>(json);
            }
        }
    }
}