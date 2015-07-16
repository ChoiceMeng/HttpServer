using Common.Log;
/********************************************************************
	created:	2015/03/25
	author:		王萌	
	purpose:	
	审核信息:
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Net.Manager
{
    public class MsgHandler
    {
        static MsgHandler ins = null;
        static public MsgHandler GetIns()
        {
            if (ins == null)
            {
                ins = new MsgHandler();
                return ins;
            }

            return ins;
        }

        private int nProcessCountPerFrame = 10;
        public int ProcessCountPerFrame
        {
            set { nProcessCountPerFrame = value; }
            get { return nProcessCountPerFrame; }
        }

        /// <summary>
        /// 由客户端序列化出来的处理消息
        /// </summary>
        private Queue<Message> mProcessMsgs = new Queue<Message>();
        /// <summary>
        /// 处理消息的锁
        /// </summary>
        private object mProcessLocker = new object();
        /// <summary>
        /// respond content list:由mSendMsgs序列化后的字符串队列
        /// </summary>
        private Queue<RespondObj> mResponds = new Queue<RespondObj>();
        /// <summary>
        /// 发送消息的锁
        /// </summary>
        private object mRespondLocker = new object();

        Dictionary<short, MsgHandleFunc> msgHandleFuncDic = new Dictionary<short,MsgHandleFunc>();
        Dictionary<short, MsgCreateFunc> msgCreateFuncDic = new Dictionary<short, MsgCreateFunc>();
        
        public void RegisterMsgHandleFunc(short msgType, MsgHandleFunc func)
        {
            if (!msgHandleFuncDic.ContainsKey(msgType))
                msgHandleFuncDic.Add(msgType, func);
            else
                msgHandleFuncDic[msgType] += func;
        }

        public void RegisterMsgCreateFunc(short msgType, MsgCreateFunc func)
        {
            if (!msgCreateFuncDic.ContainsKey(msgType))
                msgCreateFuncDic.Add(msgType, func);
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="mt"></param>
        /// <param name="msgObj"></param>
        public void HandleMessage(short mt, Message msgObj)
        {
            if (!this.msgHandleFuncDic.ContainsKey(mt))
                return;

            this.msgHandleFuncDic[mt](msgObj.SessionId, msgObj);
        }

        public Message CreateMsg(short msgType)
        {
            if (msgCreateFuncDic.ContainsKey(msgType))
                return msgCreateFuncDic[msgType]();

            return null;
        }

        public void MainThread()
        {
            HandleProcess();
            HandleRespondMsg();
        }

        public void AddProcessMsg(List<Message> msgList)
        {
            for (int i = 0; i < msgList.Count; ++i)
            {
                lock (mProcessLocker)
                {
                    mProcessMsgs.Enqueue(msgList[i]);
                }
            }
        }

        /// <summary>
        /// 获得接收消息
        /// </summary>
        /// <returns></returns>
        public Message PopProcessMessage()
        {
            lock (mProcessLocker)
            {
                if (this.mProcessMsgs.Count > 0)
                {
                    Message msg = this.mProcessMsgs.Dequeue();
                    return msg;
                }

                return null;
            }
        }

        /// <summary>
        /// 获得接收消息的个数
        /// </summary>
        /// <returns></returns>
        public int GetProcessMessageCount()
        {
            lock (mProcessLocker)
            {
                return this.mProcessMsgs.Count;
            }
        }

        /// <summary>
        /// 处理接收消息:主线程
        /// </summary>
        protected void HandleRespondMsg()
        {
            int nDoneCount = 0;
            while (this.GetRespondMessageCount() > 0)
            {
                if (nDoneCount >= nProcessCountPerFrame) break;

                try
                {
                    RespondObj msg = PopRespondMessage();
                    if (msg == null)
                        continue;

                    Session ses = SessionMgr.GetIns().GetSession(msg.SessionId);
                    if(ses != null)
                    // respond客户端消息
                        ses.RespondImp(msg.RespondString);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                nDoneCount++;
            }
        }

        /// <summary>
        /// 主线程处理函数
        /// </summary>
        void HandleProcess()
        {
            int nDoneCount = 0;
            while (this.GetProcessMessageCount() > 0)
            {
                if (nDoneCount >= nProcessCountPerFrame) break;
                try
                {
                    Message msg = PopProcessMessage();
                    if (msg == null)
                        continue;

                    MsgHandler.GetIns().HandleMessage(msg.MsgType, msg);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }


        /// <summary>
        /// 获得需要respond的消息个数
        /// </summary>
        /// <returns></returns>
        public int GetRespondMessageCount()
        {
            lock (mRespondLocker)
            {
                return this.mResponds.Count;
            }
        }

        /// <summary>
        /// 获得发送消息
        /// </summary>
        /// <returns></returns>
        public RespondObj PopRespondMessage()
        {
            lock (mRespondLocker)
            {
                if (this.mResponds.Count > 0)
                {
                    RespondObj msg = mResponds.Dequeue();
                    return msg;
                }

                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddRespondMsg(RespondObj resObj)
        {
            lock (mRespondLocker)
            {
                mResponds.Enqueue(resObj);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SendMessage(long lSessionId, Message msg)
        {
            Session ses = SessionMgr.GetIns().GetSession(lSessionId);
            if (ses != null)
            {
                ses.AddSendMsg(msg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void RespondMessage(long lSessionId)
        {
            Session ses = SessionMgr.GetIns().GetSession(lSessionId);
            if (ses != null)
            {
                ses.Resopnd();
            }
        }
    }
}
