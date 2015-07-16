/********************************************************************
	created:	2015/03/24
	author:		王萌	
	purpose:	客户端消息管理器
 *              负责客户端消息的接收与发送处理
	审核信息:
*********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Net.Binary;

namespace Net.Manager
{
    public class ClientMsgManager
    {
        private int nProcessCountPerFrame = 10;
        public int ProcessCountPerFrame
        {
            set { nProcessCountPerFrame = value; }
            get { return nProcessCountPerFrame; }
        }
        /// <summary>
        /// 处理消息
        /// </summary>
        private Queue<Message> mSendMsgs = new Queue<Message>();
        /// <summary>
        /// 发送消息的锁
        /// </summary>
        private object mSendLocker = new object();
        /// <summary>
        /// 接收消息
        /// </summary>
        private Queue<Message> mReciveMsgs = new Queue<Message>();
        /// <summary>
        /// 接收消息的锁
        /// </summary>
        private object mReciveLocker = new object();

        BinaryDataParser mParser = new BinaryDataParser();
        Thread th = null;

        /// <summary>
        /// 发送Ip
        /// </summary>
        private string mSendIp = string.Empty;

        public ClientMsgManager()
        {
            th = new Thread(ProcessSendMsg);
            th.Start();
        }

        /// <summary>
        /// set send ip
        /// </summary>
        public void SetSendIP(string szIP)
        {
            mSendIp = szIP;
            mParser.SetSendIP(szIP);
        }
        /// <summary>
        /// 
        /// </summary>
        public void SendMsg(Message msg)
        {
            lock (mSendLocker)
            {
                mSendMsgs.Enqueue(msg);
            }
        }

        /// <summary>
        /// 获得发送消息
        /// </summary>
        /// <returns></returns>
        Message PopSendMessage()
        {
            lock (mSendLocker)
            {
                if (this.mSendMsgs.Count > 0)
                {
                    Message msg = mSendMsgs.Dequeue();
                    return msg;
                }

                return null;
            }
        }

        /// <summary>
        /// 获得需要处理消息的个数
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
        /// 获得发送消息
        /// </summary>
        /// <returns></returns>
        Message PopReciveMessage()
        {
            lock (mReciveLocker)
            {
                if (this.mReciveMsgs.Count > 0)
                {
                    Message msg = mReciveMsgs.Dequeue();
                    return msg;
                }

                return null;
            }
        }

        /// <summary>
        /// 获得需要处理消息的个数
        /// </summary>
        /// <returns></returns>
        public int GetReciveMessageCount()
        {
            lock (mReciveLocker)
            {
                return this.mReciveMsgs.Count;
            }
        }

        /// <summary>
        /// 子线程处理需要发送的消息
        /// </summary>
        void ProcessSendMsg()
        {
            while (true)
            {
                try
                {
                    if (this.GetSendMessageCount() > 0)
                    {
                        Message msg = PopSendMessage();
                        if (msg == null)
                            continue;

                        // 发送服务器
                        byte[] bySend = mParser.SerializeMsg(msg);
                        SendToServer(bySend);
                    }

                    Thread.Sleep(10);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 主线程处理需要接收的消息
        /// </summary>
        public void ProcessReciveMsg()
        {
            int nDoneCount = 0;
            while (this.GetReciveMessageCount() > 0)
            {
                if (nDoneCount >= nProcessCountPerFrame) break;

                try
                {
                    Message msg = PopReciveMessage();
                    if (msg == null)
                        continue;

                    // 发送服务器
                    MsgHandler.GetIns().HandleMessage(msg.MsgType, msg);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                nDoneCount++;
            }
        }

        /// <summary>
        /// 使用webRequest向服务器发送请求
        /// </summary>
        void SendToServer(byte[] bySend)
        {
            string strText = Convert.ToBase64String(bySend);
            byte[] byResultSend = Encoding.UTF8.GetBytes(strText);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(mSendIp + MessageDef.BinaryPath);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byResultSend.Length;
            request.Method = "POST";
            request.GetRequestStream().Write(byResultSend, 0, byResultSend.Length);
            request.GetRequestStream().Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            response.Close();

            mParser.DataContent = Convert.FromBase64String(retString);
            List<Message> msgList = mParser.ParseMessage();
            for (int i = 0; i < msgList.Count; ++i)
            {
                lock(mReciveLocker)
                {
                    mReciveMsgs.Enqueue(msgList[i]);
                }                
            }
        }
    }
}
