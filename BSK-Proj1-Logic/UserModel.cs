using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSK_Proj1_Logic
{
    public class UserModel
    {
        public string UserEmail { get; set; }
        public string SessionKey { get; set; }

        public UserModel(string userEmail, string sessionKey)
        {
            UserEmail = userEmail;
            SessionKey = sessionKey;
        }
    }
}
