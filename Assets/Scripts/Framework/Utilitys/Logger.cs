using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Text;

public static class Logger
{
    static Logger()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
    }

    public enum LoggerLevel
    {
        Trace,
        Info,
        Warning,
        Error,
        None,
    }

    public static LoggerLevel Level { get; set; }

    private static StringBuilder sb = new StringBuilder();

    private static string m_WriteToFile;
    public static string WriteToFile
    {
        get
        {
            return m_WriteToFile;
        }

        set
        {
            if (value != m_WriteToFile)
            {
                m_WriteToFile = value;

                if (!string.IsNullOrEmpty(value))
                {
                    if (File.Exists(value)) File.Delete(value);
                    Application.logMessageReceived += OnLogMessageReceived;
                    if (WriteToFileThread == null)
                    {
                        WriteToFileThread = new Thread(ThreadLoggerProc);
                        WriteToFileThread.Start();
                    }
                }
                else
                {
                    Application.logMessageReceived -= OnLogMessageReceived;
                    if (WriteToFileThread != null)
                    {
                        WriteToFileThread.Abort();
                        WriteToFileThread = null;
                    }
                }
            }
        }
    }

    private class LogMsg
    {
        public LogMsg(string msg, string stackTrace, LogType logType)
        {
            Msg = msg;
            StackTrace = stackTrace;
            LogType = logType;
        }

        public string Msg;
        public string StackTrace;
        public LogType LogType;
    }

    private static Queue<LogMsg> m_UsedLogMsgQueue = new Queue<LogMsg>(32);
    private static Queue<LogMsg> m_UnUsedLogMsgQueue = new Queue<LogMsg>(32);
    private static object _useQuenelock = new object();
    private static object _unUseQuenelock = new object();
    private static Thread WriteToFileThread;
    private static void ThreadLoggerProc()
    {
        while (true)
        {
            lock (_useQuenelock)
            {
                while (m_UsedLogMsgQueue.Count > 0)
                {
                    var logMsg = m_UsedLogMsgQueue.Dequeue();

                    File.AppendAllText(m_WriteToFile, logMsg.Msg, System.Text.UnicodeEncoding.UTF8);

                    File.AppendAllText(m_WriteToFile, "\n", System.Text.UnicodeEncoding.UTF8);

                    var stackTrace = logMsg.StackTrace;
                    if (!string.IsNullOrEmpty(stackTrace))
                    {
                        File.AppendAllText(m_WriteToFile, "Stack Trace:\n", System.Text.UnicodeEncoding.UTF8);
                        File.AppendAllText(m_WriteToFile, stackTrace, System.Text.UnicodeEncoding.UTF8);
                    }

                    lock (_unUseQuenelock) m_UnUsedLogMsgQueue.Enqueue(logMsg);
                }
            }

            Thread.Sleep(0);
        }

    }

    private static void OnLogMessageReceived(string condition, string stackTrace, LogType logType)
    {
        if (!string.IsNullOrEmpty(WriteToFile))
        {
            var now = System.DateTime.Now;
            condition = now.ToString("[HH:mm:ss]") + condition;

            var logMsg = default(LogMsg);
            lock (_unUseQuenelock)
            {
                if (m_UnUsedLogMsgQueue.Count > 0)
                {
                    logMsg = m_UnUsedLogMsgQueue.Dequeue();
                }
            }

            if (logMsg != null)
            {
                logMsg.Msg = condition;
                logMsg.StackTrace = stackTrace;
                logMsg.LogType = logType;
            }
            else
            {
                logMsg = new LogMsg(condition, stackTrace, logType);
            }

            lock (_useQuenelock) m_UsedLogMsgQueue.Enqueue(logMsg);
        }
    }

    public class LogBase<T> where T : class
    {
        protected string GetCallerLine(string msg, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            sb.Clear();

            var now = System.DateTime.Now;
            sb.Append("@");
            sb.Append(sourceFilePath);
            sb.Append($" + <{memberName}> in line : {sourceLineNumber}");
            sb.AppendLine();
            sb.Append(msg);

            return sb.ToString();
        }

        public virtual void Output(string msg, bool callline = true,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {

        }
    }

    public class LogTrace : LogBase<LogTrace>
    {
        public override void Output(string msg, bool callline = true,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            UnityEngine.Debug.Log(callline ? GetCallerLine(msg, memberName, sourceFilePath, sourceLineNumber) : msg);
        }
    }

    public class LogInfo : LogBase<LogInfo>
    {
        public override void Output(string msg, bool callline = true,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            UnityEngine.Debug.Log(callline ? GetCallerLine(msg, memberName, sourceFilePath, sourceLineNumber) : msg);
        }
    }

    public class LogWarning : LogBase<LogWarning>
    {
        public override void Output(string msg, bool callline = true,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            UnityEngine.Debug.LogWarning(callline ? GetCallerLine(msg, memberName, sourceFilePath, sourceLineNumber) : msg);
        }
    }

    public class LogError : LogBase<LogError>
    {
        public override void Output(string msg, bool callline = true,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            UnityEngine.Debug.LogError(callline ? GetCallerLine(msg, memberName, sourceFilePath, sourceLineNumber) : msg);
        }
    }

    private static LogTrace m_Trace = new LogTrace();
    public static LogTrace Trace
    {
        get
        {
            if (Level <= LoggerLevel.Trace)
            {
                return m_Trace;
            }
            return null;
        }
    }

    private static LogInfo m_Info = new LogInfo();
    public static LogInfo Info
    {
        get
        {
            if (Level <= LoggerLevel.Info)
            {
                return m_Info;
            }
            return null;
        }
    }

    private static LogWarning m_Warning = new LogWarning();
    public static LogWarning Warning
    {
        get
        {
            if (Level <= LoggerLevel.Warning)
            {
                return m_Warning;
            }
            return null;
        }
    }

    private static LogError m_Error = new LogError();
    public static LogError Error
    {
        get
        {
            if (Level <= LoggerLevel.Error)
            {
                return m_Error;
            }
            return null;
        }
    }
}
