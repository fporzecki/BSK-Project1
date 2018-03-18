using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BSK_Proj1_Logic
{
    //todo: pls rename me
    public static class HelperClass
    {
        public static CipherMode GetCipherMode(string encryptionMode)
        {
            var cipherMode = CipherMode.ECB;
            if (encryptionMode == "CBC")
                cipherMode = CipherMode.CBC;
            if (encryptionMode == "OFB")
                cipherMode = CipherMode.OFB;
            if (encryptionMode == "CFB")
                cipherMode = CipherMode.CFB;

            return cipherMode;
        }
    }
}
