using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using PB = Google.Protobuf;

/// <summary>
/// cannot type HanGul comment,
/// </summary>
public class Test : MonoBehaviour
{

    Sample mSample = new Sample();

    // Use this for initialization
    void Start()
    {

        //Debug.Log(nameof(Test));

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
            var url = "http://14.32.173.60:17761";
            var packet = new TestPacket() { __pbDat = base64text, __pbName = name };
            var packetJson = Newtonsoft.Json.JsonConvert.SerializeObject(packet);
        }
        catch
        {
           
        }
    }

    private void OnGUI()
    {
        var isTest = string.IsNullOrEmpty(mTitle);

        GUI.color = Color.green;
        if (GUI.Button(new Rect(10, 10, Screen.width - 20, 40), isTest ? "Test" : "Read"))
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

                    _Desrialize(bytes);

                    // I will display properties of 'Sample' class instance.
                    if(mSample != null)
                    {
                        mMessage = mSample.ToString();
                    }
                }
            }
        }

        GUI.color = Color.yellow;
        GUI.Label(new Rect(10, 60, Screen.width - 20, 40), mTitle);

        GUI.color = Color.green;
        if (GUI.Button(new Rect(10, 100, Screen.width - 20, 40), "Write & Send"))
        {
            var buf = _Serialize();
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
                RequestSamplePacket("Sample", mMessage);
            }
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

    Sample _Desrialize(byte[] buf)
	{
        mSample = Sample.Parser.ParseFrom(buf);
        return mSample;
	}

	byte[] _Serialize()
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
