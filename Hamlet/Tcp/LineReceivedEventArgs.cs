using System;

namespace Hamlet.Tcp
{
    internal class LineReceivedEventArgs : EventArgs
    {
        public string Line;
        public string Prefix
        {
            get
            {
                if (!Line.StartsWith(":"))
                    return "";
                var ix = Line.IndexOf(' ');
                if (ix > -1)
                {
                    return Line.Substring(1, ix);
                }
                throw new FormatException("expected ^[ \":\" prefix SPACE ] command, but SPACE not found");
            }
        }
        public string CommandWithParams
        {
            get
            {
                var ix = Line.IndexOf(' ');
                if (Line.StartsWith(":") && ix > -1)
                    return Line.Substring(ix + 1);
                return Line;
            }
        }
        public string Command
        {
            get
            {
                var cwp = CommandWithParams;
                var ix = cwp.IndexOf(' ');
                if (ix > -1)
                    return cwp.Substring(0, ix);
                return cwp;
            }
        }
        public string Params
        {
            get
            {
                var cwp = CommandWithParams;
                var ix = cwp.IndexOf(' ');
                if (ix > -1)
                    return cwp.Substring(ix + 1);
                return "";
            }
        }
    }
}