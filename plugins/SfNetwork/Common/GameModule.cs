using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;

namespace SF
{
    public enum MOUDLE_GROUP
    {
        COMMON = 1,
        PROTOCOL = 2,               // Response 패킷
        MODULE = 3,                 // Network/Local 모드에 따라 다르게 구현 된 모듈 인스턴스 (주로 서버와 통신하는 모듈, ModuleBinder)
        MODULE_FACTORY = 4,         // 모듈 팩토리
        GAME_DATA,                  // 게임 데이터
        GM_COMMAND = 10,            // 운영자 명령
        CONSOLE_COMMAND = 11        // console 명령
    }

    /// <summary>
    /// 모듈에 접근해서 필요한 Type을 불러올 수 있습니다.
    /// </summary>
    public static class GameModule
    {
        private static IEnumerable<Module> GetLoadedModules()
        {
            var domain = System.AppDomain.CurrentDomain;

            if (domain != null)
            {
                var assemblies = domain.GetAssemblies();
                if (assemblies == null)
                {
                    Dev.Logger.LogError("[Reflection] assembly array is null");
                    yield break;
                }

                for (int k = 0; k < assemblies.Length; k++)
                {
                    var asm = assemblies[k];
                    if (asm != null)
                    {
                        var modules = asm.GetLoadedModules(false);
                        if (modules != null)
                        {
                            for (int i = 0; i < modules.Length; i++)
                            {
                                var mod = modules[i];
                                if (mod != null)
                                {
                                    yield return mod;
                                }
                                else
                                {
                                    Dev.Logger.LogError("[Reflection] Module is null");
                                }
                            }
                        }
                        else
                        {
                            Dev.Logger.LogError("[Reflection] Module array is null");
                        }
                    }
                    else
                    {
                        Dev.Logger.LogError("[Reflection] Assembly is null");
                    }
                }
            }
            else
            {
                Dev.Logger.LogError("[Reflection] Current Domain is null");
            }
        }

        internal static IEnumerable<Type> FindTypes(TypeFilter typeFilter, object filterCritica)
        {
            foreach (var mod in GameModule.GetLoadedModules())
            {
                foreach (var type in mod.FindTypes(typeFilter, filterCritica))
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// 모듈에 접근해서 타입 필터로 걸러진 모든 type을 가져옵니다.
        /// </summary>
        public static IEnumerable<Type> FindTypes(MOUDLE_GROUP category, TypeFilter typeFilter, object filterCritica)
        {
            if (UseCachedTypes)
            {
                var list = CachedData.GetList(category);
                if (list != null)
                {
                    for (int i = 0; i < list.Types.Count; i++)
                    {
                        var type = list.Types[i];
                        if (type != null)
                            yield return type;
                    }
                }
            }
            else
            {
                foreach (var mod in GameModule.GetLoadedModules())
                {
                    foreach (var type in mod.FindTypes(typeFilter, filterCritica))
                    {
                        if (type != null)
                        {
                            CacheType(category, type);
                            yield return type;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 수집 된 Type의 캐싱 비활성화 여부 (기본값은 false)
        /// </summary>
        static MOUDLE_GROUP[] DisabledCategories = null;

        static bool IsDisabled(MOUDLE_GROUP cat)
        {
            return DisabledCategories != null && DisabledCategories.Contains(cat);
        }

        /// <summary>
        /// using() 블록 안에서 Type 수집 캐싱을 무시하고 싶을 때 사용합니다.
        /// </summary>
        public class IgnoreCaching : IDisposable
        {
            public IgnoreCaching(params MOUDLE_GROUP[] categories)
            {
                DisabledCategories = categories;
            }

            public void Dispose()
            {
                DisabledCategories = null;
            }
        }

        /// <summary>
        /// 여러개의 타입중에서 첫 번째 타입을 반환합니다.
        /// </summary>
        public static Type FirstType(MOUDLE_GROUP category, TypeFilter typeFilter, object filterCritica)
        {
            foreach (var type in FindTypes(category, typeFilter, filterCritica))
                return type;
            return null;
        }

        static List<string> m_ListInvalidModule = new List<string>()
        {
            "ServerLayer_Local.dll"
        };

        /// <summary>
        /// 해당 타입이 캐싱대상에 맞는지 확인합니다.
        /// </summary>
        private static bool IsValidType(Type type)
        {
            if (type == null)
                return false;

            return !m_ListInvalidModule.Contains(type.Module.Name);
        }

        /// <summary>
        /// 해당 Category에 type 을 캐싱합니다.
        /// </summary>
        private static void CacheType(MOUDLE_GROUP category, Type type)
        {
            if (IsTypeCaching && !IsDisabled(category) && IsValidType(type))
            {
                var list = CachedData.GetList(category);
                if (list != null)
                    list.InsertType(type);
            }
        }

        /// <summary>
        /// 타입을 캐싱중인가요?
        /// </summary>
        private static bool IsTypeCaching
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.OSXEditor;
            }
        }

        /// <summary>
        /// 캐싱되어 있는 타입을 사용 유무를 반환합니다.
        /// 본 모드가 설정되어 있으면 에셈블리에서 타입 인스턴스를 수집하지 않습니다.
        /// </summary>
        private static bool UseCachedTypes
        {
            get
            {
                return Application.platform == RuntimePlatform.IPhonePlayer ||
                    Application.platform == RuntimePlatform.Android;
            }
        }

        [System.Serializable]
        class CachedTypeList
        {
            public CachedTypeList(MOUDLE_GROUP category)
            {
                Category = category;
            }

            // [2017-05-23] narlamy
            // LitJson은 기능이 빈약함. enum 처리하려면 어딘가에서 등록해야 함

            //[JsonConverter(typeof(StringEnumConverter))]
            public MOUDLE_GROUP Category { get; set; }

            
            [JsonAlias("Types")]
            List<string> mStrTypes = new List<string>();

            List<Type> mTypes = null;
            [JsonIgnore]
            public List<Type> Types
            {
                get
                {
                    if (mTypes == null)
                    {
                        mTypes = new List<Type>();

                        if (mStrTypes.Count == 0)
                        {
                            //Dev.Logger.Log(string.Format("[GameModule] Category {0} Create Type SKIP",Category));
                            return mTypes;
                        }

                        //List<string> listSuccess = new List<string>();
                        List<string> listAsseblyFail = new List<string>();
                        List<string> listTypeFail = new List<string>(); 
                        string[] arrSplit = new string[] { "," };
                        foreach (string typename in mStrTypes)
                        {
                            string[] arrStr = typename.Split(arrSplit, StringSplitOptions.RemoveEmptyEntries);
                            if (arrStr.Length >= 2)
                            {
                                Assembly assembly = Assembly.Load(arrStr[1].Trim());
                                if (assembly != null)
                                {
                                    Type makeType = assembly.GetType(arrStr[0].Trim());
                                    if (makeType != null)
                                    {
                                        mTypes.Add(makeType);
                                    }
                                    else
                                    {
                                        listTypeFail.Add(typename);
                                    }
                                }
                                else
                                {
                                    listAsseblyFail.Add(typename);
                                }
                            }

                        }
                        
                        if (listAsseblyFail.Count > 0)
                        {
                            var builder = new StringBuilder();
                            foreach (string name in listAsseblyFail)
                                builder.Append(string.Format("\n{0}", name));
                            Dev.Logger.Log(string.Format("[GameModule] Category {0} Load Assembly Fail Count:{1}\n{2}", Category.ToString(), listTypeFail.Count, builder.ToString()));
                        }

                        if (listTypeFail.Count > 0)
                        {
                            var builder = new StringBuilder();
                            foreach (string name in listTypeFail)
                                builder.Append(string.Format("\n{0}", name));
                            Dev.Logger.Log(string.Format("[GameModule] Category {0} Create Types Fail Count:{1}\n{2}", Category.ToString(), listTypeFail.Count, builder.ToString()));
                        }
                    }
                    return mTypes;
                }
            }

            public void InsertType(Type type)
            {
                if (!Types.Contains(type))
                {
                    Types.Add(type);
                    mStrTypes.Add(Common.JsonTypeUtil.TypeToString(type));
                }
            }
        }

        [System.Serializable]
        class CachedTypeData
        {
            [JsonIgnore]
            public bool IsEmpty
            {
                get
                {
                    var cnt = 0;
                    for (int i = 0; i < Data.Length; i++)
                        cnt += Data[i].Types.Count;
                    return cnt == 0;
                }
            }

            public CachedTypeData()
            {
                var cats = System.Enum.GetValues(typeof(MOUDLE_GROUP));
                int index = 0;

                Data = new CachedTypeList[cats.Length];
                foreach (var cat in cats)
                {
                    var list = new CachedTypeList((MOUDLE_GROUP)cat);
                    Data[index++] = list;
                }
            }

            public CachedTypeList[] Data { get; set; }

            public CachedTypeList GetList(MOUDLE_GROUP category)
            {
                if (Data == null)
                    return null;

                for (int i = 0; i < Data.Length; i++)
                {
                    if (Data[i].Category == category)
                        return Data[i];
                }
                return null;
            }
        }

        static private CachedTypeData CachedData
        {
            get
            {
                lock (mCachedData)
                {
                    if (mCachedData.IsEmpty && Application.isPlaying)
                    {
                        var loaded = LoadCachedTypeList();
                        if (loaded != null)
                            mCachedData = loaded;
                    }
                    return mCachedData;
                }
            }
        }

        private static CachedTypeData mCachedData = new CachedTypeData();

        const string FILE_PATH = "System/CachedTypes";

        static public bool SaveCachedTypeList()
        {
            if (mCachedData == null || !IsTypeCaching || UseCachedTypes || !Application.isPlaying || !Application.isEditor)
                return false;

            var fullPath = Application.dataPath + "/Resources/" + FILE_PATH + ".json";
            //var jsonText = JsonConvert.SerializeObject(mCachedData,Formatting.Indented);
            var jsonText = LitJson.JsonMapper.ToJson(mCachedData);

            using (var file = System.IO.File.CreateText(fullPath))
            {
                file.Write(jsonText);
            }
            return true;
        }

        static CachedTypeData LoadCachedTypeList()
        {
            try
            {
                TextAsset textAsset = Resources.Load<TextAsset>(FILE_PATH);

                if (textAsset != null)
                {
                    var text = textAsset.text;

                    if (!string.IsNullOrEmpty(text))
                    {
                        //return JsonConvert.DeserializeObject<CachedTypeData>(text);
                        return JsonMapper.ToObject<CachedTypeData>(text);
                    }
                }
            }
            catch (System.Exception e)
            {
                Dev.Logger.LogError("[GameModule] LoadCachedTypeList() => " + e);
            }

            return null;
        }

        /// <summary>
        /// 모든 모듈에 접근하지 않기 위해서 네임스페이스를 검사합니다.
        /// </summary>
        public static bool InGameModules(Type type)
        {
            // [2016-07-07] narlamy
            // 클래스/구조체 내부에 정의된 클래스/구조체는 모듈 리스트에서 제외합니다.
            // 내부 클래스를 serializing 할 수는 있지만 모듈로 처리하지는 않겠습니다.
            // iOS 디바이스에서 내부 클래스는 Type 인스턴스로 생성하지 못 하는군요.
            //
            // 추상 클래스를 제거하려고 했으나 interface와 Static Class 역시 추상 클래스여서
            // 제외시킬 수 없었습니다. TableProxy 싱글톤을 수집해야 하는데 대부분의 TableProxy가
            // static class 입니다.

            return type != null && !type.IsNested &&
                !string.IsNullOrEmpty(type.Namespace) && type.Namespace.StartsWith("SF");
        }
    }
}