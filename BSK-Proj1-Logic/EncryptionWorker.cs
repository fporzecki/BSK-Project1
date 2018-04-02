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
            var tdesKey = LoadTripleDESKey();
            var tdesIV = LoadTripleDESIV();

                using (var xmlOutputStream = new FileStream(Path.Combine(path, outNameNoExtension + ".xml"), FileMode.OpenOrCreate, FileAccess.Write))
                {
                    var tdes = new TripleDESCryptoServiceProvider
                    {
                        Mode = cipherMode
                    };

                    var encryptedText = EncryptFileToXml(tdes, fileName);

                    var fileExtension = Path.GetExtension(fileName);

                    XmlComposer.CreateXml(new List<UserModel> { new UserModel("asd", "123") }, Convert.ToBase64String(tdesIV), Convert.ToString(cipherMode),
                        tdes.BlockSize, tdes.KeySize, "TripleDES", encryptedText, fileExtension).Save(xmlOutputStream);
                }

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


                var tdes = new TripleDESCryptoServiceProvider
                {
                    Mode = HelperClass.GetCipherMode(cipherMode)
                };


                DecryptFileFromXml(dataInBase64, tdes, fileName, extension);

            reader.Close();
        }

        private void DecryptFileFromXml(string encryptedText, TripleDESCryptoServiceProvider tdes, string fileName, string extension)
        {
            var key = LoadTripleDESKey();
            var iv = LoadTripleDESIV();

            var encryptedBytes = Convert.FromBase64String(encryptedText);

            using (var memoryStream = new MemoryStream(encryptedBytes))
            using (var outputFileStream = new FileStream(Path.GetFileNameWithoutExtension(fileName) + extension, FileMode.OpenOrCreate))
            using (var outputStream = new CryptoStream(memoryStream, new FromBase64Transform(), CryptoStreamMode.Read))
            using (var cryptoStream = new CryptoStream(memoryStream, tdes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
            {
                var plainTextBytes = new byte[encryptedBytes.Length];

                var fraction = plainTextBytes.Length / 100;

                var length = fraction;

                outputStream.Read(encryptedBytes, 0, encryptedBytes.Length);

                // probably should be changed to be more expandable -> more path options and shit
                memoryStream.Position = 0;

                for (var i = 1; length <= plainTextBytes.Length; i++)
                {
                    cryptoStream.Read(plainTextBytes, 0, fraction);
                    length += fraction;
                    _backgroundWorker.ReportProgress(i);
                }

                memoryStream.Position = 0;

                memoryStream.CopyTo(outputFileStream);
            }
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

        private string EncryptFileToXml(TripleDESCryptoServiceProvider tdes, string filePath)
        {
            var tempFile = File.Create("temp.txt");
            var outputStream = new CryptoStream(tempFile, new ToBase64Transform(), CryptoStreamMode.Write);

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                // todo: make it a binary batch array processing method -> for big files and progress bar updates
                var key = LoadTripleDESKey();
                var iv = LoadTripleDESIV();


                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, tdes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                {
                    var fraction = fileStream.Length / 100;

                    var fin = new byte[fraction];

                    var length = fraction;

                    for (var i = 1; length <= fileStream.Length; i++)
                    {
                        fileStream.Read(fin, 0, (int)fraction);

                        cryptoStream.Write(fin, 0, fin.Length);
                        length += fraction;
                        if (i % 2 == 0)
                        {
                            _backgroundWorker.ReportProgress(i / 2);
                        }
                    }
                    cryptoStream.FlushFinalBlock();
                    memoryStream.Position = 0;
                    memoryStream.CopyTo(outputStream);
                }
            }

            tempFile.Close();
            var encryptedText = File.ReadAllText("temp.txt");
            File.Delete("temp.txt");
            outputStream.Close();
            return encryptedText;
        }

        //private string ProcessFile(long totlen, Stream fin, TripleDESCryptoServiceProvider tdes)
        //{
        //    long rdlen = 0;
        //    var prog = 0d;

        //    //var stringBuilder = new StringBuilder();

            


        //    while (rdlen < totlen)
        //    {
        //        var bin = new byte[100];

        //        var len = fin.Read(bin, 0, 100);
                
        //        encStream.Write(bin, 0, len);
                
        //        rdlen = rdlen + len;
        //        var progressValue = Math.Round(((double)rdlen / (double)totlen) * 100.0);

        //        if (prog != progressValue)
        //        {
        //            _backgroundWorker.ReportProgress((int)progressValue);
        //            prog = progressValue;
        //        }

        //        //var binaryString = Convert.ToBase64String(Encoding.UTF8.GetBytes(Encoding.UTF8.GetChars(bin)));
        //        //stringBuilder.Append(binaryString);
        //    }

        //    var bytes = new byte[encStream.Length];
        //    encStream.Read(bytes, 0, (int)encStream.Length);
        //    var binaryString = Convert.ToBase64String(Encoding.UTF8.GetBytes(Encoding.UTF8.GetChars(bytes)));

        //    return binaryString;
        //}
    }
}
