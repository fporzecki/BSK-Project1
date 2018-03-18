using System.Collections.Generic;
using System.Xml.Linq;

namespace BSK_Proj1_Logic
{
    public static class XmlComposer
    {
        private const string EncryptedFile = "EncryptedFile";
        private const string EncryptedFileHeader = "EncryptedFileHeader";
        private const string EncryptedData = "EncryptedData";
        private const string Algorithm = "Algorithm";
        private const string KeySize = "KeySize";
        private const string BlockSize = "BlockSize";
        private const string CipherMode = "CipherMode";
        private const string IV = "IV";
        private const string ApprovedUsers = "ApprovedUsers";
        private const string User = "User";
        private const string Email = "Email";
        private const string SessionKey = "SessionKey";
        private const string FileExtension = "FileExtension";

        public static XElement CreateXml(List<UserModel> users, string iv, string cipherMode, 
            int blockSize, int keySize, string algorithm, string encryptedData, string fileExtension)
        {
            var userElements = new List<XElement>();
            foreach (var user in users)
            {
                userElements.Add(new XElement(User, new XElement(Email, user.UserEmail), new XElement(SessionKey, user.SessionKey)));
            }
            var approvedUsers = new XElement(ApprovedUsers, userElements);
            var algorithmElement = new XElement(Algorithm, algorithm);

            //todo: how to convert IV to string?
            var ivElement = new XElement(IV, iv);
            var extensionElement = new XElement(FileExtension, fileExtension);
            var keySizeElement = new XElement(KeySize, keySize);
            var blockSizeElement = new XElement(BlockSize, blockSize);
            var cipherModeElement = new XElement(CipherMode, cipherMode);
            var encryptedDataElement = new XElement(EncryptedData, encryptedData);
            var header = new XElement(EncryptedFileHeader, extensionElement, algorithmElement, keySizeElement, blockSizeElement, cipherModeElement, ivElement, approvedUsers);
            var root = new XElement(EncryptedFile, header, encryptedDataElement);

            return root;
        }
    }
}
