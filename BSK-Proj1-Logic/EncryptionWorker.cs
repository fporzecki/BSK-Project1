using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace BSK_Proj1_Logic
{
    public class EncryptionWorker
    {
        public BackgroundWorker _backgroundWorker;
        private readonly string _keyFileName = "3dskey.key";
        private readonly string _ivFileName = "3dsiv.key";

        public EncryptionWorker(BackgroundWorker backgroundWorker)
        {
            _backgroundWorker = backgroundWorker;
        }

        // Load 3DES key from generated file
        // Generate 3DES key if none is present
        // TODO: choosing files
        public byte[] LoadTripleDESKey()
        {
            byte[] key;
            try
            {
                key = File.ReadAllBytes(_keyFileName);
            }catch(IOException ex)
            {
                var algorithm = TripleDESCryptoServiceProvider.Create();
                algorithm.GenerateKey();
                key = algorithm.Key;
                File.WriteAllBytes(_keyFileName, key);
            }
            return key;
        }

        // Load IV key from generated file
        // Generate IV key if none is present
        // TODO: choosing files
        public byte[] LoadTripleDESIV()
        {
            byte[] key;
            try
            {
                key = File.ReadAllBytes(_ivFileName);
            }
            catch (IOException ex)
            {
                var algorithm = TripleDESCryptoServiceProvider.Create();
                algorithm.GenerateIV();
                key = algorithm.IV;
                File.WriteAllBytes(_ivFileName, key);
            }
            return key;
        }

        // Encrypts given file with 3DES algorithm
        // Then overwrites the original file with encrypted
        public void EncryptFile(string fileName, string outName, CipherMode cipherMode)
        {         
            string outputName = Path.GetFileNameWithoutExtension(outName) + ".c3d.tmp.enc";
            var tdesKey = LoadTripleDESKey();
            var tdesIV = LoadTripleDESIV();

            FileStream fin = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
            FileStream foutTemp = new FileStream(outputName, FileMode.OpenOrCreate, FileAccess.Write);
            foutTemp.SetLength(0);

            byte[] bin = new byte[100];         
            long totlen = fin.Length;                      

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Mode = cipherMode;

            CryptoStream encStream = new CryptoStream(foutTemp, tdes.CreateEncryptor(tdesKey, tdesIV), CryptoStreamMode.Write);

            ProcessFile(totlen, encStream, bin, fin);

            encStream.Close();
            fin.Close();
            fout.Close();
            foutTemp.Close();
            File.Copy(outputName, outName, true);
            File.Delete(outputName);
        }

        // Decrypts given file with 3DES algorithm
        // Then overwrites the original file with decrypted
        public void DecryptFile(string fileName, CipherMode cipherMode)
        {
            string outName = Path.GetFileNameWithoutExtension(fileName) + ".c3d.tmp.dec";
            var tdesKey = LoadTripleDESKey();
            var tdesIV = LoadTripleDESIV();

            FileStream fin = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);

            byte[] bin = new byte[100];
            long rdlen = 0;
            long totlen = fin.Length;
            int len;

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Mode = cipherMode;

            CryptoStream encStream = new CryptoStream(fout, tdes.CreateDecryptor(tdesKey, tdesIV), CryptoStreamMode.Write);

            ProcessFile(totlen, encStream, bin, fin);

            encStream.Close();
            fin.Close();
            fout.Close();
            File.Copy(outName, fileName, true);
            File.Delete(outName);
        }

        private void ProcessFile(long totlen, CryptoStream encStream, byte[] bin, FileStream fin)
        {
            long rdlen = 0;
            var prog = 0d;
            while (rdlen < totlen)
            {
                var len = fin.Read(bin, 0, 100);
                encStream.Write(bin, 0, len);
                rdlen = rdlen + len;
                var progressValue = Math.Round(((double)rdlen / (double)totlen) * 100.0);

                if (prog != progressValue)
                {
                    _backgroundWorker.ReportProgress((int)progressValue);
                    prog = progressValue;
                }
            }
        }
    }
}
