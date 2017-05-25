using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using LitJson;

namespace SF.Network
{
    /// <summary>
    /// 패킷 이름을 가지고 패킷 인스턴스를 생성합니다.
    /// </summary>
    internal class PacketFactory
    {
        // 다른 프로세스에서는 새로운 싱글톤 인스턴스가 할당 될 겨~

        private static PacketFactory mInstance = new PacketFactory();
        public static PacketFactory Instance { get { return mInstance; } }

        private Dictionary<string, System.Type> mResponseTypes = new Dictionary<string, System.Type>();

        private PacketFactory()
        {
            TypeFilter myTypeFilter = (type, filterCriteria) =>
            {
                if (!GameModule.InGameModules(type))
                    return false;

                if (System.Attribute.GetCustomAttribute(type, typeof(System.ObsoleteAttribute)) != null)
                    return false;

                return !type.IsAbstract && type.IsSubclassOf(typeof(ResponsePacket));
            };

            foreach (var type in GameModule.FindTypes(MOUDLE_GROUP.PROTOCOL, myTypeFilter, null))
            {
                if (!mResponseTypes.ContainsKey(type.Name))
                    mResponseTypes.Add(type.Name, type);
                //Dev.Logger.Log(string.Format("[PacketFactory] register response packet (\"{0}\")", type.Name));
            }
        }

        [System.Serializable]
        internal class ResponseData
        {
            public string Name = null;

            /// <summary>
            /// ResponseData 객체 생성, 암호화 되어 있다면 암호를 풀 수 있는 키가 필요하므로 입력받습니다.
            /// 암호화가 되어 있지 않은 경우 키는 사용하지 않습니다.
            /// </summary>
            public static ResponseData Create(string responseText, string code)
            {
                //var data = JsonConvert.DeserializeObject<ResponseData>(responseText);
                var data = JsonMapper.ToObject<ResponseData>(responseText);
                data.mCode = code;
                return data;
            }

            [LitJson.JsonIgnore]
            public string Content
            {
                get
                {
#if USE_ENCRYPT
                    if (!string.IsNullOrEmpty(mContent))
                    {
                        return mContent;
                    }
                    else
                    {
                        var dat = (mDat != null) ? mDat : mDefaultDat;
                        return PakêtêŞîfrekirinn.Ｄｅｃｒipｔ(dat, (mDefaultDat != null) ? "" : mCode);
                    }
#else
                    return mContent != null ? mContent : "";
#endif
                }
            }

            string mCode = "";  // 복호화 키 (WebServer.HashKey)

            [LitJson.JsonAlias("Content")]
            string mContent = null;

            /// <summary>
            /// 패킷 전송할 때 입력한 키 혹은 세션에 연결 된 암호화키 사용
            /// </summary>
            [LitJson.JsonAlias("FCFE")]
            string mDat = null;

            /// <summary>
            /// 별도로 부여 받은 암호화키를 사용하지 않고 모듈의 기본 값 사용
            /// </summary>
            [LitJson.JsonAlias("FD0E")]
            string mDefaultDat = null;

            internal bool IsDecryped
            {
                get { return mDat != null || mDefaultDat != null; }
            }
        }

        /// <summary>
        /// 입력받은 문자열을 해석하여 적절한 패킷 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="responseText">패킷을 통해 넘어받은 내용</param>
        /// <returns></returns>
        internal ResponsePacket CreateResponse(RequestPacket request, string responseText)
        {
            bool isBinData = false;   // decription 된 코드 여부

            try
            {
                var hashKey = request.HashKey;
                var responseData = ResponseData.Create(responseText, hashKey);

                isBinData = responseData.IsDecryped;

                if (responseData.Name!=null && mResponseTypes.ContainsKey(responseData.Name))
                {
                    System.Type responseType = mResponseTypes[responseData.Name];

                    //ResponsePacket response = JsonConvert.DeserializeObject(responseData.Content, responseType) as ResponsePacket;
                    ResponsePacket response = JsonMapper.ToObject(responseType, responseData.Content) as ResponsePacket;

                    if (response != null)
                    {
                        // RequestPacket 연결
                        response.SetRequestPacket(request);

                        // ResponsePacket이 RequstPacket에 연결된 패킷이 아닌 경우에 경고 메세지
                        if (request.ResponseType != response.GetType())
                        {
                            Dev.Logger.LogWarning(
                                string.Format("[PacketFactory] 요청한 패킷에 대한 응답 패킷이 올바르지 않습니다. (ReqType=\"{0}\", ResType=\"{1}\", Content=\"{2}\")",
                                request.GetType().Name, response != null ? response.GetType().Name : "<<null>>", responseText));
                        }
                    }
                    else
                    {
                        // Response 디시리얼라이징에 실패했기 때문에 에러 리스폰스 생성

                        Dev.Logger.LogWarning(
                            string.Format("[PacketFactory] ResponsePacket을 디시얼라이징 못 했습니다. (ReqType=\"{0}\", ResType=\"{1}\", Content=\"{2}\")",
                            request.GetType().Name, response != null ? response.GetType().Name : "<<null>>", responseText));

                        response = request.CreateResponse(ServerError.ParsingError);
                    }

                    return response;
                }
                else
                {
                    // 에러 처리

                    var msg = string.Format("[PacketFactory] \"{0}\"은 등록되지 않은 Response 패킷입니다.", responseData.Name);
                    Dev.Logger.LogWarning(msg);

                    return request.CreateResponse(ServerError.NotDefined, responseData.Content);
                }
            }
            catch (System.Exception e)
            {
                string msg = string.Format(
                    "[{2}] Response 패킷 인스턴스 생성 실패" +
                    "\nException : \"{0}\"" +
                    "\n\nContents : \"{1}\"",
                    e, responseText, request.PacketName);

                var errCode = isBinData ? ServerError.DecryptionFailed : ServerError.ParsingError;
                var resPacket = request.CreateResponse(errCode);

                if (resPacket != null)
                {
                    Dev.Logger.LogWarning(msg);
                    return resPacket;
                }
                else
                {
                    throw new System.Exception(msg);                
                }
            }
        }
    }
}
