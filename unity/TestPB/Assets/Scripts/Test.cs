#define DIRECTLY_CHANGE_URL
#define SIMPLE_SENDING

using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using PB = Google.Protobuf;
using Net = SF.Network;

/// <summary>
/// cannot type HanGul comment,
/// </summary>
public class Test : MonoBehaviour
{
    Sample mSample = new Sample();
    const string DEFAULT_SERVER = "Defalut";

    // Use this for initialization
    void Start()
    {
        //var uri = new System.Uri("http://14.32.173.60:17761");
        var uri = new System.Uri("http://172.30.56.165:17761");
        var connection = Net.ConnectionFactory.Create(Net.ConnectionType.Http, uri).SetTimeout(10);
        Net.ServerConnections.AddAndChange(DEFAULT_SERVER, connection);
    }

    // Update is called once per frame
    void Update()
    {

    }

    string mTitle = "";
    string mMessage = "";

    private string FilePath { get { return Application.dataPath + "\\Resources\\sample.txt"; } }

    public struct TestPacket
    {
        public string __pbDat;
        public string __pbName;
    }

    private void RequestSamplePacket(string name, string base64text)
    {
        try
        {
            var packet = new TestPacket() { __pbDat = base64text, __pbName = name };
            var packetJson = Newtonsoft.Json.JsonConvert.SerializeObject(packet);
        }
        catch(System.Exception e)
        {
            Debug.LogException(e);
        }
    }
    
    void RequestPerson(Lua.User.ReqPerson reqPerson)
    {
        var bufSize = reqPerson.CalculateSize();
        
#if SIMPLE_SENDING
        var request = new Net.RequestBinary(reqPerson.GetType().Name, () => {

            try
            {
                using (var ms = new MemoryStream(bufSize))
                {
                    using (var outputStream = new PB.CodedOutputStream(ms))
                    {
                        reqPerson.WriteTo(outputStream);
                        outputStream.Flush();
                        var buf = ms.GetBuffer();
                        return buf;
                    }
                }
            }
            catch
            {
                return new byte[0];
            }
        });

        request.Send((res) =>
        {
            var response = request.Response;

            if (response != null)
            {
                if (response.IsSuccess)
                {
                    var retData = response.GetBytes();

                    var resPerson = Lua.User.ResPerson.Parser.ParseFrom(retData);
                    var s = string.Format("Name = {0}, Age = {1}, ErrCode= {2}\t\t{3}", resPerson.Name, resPerson.Age, resPerson.ErrCode, System.DateTime.Now);
                    Debug.Log(s);
                    mMessage = s;

                    gameObject.name = "================ " + resPerson.Name + " ================";
                }
                else
                {
                    gameObject.name = "================ RECEIVED : (ErrCode=" + response.GetErrorCode() + ") ================";

                    Debug.LogWarning("response error : " + response.GetErrorCode());
                }
            }
            else
            {
                Debug.LogWarning("response is null");
            }
        });

#elif DIRECTLY_CHANGE_URL
        var uri = new System.Uri("http://14.32.173.60:17761");
            
        using (new Net.Connections.ConnectionPB(uri).Change())
        {
            var reqServer = new Net.RequestBinary("", buf);
            reqServer.Send((resPacket) =>
            {

            });
        }
#else
        using (Net.ServerConnections.Change(DEFAULT_SERVER))
        {
            var reqServer = new Net.RequestBinary("", buf);
            reqServer.Send((resPacket) =>
            {

            });
        }
#endif        
    }

    private void RequestSamplePacket(string name, byte[] buf)
    {

    }

    void SendPerson()
    {
        const int SEND_CNT = 10000;
        
        Debug.Log("REQUEST TIME = " + System.DateTime.Now);

        //StartCoroutine(RequestPerson(reqPerson));
        for (int i = 0; i < SEND_CNT; i++)
        {
            var reqPerson = new Lua.User.ReqPerson();
            reqPerson.ID = (i + 1).ToString();
            reqPerson.Header = new Lua._ReqHeader();
            reqPerson.Header.Category = 0;
            RequestPerson(reqPerson);
        }
    }

    private void OnGUI()
    {
        var isTest = string.IsNullOrEmpty(mTitle);

        GUI.color = Color.green;
        if (GUI.Button(new Rect(10, 10, Screen.width - 20, 40), isTest ? "Test" : "Deserialize-Sample"))
        {
            if (isTest)
            {
                // test assembly
                var myTest = new Google.Protobuf.MyTest();
                mTitle = myTest.GetName();
            }
            else
            {
                // read & parse≤≤
                var textAsset = Resources.Load<TextAsset>("sample.txt");
                if (textAsset != null)
                {
                    string s = textAsset.text;
                    var bytes = System.Convert.FromBase64String(s);

                    _DesrializeSample(bytes);

                    // I will display properties of 'Sample' class instance.
                    if (mSample != null)
                    {
                        mMessage = mSample.ToString();
                    }
                }
            }
        }

        GUI.color = Color.yellow;
        GUI.Label(new Rect(10, 60, Screen.width - 20, 40), mTitle);

        GUI.color = Color.green;
        if (GUI.Button(new Rect(10, 100, Screen.width - 20, 40), "Write"))
        {
            var buf = _SerializeSample();
            if (buf != null)
            {
                // convert
                mMessage = System.Convert.ToBase64String(buf);

                Debug.Log(mMessage);

#if UNITY_EDITOR
                using(var fileStream = File.OpenWrite(FilePath))
                {
                    if (fileStream != null)
                    {
                        using (var writer = new StreamWriter(fileStream))
                        {
                            writer.Write(mMessage);
                        }
                    }
                }
#endif
                // send
                RequestSamplePacket("Sample", buf);
            }
		}

        if (GUI.Button(new Rect(10, 140, Screen.width - 20, 40), "Send"))
        {
            SendPerson();
        }

        GUI.color = Color.yellow;
		GUI.Label(new Rect(10, 180, Screen.width, Screen.height - 10), mMessage);

        GUI.color = Color.cyan;
		GUI.Label(new Rect(10, 280, Screen.width, Screen.height - 10), mJsonText);

        GUI.color = Color.magenta;
        if(GUI.Button(new Rect(10,Screen.height-50,Screen.width-20,40), "CLEAR"))
        {
            mMessage = "";
        }
	}

    private string mJsonText = "";

    string GetjsonText()
    {
        try
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(mSample, Newtonsoft.Json.Formatting.Indented);
        }
        catch
        {
            return "{}";
        }
    }

    Sample _DesrializeSample(byte[] buf)
	{
        mSample = Sample.Parser.ParseFrom(buf);
        return mSample;
	}

	byte[] _SerializeSample()
	{
		mSample.Id = 7761;
		mSample.Name = "Cho Myeong Geun";

        mJsonText = GetjsonText();

		var bufSize = mSample.CalculateSize();
		using (var memStream = new MemoryStream(bufSize))
		{
			using (var outStream = new PB.CodedOutputStream(memStream))
			{
				mSample.WriteTo(outStream);
                outStream.Flush();

				return memStream.ToArray();
			}
		}
	}
}
