using System.Diagnostics;
using System.Threading;
using System;
using System.Text;
using System.IO;
using UnityEngine;

namespace GameLib
{
    public static class Log
    {
        private static readonly bool m_LogDebugEnabled = true;
        private static readonly bool m_StackTrack = true;

        private static readonly int m_MaxLogFrameCount = 1;
        private static readonly int m_MaxLogFileSize = 5 * 1024 * 1024;
        private static readonly int m_MaxLastLogSize = 60 * 1024;
        private static readonly int m_MaxLogLineSize = 3000;

        private static readonly string m_LogFileName = "GameLib.log";
        private static string m_LogFilePath;

        private static readonly StringBuilder m_StackBuilder = new StringBuilder();
        private static readonly StringBuilder m_LastLogBuilder = new StringBuilder();

        private static readonly object m_WriteLock = new object();

        private static void PrintLine(LogType logType, string logStr)
        {
            string logLine = string.Empty;

            lock (m_StackBuilder)
            {
                m_StackBuilder.Length = 0;

                if (m_StackTrack)
                {
                    var stackTrace = new StackTrace(true);

                    int frameIndex = Application.platform == RuntimePlatform.IPhonePlayer ? 1 : 2;

                    for (int printIndex = 0; printIndex < m_MaxLogFrameCount && frameIndex < stackTrace.FrameCount - 1; printIndex++, frameIndex++)
                    {
                        var stackFrame = stackTrace.GetFrame(frameIndex);

                        string fullFileName = stackFrame.GetFileName();
                        string fileName = string.Empty;

                        if (!string.IsNullOrEmpty(fullFileName))
                        {
                            string[] fileNames = fullFileName.Split(new char[2] { '\\', '/' });

                            if (fileNames.Length > 0)
                            {
                                fileName = fileNames[fileNames.Length - 1];
                            }
                        }

                        var method = stackFrame.GetMethod();

                        m_StackBuilder.Append(string.Concat(
                            printIndex > 0 ? "<-" : "",
                            fileName, ":",
                            stackFrame.GetFileLineNumber(), ",",
                            method != null ? method.ToString() : "Unknown"
                        ));
                    }
                }

                logLine = string.Concat(
                    logType == LogType.Log ? "" : (logType.ToString() + " "),
                    DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff"),
                    " (", Thread.CurrentThread.ManagedThreadId.ToString(), ")",
                    " [", m_StackBuilder.ToString(), "] ",
                    logStr, "\n"
                );
            }

            AppendToLastLog(logLine);

            if (m_LogDebugEnabled)
            {
                WriteLogToFile(logLine);
#if UNITY_EDITOR
                WriteLogToConsole(logType, logLine);
#endif
            }
        }

        private static void AppendToLastLog(string logLine)
        {
            if (m_MaxLastLogSize <= 0)
            {
                return;
            }

            lock (m_WriteLock)
            {
                if (logLine.Length > m_MaxLogLineSize)
                {
                    logLine = logLine.Substring(0, m_MaxLogLineSize);
                }

                m_LastLogBuilder.Append(logLine);

                if (m_LastLogBuilder.Length > m_MaxLastLogSize)
                {
                    m_LastLogBuilder.Remove(0, m_MaxLastLogSize / 2);
                }
            }
        }

        private static void WriteLogToConsole(LogType logType, string logLine)
        {
#if !UNITY_EDITOR
			Console.Write(sLogLine);
#else
            if (logType == LogType.Error)
                UnityEngine.Debug.LogError(logLine);
            else
                UnityEngine.Debug.Log(logLine);
#endif
        }

        private static void WriteLogToFile(string logLine)
        {
            string logFilePath = GetLogFilePath();

            if (string.IsNullOrEmpty(logFilePath))
            {
                return;
            }

            lock (m_WriteLock)
            {
                try
                {
                    if (!File.Exists(logFilePath))
                    {
                        File.WriteAllText(logFilePath, logLine);
                    }
                    else
                    {
                        CheckLogFileLength(logFilePath);
                        File.AppendAllText(logFilePath, logLine);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.ToString());
                }
            }
        }

        private static void CheckLogFileLength(string logFilePath)
        {
            var fileInfo = new FileInfo(logFilePath);

            if (fileInfo.Length > m_MaxLogFileSize)
            {
                var binaryReader = new BinaryReader(File.Open(logFilePath, FileMode.Open, FileAccess.ReadWrite));
                int pos = m_MaxLogFileSize / 2;
                binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
                byte[] readBytes = binaryReader.ReadBytes(pos + 1);
                binaryReader.Close();

                var binaryWriter = new BinaryWriter(File.Open(logFilePath, FileMode.Create, FileAccess.ReadWrite));
                binaryWriter.Write(readBytes);
                binaryWriter.Close();
            }
        }

        private static string GetLogFilePath()
        {
            if (string.IsNullOrEmpty(m_LogFilePath))
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_STANDALONE_OSX
                string logFolder = Application.dataPath + "/../GameLog/";

                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                m_LogFilePath = logFolder + m_LogFileName;
#elif UNITY_ANDROID
                m_LogFilePath = "/sdcard/" + m_LogFileName;
#elif UNITY_IPHONE
                m_LogFilePath = Application.temporaryCachePath + "/" + m_LogFileName; // Application.persistentDataPath
#endif
            }

            return m_LogFilePath;
        }

        public static void HandleException(string logString, string stackTrace, LogType logType)
        {
            string logLine = string.Concat(
                logType.ToString(), " ",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), " ",
                logString, "\n",
                stackTrace);

            AppendToLastLog(logLine);
            WriteLogToFile(logLine);
        }

        #region Debug with string.Concat

        public static void DebugFormat(string formatStr, params object[] args)
        {
            PrintLine(LogType.Log, string.Format(formatStr, args));
        }

        public static void Debug(string str)
        {
            PrintLine(LogType.Log, str);
        }

        public static void Debug(string str1, string str2)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2));
        }

        public static void Debug(string str1, string str2, string str3)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3));
        }

        public static void Debug(string str1, string str2, string str3, string str4)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4));
        }

        public static void Debug(string str1, string str2, string str3, string str4, string str5)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4, str5));
        }

        public static void Debug(string str1, string str2, string str3, string str4, string str5, string str6)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4, str5, str6));
        }

        public static void Debug(string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4, str5, str6, str7));
        }

        public static void Debug(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8));
        }

        public static void Debug(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9));
        }

        public static void Debug(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10));
        }

        public static void Debug(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11));
        }

        public static void Debug(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11, string str12)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12));
        }

        #endregion

        #region Warning with string.Concat

        public static void WarningFormat(string formatStr, params object[] args)
        {
            PrintLine(LogType.Warning, string.Format(formatStr, args));
        }

        public static void Warning(string str)
        {
            PrintLine(LogType.Warning, str);
        }

        public static void Warning(string str1, string str2)
        {
            PrintLine(LogType.Warning, string.Concat(str1, str2));
        }

        public static void Warning(string str1, string str2, string str3)
        {
            PrintLine(LogType.Warning, string.Concat(str1, str2, str3));
        }

        public static void Warning(string str1, string str2, string str3, string str4)
        {
            PrintLine(LogType.Warning, string.Concat(str1, str2, str3, str4));
        }

        public static void Warning(string str1, string str2, string str3, string str4, string str5)
        {
            PrintLine(LogType.Warning, string.Concat(str1, str2, str3, str4, str5));
        }

        public static void Warning(string str1, string str2, string str3, string str4, string str5, string str6)
        {
            PrintLine(LogType.Warning, string.Concat(str1, str2, str3, str4, str5, str6));
        }

        public static void Warning(string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            PrintLine(LogType.Warning, string.Concat(str1, str2, str3, str4, str5, str6, str7));
        }

        public static void Warning(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
        {
            PrintLine(LogType.Warning, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8));
        }

        public static void Warning(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
        {
            PrintLine(LogType.Warning, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9));
        }

        public static void Warning(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10)
        {
            PrintLine(LogType.Warning, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10));
        }

        public static void Warning(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11));
        }

        public static void Warning(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11, string str12)
        {
            PrintLine(LogType.Log, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12));
        }

        #endregion

        #region Error with string.Concat

        public static void ErrorFormat(string formatStr, params object[] args)
        {
            PrintLine(LogType.Error, string.Format(formatStr, args));
        }

        public static void Error(string str)
        {
            PrintLine(LogType.Error, str);
        }

        public static void Error(string str1, string str2)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2));
        }

        public static void Error(string str1, string str2, string str3)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2, str3));
        }

        public static void Error(string str1, string str2, string str3, string str4)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2, str3, str4));
        }

        public static void Error(string str1, string str2, string str3, string str4, string str5)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2, str3, str4, str5));
        }

        public static void Error(string str1, string str2, string str3, string str4, string str5, string str6)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2, str3, str4, str5, str6));
        }

        public static void Error(string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2, str3, str4, str5, str6, str7));
        }

        public static void Error(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8));
        }

        public static void Error(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9));
        }

        public static void Error(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10));
        }

        public static void Error(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11));
        }

        public static void Error(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11, string str12)
        {
            PrintLine(LogType.Error, string.Concat(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12));
        }

        #endregion
    }
}