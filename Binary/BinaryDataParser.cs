/********************************************************************
	created:	2015/03/23
	author:		王萌	
	purpose:	二进制数据解析器，解析出相应的消息
	审核信息:
*********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Net.Http;
using Net.Manager;
using System.Web;

namespace Net.Binary
{
    class BinaryDataParser
    {
        BinaryStream stream = null;
        MsgHandler processer = MsgHandler.GetIns();
        Session mSession = null;
        /// <summary>
        /// send ip
        /// </summary>
        private string mIP;
        /// <summary>
        /// string data
        /// </summary>
        private string szData;
        public string StringContent
        {
            set { szData = value; }
            get { return szData; }
        }
        /// <summary>
        /// data content
        /// </summary>
        public byte[] DataContent
        {
            set { stream.SetContent(value); }
        }

        /// <summary>
        /// 指定发送目标ip
        /// </summary>
        public void SetSendIP(string szIP)
        {
            mIP = szIP;
        }

        public Session SessionOwner
        {
            set { mSession = value; }
            get { return mSession; }
        }

        /// <summary>
        /// 服务端使用的构造函数
        /// </summary>
        /// <param name="servlet"></param>
        public BinaryDataParser(HttpServlet servlet)
        {
            string strContent = servlet.getContentText();
            stream = new BinaryStream();
            mSession = SessionMgr.GetIns().CreateSession(servlet);

            stream.SetContent(Convert.FromBase64String(strContent));
        }

        /// <summary>
        /// 客户端使用的构造函数
        /// </summary>
        public BinaryDataParser()
        {
            mSession = SessionMgr.GetIns().CreateSession();
            stream = new BinaryStream();
        }

        /// <summary>
        /// 解构数据：服务端/客户端将接收到的data数据反序列化成相应消息结构
        /// </summary>
        public List<Message> ParseMessage()
        {
            List<Message> msgList = new List<Message>();
            stream.ParseMessage();
            // 先解析出这个包包含的消息数量
            int sMsgCount = stream.ReadInt32();
            for (int i = 0; i < sMsgCount; ++i)
            {
                short sMsgType = stream.ReadShort();
                Message msg = processer.CreateMsg(sMsgType);
                if (msg == null)
                {
                    // error log
                    continue;
                }
                msg.MsgType = sMsgType;

                int sMsgLength = stream.ReadInt32();
                byte[] msgContent = stream.ReadBytes(sMsgLength);
                msg.SessionId = mSession.SessionId;
                msg.SetContent(msgContent);
                msg.ParseObj();

                msgList.Add(msg);
            }

            return msgList;
        }

        public void AddMsgListToProcess(List<Message> msgList)
        {
            mSession.AddProcessMsg(msgList);
        }

        /// <summary>
        /// 调用序列化数据:客户端将消息体序列化成字符串发送给服务器
        /// </summary>
        public byte[] SerializeMsg(Message msg)
        {
            BinaryStream bs = new BinaryStream();
            bs.DeconstructMessage();
            int nMsgCount = 1;
            bs.Write(nMsgCount);

            msg.DeconstructObj();
            byte[] msgData = msg.GetWriteBuffer();
            bs.Write(msgData);

            byte[] bySend = bs.GetWriteBuffer();

            return bySend;
        }
    }
}
