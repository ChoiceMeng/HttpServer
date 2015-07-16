using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net
{
    public class RespondObj
    {
        private long mSessionId = 0;
        public long SessionId
        {
            set { mSessionId = value; }
            get { return mSessionId; }
        }

        private string mRespondString;
        public string RespondString
        {
            get { return mRespondString; }
            set { mRespondString = value; }
        }
    }
}
