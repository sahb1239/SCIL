using System;
using System.IO;
using System.Threading.Tasks;

namespace SCIL
{
    /// <summary>
    /// https://stackoverflow.com/questions/11996299/c-net-identify-zip-file
    /// </summary>
    public static class ZipHelper
    {
        public const string SignatureZip = "50-4B-03-04";

        public static async Task<bool> CheckSignature(string filepath)
        {
            int signatureSize = 4;
            string expectedSignature = SignatureZip;

            if (String.IsNullOrEmpty(filepath)) throw new ArgumentException("Must specify a filepath");
            if (String.IsNullOrEmpty(expectedSignature)) throw new ArgumentException("Must specify a value for the expected file signature");
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length < signatureSize)
                    return false;
                byte[] signature = new byte[signatureSize];
                int bytesRequired = signatureSize;
                int index = 0;
                while (bytesRequired > 0)
                {
                    int bytesRead = await fs.ReadAsync(signature, index, bytesRequired).ConfigureAwait(false);
                    bytesRequired -= bytesRead;
                    index += bytesRead;
                }
                string actualSignature = BitConverter.ToString(signature);
                if (actualSignature == expectedSignature) return true;
                return false;
            }
        }
    }
}