using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saraff.Twain.DS.Extensions {

    public interface ILog {

        void Write(Exception ex);

        void Write(string message, LogLevel level = LogLevel.None);
    }

    public enum LogLevel { 
        None,
        Info,
        Warning,
        Error
    }
}
