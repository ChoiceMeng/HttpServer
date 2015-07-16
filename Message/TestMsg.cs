using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net
{
    public class TestMsg : Message
    {
        public int nRoleId;
        public byte nTestId;
        public string szTestString;

        protected override void Serial()
        {
            Write(nRoleId);
//             Write(szTestString);      
//             Write(nTestId);
        }

        protected override void Deserialize()
        {
            nRoleId = ReadInt32();
//             szTestString = ReadString();
//             nTestId = ReadByte();
        }

        public static TestMsg Create()
        {
            return new TestMsg();
        }
    }
}
