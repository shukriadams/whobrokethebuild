using System;
using System.IO;
using System.Text;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Allows piping out of regular console output to signalr. 
    /// </summary>
    public class ConsoleWriter : TextWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public override void Write(string value)
        {
            if (WriteEvent != null) 
                WriteEvent(this, new ConsoleWriterEventArgs(value));

            base.Write(value);
        }

        public override void Write(object value)
        {
            if (WriteEvent != null)
                WriteEvent(this, new ConsoleWriterEventArgs(value.ToString()));

            base.Write(value);
        }

        public override void WriteLine(string value)
        {
            if (WriteEvent != null)
                WriteEvent(this, new ConsoleWriterEventArgs(value));

            base.WriteLine(value);
        }

        public event EventHandler<ConsoleWriterEventArgs> WriteEvent;
    }
}
