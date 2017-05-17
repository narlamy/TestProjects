namespace N2.Network
{
    public interface IConnector
    {
        string URL { get; set; }
    }

    public class HttpConnector : IConnector
    {
        public string URL { get; set; }
    }

    public interface ISender
    {
        void Send(RequestPacket req);
    }

    public delegate void OnListen(RequestPacket req);

    public interface IListener
    {
        void Add(OnListen onListen);
        void Remove(OnListen onListen);
    }

    public static class Sender
    {
        private static ISender mInstance = null;
        public static ISender Instance
        {
            get { return mInstance; }
        }

        static Sender()
        {
            var connector = new HttpConnector() { URL = "http://192.168.0.1:17761" };
            mInstance = new HttpSender(connector);
        }
    }
}
