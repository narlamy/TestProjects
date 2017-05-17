using System;

namespace Cocoonbeat.Net
{
    class UnityProxyFactory : IProxyFactory
    {
        public INetResponseProxy CreateResponseProxy(IWebServer webServer)
        {
            return Net.NetResponseProxy.Create(webServer); 
        }

        public IDownloadCallbackProxy CreateCallbackProxy(bool aborted)
        {
            return Net.CallbackProxy.Create(aborted);
        }
        
        public IFileDownloader CreateFileDownloader()
        {
            return new AsyncFileDownloader();
        }

        public bool UseUploadCallbackProxy
        {
            get { return UnityEngine.Application.isPlaying; }
        }
    }
}
