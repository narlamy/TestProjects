using System;

namespace SF.Dev
{
    /// <summary>
    /// 로그 출력 : 지정된 로그
    /// </summary>
    public static class Logger
    {
        static ILogger mInstance = null;
        static ILoggerFactory mFactory = new DefaultLoggerFactory();

        public static Dev.ILogger Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = mFactory.Create();
                return mInstance;
            }
        }

        /// <summary>
        /// 로그 인스턴스를 생성할 수 있는 팩토리를 교체합니다.
        /// 기존 로그 인스턴스는 삭제되어 변경된 팩토리에 영향을 받습니다.
        /// </summary>
        public static void ChangeLoggerFactory(ILoggerFactory factory)
        {
            mFactory = factory;
            mInstance = null;
        }

        /// <summary>
        /// 로그 인스턴스를 초기화합니다.
        /// </summary>
        public static void Reset()
        {
            mInstance = null;
        }

        public static void Log(string msg)
        {
            Instance.Log(msg);
        }

        public static void Log(string format, params object[] p)
        {
            Instance.Log(format, p);
        }

        public static void LogWarning(string msg)
        {
            Instance.LogWarning(msg);
        }

        public static void LogWarning(System.Exception e)
        {
            Instance.LogWarning(e.ToString());
        }

        public static void LogWarning(string format, params object[] p)
        {
            Instance.LogWarning(format, p);
        }

        public static void LogError(System.Exception e)
        {
            Instance.LogError(e.ToString());
        }

        public static void LogException(System.Exception e)
        {
            Instance.LogException(e);
        }

        public static void LogError(string msg)
        {
            Instance.LogError(msg);
        }

        public static void LogError(string format, params object[] p)
        {
            Instance.LogError(format, p);
        }

        public static void Assert(bool condition, string format, params object[] p)
        {
            Instance.Assert(condition, format, p);
        }
    }

    #region  더미 팩토리
    /// <summary>
    /// 기본 로그 팩토리
    /// </summary>
    public class DefaultLoggerFactory : ILoggerFactory
    {
        public ILogger Create()
        {
            return new DummyLogger();
        }
    }

    /// <summary>
    /// 아무 동작하지 않는 로그 인스턴스
    /// </summary>
    class DummyLogger : ILogger
    {
        public bool Enabled { get; set; }

        public void Assert(bool condition, string format, params object[] p)
        {
        }

        public void Log(string format, params object[] p)
        {
        }

        public void LogError(string format, params object[] p)
        {
        }

        public void LogException(Exception e)
        {
        }

        public void LogWarning(string format, params object[] p)
        {
        }
    }
    #endregion
}
