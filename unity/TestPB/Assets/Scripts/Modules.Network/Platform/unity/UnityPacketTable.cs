using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using System.IO;

namespace N2.Network
{
    /// <summary>
    /// 패킷 ID 테이블 : 클래스만 생성하면 자동으로 ID를 부여합니다.
    /// </summary>
    class UnityPacketTable : IPacketTable
    {
        public UnityPacketTable()
        {
            Init();
        }

        const int BASE_NUM = 1000;
        Dictionary<string, int> mTable = new Dictionary<string, int>();

        Dictionary<int, string> mFinder = null;
        Dictionary<int, string> Finder
        {
            get
            {
                if (mFinder == null)
                {
                    mFinder = new Dictionary<int, string>();

                    foreach (var pair in mTable)
                    {
                        if(!mFinder.ContainsKey(pair.Value))
                            mFinder.Add(pair.Value, pair.Key);
                    }
                }
                return mFinder;
            }
        }

        void Init()
        {
            LoadTable();

            if (Application.isEditor)
            {
                if (Collect())
                    SaveTable();
            }
        }

        bool Collect()
        {
            TypeFilter myTypeFilter = (type, filterCriteria) =>
            {
                if (!GameModule.InGameModules(type))
                    return false;

                if (Attribute.GetCustomAttribute(type, typeof(System.ObsoleteAttribute)) != null)
                    return false;

                return !type.IsAbstract && type.IsSubclassOf(typeof(ResponsePacket));
            };

            bool isAppend = false;

            foreach (Type type in GameModule.FindTypes(ModuleCategory.Packet, myTypeFilter, null))
            {
                var key = type.FullName;

                if (!mTable.ContainsKey(key))
                {
                    int uniqueNum = (mTable.Count + 1) * BASE_NUM;
                    mTable.Add(key, uniqueNum);
                    isAppend = true;
                }
            }

            return isAppend;
        }

        const string PATH = "System/PacketDefines";
        const string PATH_EXT = ".txt";

        void LoadTable()
        {
            var textAsset = Resources.Load<TextAsset>(PATH);
            if (textAsset != null)
            {
                using (var reader = new StringReader(textAsset.text))
                {
                    if (reader == null)
                    {
                        Dev.Logger.LogWarning("[PacketTable] loading failed => " + PATH);
                        return;
                    }

                    for (; ; )
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;

                        string[] tokens = line.Split('|');
                        if (tokens.Length >= 2)
                        {
                            int uniqueNum;

                            if (int.TryParse(tokens[1], out uniqueNum))
                            {
                                mTable.Add(tokens[0].Trim(), uniqueNum);
                            }
                        }
                    }
                }
            }
            else
            {
                Dev.Logger.LogError("[PacketTable] cannot load table file => " + PATH);
            }
        }

        void SaveTable()
        {
            if (mTable.Count == 0)
                return;

            string targetPath = Application.dataPath + "/Resources/" + PATH + PATH_EXT;
            using (var writer = new StreamWriter(targetPath))
            {
                foreach (var pair in mTable)
                {
                    writer.WriteLine("{0} | {1}", pair.Key, pair.Value);
                }
            }
        }

        public int GetUniqueNum(Type type)
        {
            int uniqueNum;

            if (mTable.TryGetValue(type.FullName, out uniqueNum)) return uniqueNum;
            else return 0;
        }

        public string FindPacketName(int uniqueNum)
        {
            string packetName;
            return Finder.TryGetValue(uniqueNum, out packetName) ? packetName : "";
        }

        public int Count
        {
            get { return mTable.Count; }
        }
    }
}
