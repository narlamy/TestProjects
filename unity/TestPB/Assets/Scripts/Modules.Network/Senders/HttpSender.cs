using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace N2.Network
{
    public class HttpSender : ISender
	{
		Thread mResponseThread = null;
		IConnector mConnector = null;

		public HttpSender(IConnector connector)
		{
			StartHttps();
		}

		void StartHttps()
		{
			// Https를 사용하기 위해 서버 인증서 유효성 검사 콜백 등록
			ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallback;
		}

		public bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			bool isSuccess = true;

			// If there are errors in the certificate chain, look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None)
			{
				for (int i = 0; i < chain.ChainStatus.Length; i++)
				{
					if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
					{
						chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
						chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
						chain.ChainPolicy.UrlRetrievalTimeout = new System.TimeSpan(0, 1, 0);
						chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
						bool chainIsValid = chain.Build((X509Certificate2)certificate);
						if (!chainIsValid)
						{
							isSuccess = false;
						}
					}
				}
			}
			return isSuccess;
		}

		public OnChangeConnectingState OnChangeConnectingState
		{
			set { mOnChangeLoadingState = value; }
			private get { return mOnChangeLoadingState; }
		}

        public void Send(RequestPacket req)
		{

		}
	}

}
