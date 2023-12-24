using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Logger
{
    //private enum LVL
    //{
    //    DEBUG,
    //    INFO,
    //    WARN,
    //    ERROR,
    //    FATAL
    //}

    public class Logger
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private string projectName = "";
        private string className = "";

        private Logger() { }

        public static Logger GetLogger(string projectName, string className)
        {
            Logger logger = new Logger();
            logger.projectName = projectName;
            logger.className = className;

            return logger;
        }

        public void Debug(string msg)
        {
            msg = preprocessMessage(msg);
            if (log.IsDebugEnabled)
                log.Debug(msg);
        }

        public void Info(string msg)
        {
            msg = preprocessMessage(msg);
            if (log.IsInfoEnabled)
                log.Info(msg);
        }

        public void Warn(string msg)
        {
            msg = preprocessMessage(msg);
            if (log.IsWarnEnabled)
                log.Warn(msg);
        }

        public void Error(string msg)
        {
            msg = preprocessMessage(msg);
            if (log.IsErrorEnabled)
                log.Error(msg);
        }

        public void Fatal(string msg)
        {
            msg = preprocessMessage(msg);
            if (log.IsFatalEnabled)
                log.Fatal(msg);
        }

        private string preprocessMessage(string msg)
        {
            if (!string.IsNullOrEmpty(className))
                msg = className + " -> " + msg;
            if (!string.IsNullOrEmpty(projectName))
                msg = projectName + " -> " + msg;

            return msg;
        }

        //private void Write(LVL level, string msg)
        //{
        //    if (!string.IsNullOrEmpty(className))
        //        msg = className + " -> " + msg;
        //    if (!string.IsNullOrEmpty(projectName))
        //        msg = projectName + " -> " + msg;

        //    switch (level)
        //    {
        //        case LVL.DEBUG:
        //            if (log.IsDebugEnabled)
        //                log.Debug(msg);
        //            break;
        //        case LVL.INFO:
        //            if (log.IsInfoEnabled)
        //                log.Info(msg);
        //            break;
        //        case LVL.WARN:
        //            if (log.IsWarnEnabled)
        //                log.Warn(msg);
        //            break;
        //        case LVL.ERROR:
        //            if (log.IsErrorEnabled)
        //                log.Error(msg);
        //            break;
        //        case LVL.FATAL:
        //            if (log.IsFatalEnabled)
        //                log.Fatal(msg);
        //            break;
        //    }
        //}
    }
}
