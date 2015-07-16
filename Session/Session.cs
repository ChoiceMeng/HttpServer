/********************************************************************
	created:	2015/03/24
	author:		王萌	
	purpose:	服务器Session
	审核信息:   Respond这个函数的功能最好放在子线程
    审核人:     陈宗鑫
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Net.Http;
using Net.Manager;

namespace Net
{
    public class Session
    {
        /// <summary>
        /// id
        /// </summary>
        private long lSessionId;
        /// servlet
        /// </summary>
        private HttpServlet mServletOwner;
        /// <summary>
        /// 消息处理器
        /// </summary>
        private ServerMsgManager mMsgManager = null;

        public Session(long lSessionid, HttpServlet servlet)
        {
            lSessionId = lSessionid;
            mServletOwner = servlet;
            mMsgManager = new ServerMsgManager(lSessionid, mServletOwner);
        }

        public long SessionId
        {
            set { lSessionId = value; }
            get { return lSessionId; }
        }

        public HttpServlet Servlet
        {
            set { mServletOwner = value; }
        }

        public void AddProcessMsg(List<Message> msgList)
        {
            MsgHandler.GetIns().AddProcessMsg(msgList);
        }

        /// <summary>
        /// 外部可调用
        /// </summary>
        public void AddSendMsg(Message msg)
        {
            mMsgManager.AddSendMsg(msg);
        }

        /// <summary>
        /// 外部可调用
        /// </summary>
        public void Resopnd()
        {
            mMsgManager.StartSendMsg();
        }

        public void RespondImp(string szRespond)
        {
            byte[] buffer = Convert.FromBase64String(szRespond);
            mServletOwner.Context.Response.ContentLength64 = buffer.Length;
            System.IO.Stream output = mServletOwner.Context.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Flush();
            output.Close();
        }

        public void ParseSendMsg()
        {
            mMsgManager.ParseAndAddMsg(lSessionId);
        }
    }
}
