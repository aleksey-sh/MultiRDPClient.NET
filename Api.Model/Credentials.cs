namespace Api.Model
{
    public class Credentials
    {
        public Credentials(string domain, string userName, string password)
        {
            Domain = domain;
            UserName = userName;
            Password = password;
        }

        string _password = string.Empty;

        public string Domain { get; }

        public string UserName { get; }

        public string Password
        {
            get
            {
                //if (this._password != string.Empty)
                //{
                //    this._password = RijndaelSettings.Decrypt(this._password);
                //}

                return this._password;
            }
            set
            {
                string val = value;

                //if (val != string.Empty)
                //{
                //    val = RijndaelSettings.Encrypt(val);
                //}

                this._password = val;
            }
        }

    }
}