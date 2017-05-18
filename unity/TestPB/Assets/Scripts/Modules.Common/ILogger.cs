using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N2.Dev
{
    /// <summary>
    /// 로그 
    /// </summary>
    public interface ILogger
    {
        bool Enabled { get; set; }

        void Log(string msg, params object[] p);

        void LogWarning(string msg, params object[] p);

        void LogError(string msg, params object[] p);

        void LogException(Exception e);

        void Assert(bool condition, string msg, params object[] p);
    }

    public static class LoggerUtil
    {
        /// <summary>
        /// 현재 시간을 포함하는 로그
        /// </summary>
        public static void TimeLog(this ILogger logger, string msg)
        {
            var now = DateTime.Now;

            Dev.Logger.Log(
                string.Format(
                    "[{0:02}/{1:02}T{2:02}:{3:02}:{4:02}] " + msg, 
                    now.Month, now.Day, now.Hour, now.Minute, now.Second));
        }

        public static void TimeLog(string format, params object[] p)
        {
			var now = DateTime.Now;

			Dev.Logger.Log(
                string.Format(
					"[{0:02}/{1:02}T{2:02}:{3:02}:{4:02}] ",
					now.Month, now.Day, now.Hour, now.Minute, now.Second), p);
        }
    }

    /// <summary>
    /// 로그 인스턴스 생성하는 팩토리
    /// </summary>
    public interface ILoggerFactory
    {
        ILogger Create();
    }
}
