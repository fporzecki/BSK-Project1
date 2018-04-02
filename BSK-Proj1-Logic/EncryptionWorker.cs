using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;

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

        // Encrypts given file with 3DES algorithm
        public void EncryptFile(string fileName, string outName, CipherMode cipherMode)
        {
            var outNameNoExtension = Path.GetFileNameWithoutExtension(outName);
            var path = Path.GetDirectoryName(fileName);
            var outputName = outNameNoExtension + ".c3d.tmp.enc";
            var tdesKey = LoadTripleDESKey();
            var tdesIV = LoadTripleDESIV();

            var fin = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var foutTemp = new FileStream(outputName, FileMode.OpenOrCreate, FileAccess.Write);
            var xmlOutputStream = new FileStream(Path.Combine(path, outNameNoExtension + ".xml"), FileMode.OpenOrCreate, FileAccess.Write);
            foutTemp.SetLength(0);

            var bin = new byte[100];         
            var totlen = fin.Length;

            var tdes = new TripleDESCryptoServiceProvider
            {
                Mode = cipherMode
            };

            var encStream = new CryptoStream(foutTemp, tdes.CreateEncryptor(tdesKey, tdesIV), CryptoStreamMode.Write);
            var fileInBase64 = ProcessFile(totlen, encStream, bin, fin);
            encStream.Close();

            var fileExtension = Path.GetExtension(fileName);

            XmlComposer.CreateXml(new List<UserModel> { new UserModel("asd", "123") }, Convert.ToBase64String(tdesIV), Convert.ToString(cipherMode), 
                tdes.BlockSize, tdes.KeySize, "TripleDES", fileInBase64, fileExtension).Save(xmlOutputStream);

            xmlOutputStream.Close();

            fin.Close();
            foutTemp.Close();
            File.Delete(outputName);
        }

        public void DecryptXmlFile(string fileName)
        {
            var tdesKey = LoadTripleDESKey();
            var tdesIV = LoadTripleDESIV();

            var file = XElement.Load(fileName);

            var dataInBase64 = file.Element("EncryptedData").Value;
            var data = Convert.FromBase64String(dataInBase64);
            var reader = new MemoryStream(data);

            var totLen = reader.Length;

            var cipherMode = file.Descendants().First(x => x.Name.LocalName == "CipherMode").Value;
            var extension = file.Descendants().First(x => x.Name.LocalName == "FileExtension").Value;

            var outputFile = new FileStream(Path.GetFileNameWithoutExtension(fileName) + extension, FileMode.OpenOrCreate, FileAccess.Write);
            outputFile.SetLength(0);

            var bin = new byte[100];

            var tdes = new TripleDESCryptoServiceProvider
            {
                Mode = HelperClass.GetCipherMode(cipherMode)
            };

            var encStream = new CryptoStream(outputFile, tdes.CreateDecryptor(tdesKey, tdesIV), CryptoStreamMode.Write);

            ProcessFile(totLen, encStream, bin, reader);

            encStream.Close();
            outputFile.Close();
            reader.Close();
        }

        // Load 3DES key from generated file
        // Generate 3DES key if none is present
        // TODO: choosing files
        private byte[] LoadTripleDESKey()
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
        private byte[] LoadTripleDESIV()
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

        private string ProcessFile(long totlen, CryptoStream encStream, byte[] bin, Stream fin)
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
            var bytes = new byte[encStream.Length];

            encStream.Read(bytes, 0, (int)encStream.Length);

            var binaryString = Convert.ToBase64String(Encoding.UTF8.GetBytes(Encoding.UTF8.GetChars(bytes)));

            return binaryString;
        }
    }
}
