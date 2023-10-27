namespace Api.Model
{
    public class ServerDetails
    {
        public ServerDetails(Host host, Credentials login)
        {
            Host = host;
            Login = login;
        }

        public string UID { get; set; } = string.Empty;

        public string ServerName { get; set; } = string.Empty;

        public Host Host { get; set; }
        public Credentials Login { get; }

        public string Description { get; set; } = string.Empty;

        public int ColorDepth { get; set; } = 0;

        public int DesktopWidth { get; set; } = 0;

        public int DesktopHeight { get; set; } = 0;

        public bool Fullscreen { get; set; } = false;

        public int GroupID { get; set; } = 0;
    }
}