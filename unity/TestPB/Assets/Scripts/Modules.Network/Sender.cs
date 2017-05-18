namespace N2.Network
{
    

    public static class Sender
    {
        private static ISender mInstance = null;
        public static ISender Instance
        {
            get { return mInstance; }
        }

        static Sender()
        {
            var connector = new Connection() { URL = "http://192.168.0.1:17761" };
            mInstance = new ImpSender(connector);
        }
    }
}
