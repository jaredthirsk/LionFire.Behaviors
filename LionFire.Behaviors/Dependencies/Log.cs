#if NoDeps
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire
{
    public interface ILogger
    {
        void Trace(Func<string> f);
        void Trace(string msg);
        void Error(string msg);
        void Debug(string msg);
        void Warn(string msg);
        void Info(string msg);
    }
    internal class NullLog : ILogger
    {

        public void Trace(Func<string> f)
        {
        }
        public void Trace(string msg)
        {
        }
            public void Debug(string msg)
        {
        }
        public void Error(string msg)
        {
        }

        public void Warn(string msg)
        {
        }

        public void Info(string msg)
        {
        }
    }

    public static class Log
    {
        public static ILogger Get()
        {
            return NullLog;
        }
        private static NullLog NullLog = new NullLog();
    }
}
#endif