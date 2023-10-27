namespace Database.Models
{
    public class Host
    {
        public string Name { get; }
        public int Port { get; }

        public Host(string name, int port = 0)
        {
            Name = name;
            Port = port;
        }
    }
}