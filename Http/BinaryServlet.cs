/********************************************************************
	created:	2015/03/25
	author:		王萌	
	purpose:	二进制servlet
	审核信息:
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Net.Binary;

namespace Net.Http
{
    class BinaryServlet : HttpServlet
    {
        protected BinaryServlet(){}

        static public HttpServlet ServletCreate()
        {
            return new BinaryServlet();
        }

        override protected void Process()
        {
            BinaryDataParser parser = new BinaryDataParser(this);
            List<Message> msgList = parser.ParseMessage();
            parser.AddMsgListToProcess(msgList);
            SessionOwner = parser.SessionOwner;
        }

        override public bool IsProcessFinish()
        {
            return bProcessFinish;
        }

        override public void OnFinish()
        {
            mSession.ParseSendMsg();
        }
    }
}
