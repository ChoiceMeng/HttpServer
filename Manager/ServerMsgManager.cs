/********************************************************************
	created:	2015/03/24
	author:		王萌	
	purpose:	服务器消息接收发送器
	审核信息:   HandleProcess、HandleRespondMsg：一帧最好处理特定数量的消息，避免降帧
                mIP、mOwnerSessionId变量尽量初始化
 *              序列化建议放到子线程
    审核人:     陈宗鑫
    审核时间:   2015年3月25日 11:05:23
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Net.Binary;
using Net.Http;

namespace Net.Manager
{
    class ServerMsgManager
    {
        private int nProcessCountPerFrame = 10;
        public int ProcessCountPerFrame
        {
            set { nProcessCountPerFrame = value; }
            get { return nProcessCountPerFrame; }
        }

        /// <summary>
        /// 由服务器servlet处理函数处理后需要发送的消息队列
        /// </summary>
        private Queue<Message> mSendMsgs = new Queue<Message>();
        /// <summary>
        /// 发送消息的锁
        /// </summary>
        private object mSendLocker = new object();
        /// <summary>
        /// owner session id
        /// </summary>
        private long mOwnerSessionId = 0;
        /// <summary>
        /// HttpServlet owner
        /// </summary>
        HttpServlet mServletOwner = null;

        public ServerMsgManager( long lOwnerSessionId, HttpServlet servlet )
        {
            mOwnerSessionId = lOwnerSessionId;
            mServletOwner = servlet;
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddSendMsg(Message msg)
        {
            lock (mSendLocker)
            {
                mSendMsgs.Enqueue(msg);
            }
        }

        /// <summary>
        /// 获得发送消息的个数
        /// </summary>
        /// <returns></returns>
        public int GetSendMessageCount()
        {
            lock (mSendLocker)
            {
                return this.mSendMsgs.Count;
            }
        }

        /// <summary>
        /// HttpFunctionServlet
        /// </summary>
        void SendFunctionMsg()
        {

        }

        /// <summary>
        /// BinaryServlet
        /// to client
        /// </summary>
        public void StartSendMsg()
        {
            mServletOwner.SetProcessDone(true);
        }

        // 子线程调用
        public void ParseAndAddMsg(long lSessionId)
        {
            lock (mSendLocker)
            {
                BinaryStream bs = new BinaryStream();
                bs.DeconstructMessage();
                int nMsgCount = mSendMsgs.Count;
                bs.Write(nMsgCount);
                for (int i = 0; i < nMsgCount; ++i)
                {
                    Message msg = mSendMsgs.Dequeue();
                    msg.DeconstructObj();
                    bs.Write(msg.GetWriteBuffer());
                }

                RespondObj resObj = new RespondObj();
                resObj.SessionId = lSessionId;
                resObj.RespondString = Convert.ToBase64String(bs.GetWriteBuffer());
                // 加入到待发送列表即可
                MsgHandler.GetIns().AddRespondMsg(resObj);
            }
        }
    }
}
