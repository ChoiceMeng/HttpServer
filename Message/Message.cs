/********************************************************************
	created:	2015/03/23
	author:		王萌	
	purpose:	消息基类
	审核信息:   建议: 1、MessageDef这个类放到单独的文件中, dataFlag用const修饰
 *                   2、Deserialize函数用protected修饰
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Net.Binary;

namespace Net
{
    public class Message
    {
        private long nSessionId;              // 用来找到对应的HttpListenerContext
        public long SessionId
        {
            get { return nSessionId; }
            set { nSessionId = value; }
        }
        private short mMsgType = 0;              // 消息id
        public short MsgType
        {
            get{return mMsgType;}
            set { mMsgType = value; }
        }
        private string mRespondString = "";
        public string RespondString
        {
            get { return mRespondString; }
            set { mRespondString = value; }
        }

        // 二进制流读取器
        private BinaryStream mStream = new BinaryStream();

        /// <summary>
        /// 消息内容长度,不包含消息类型(short)和消息长度(int)
        /// </summary>
        private int mContentLength = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Message()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Message(byte[] Content)
        {
            mStream.SetContent(Content);
        }

        /// <summary>
        /// 设置消息内容
        /// </summary>
        /// <param name="mContent"></param>
        public void SetContent(byte[] mContent)
        {
            mStream.SetContent(mContent);
        }

        /// <summary>
        /// 获得读取消息内容
        /// </summary>
        /// <returns></returns>
        public byte[] GetContent()
        {
            return mStream.GetContent();
        }

        /// <summary>
        /// 获得写入消息内容
        /// </summary>
        /// <returns></returns>
        public byte[] GetWriteBuffer()
        {
            return mStream.GetWriteBuffer();
        }

        /// <summary>
        /// 解构消息:发送消息
        /// </summary>
        /// <param name="?"></param>
        public void DeconstructObj()
        {
            mStream.DeconstructMessage();

            mStream.Write(mMsgType);
            int nConetLength = 0;
            mStream.Write(nConetLength);

            // 序列化
            Serial();

            // 序列化完成之后再覆盖消息长度数据
            mStream.SeekAndWrite(sizeof(short), mStream.GetWriteBuffer().Length - sizeof(int) - sizeof(short));

            mStream.CloseIOStream();
        }

        /// <summary>
        /// 序列化消息
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void Serial()
        {

        }

        /// <summary>
        /// 解析消息:接收消息
        /// </summary>
        /// <param name="?"></param>
        public void ParseObj()
        {
/*            mStream = stream;*/
/*            SetContent(stream.GetContent());*/
            mStream.ParseMessage();

            // 反序列化
            Deserialize();

            mStream.CloseIOStream();
        }

        /// <summary>
        /// 反序列化消息
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void Deserialize()
        {

        }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        protected short ReadShort()
        {
            return mStream.ReadShort();
        }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        protected int ReadInt32()
        {
            return mStream.ReadInt32();
        }

        /// <summary>
        /// 获得一个UInt
        /// </summary>
        /// <returns></returns>
        protected uint ReadUInt32()
        {
            return mStream.ReadUInt32();
        }

        /// <summary>
        /// 获得一个Int64
        /// </summary>
        /// <returns></returns>
        protected long ReadInt64()
        {
            return mStream.ReadInt64();
        }

        /// <summary>
        /// 读一个字节
        /// </summary>
        /// <returns></returns>
        protected byte ReadByte()
        {
            return mStream.ReadByte();
        }

        /// <summary>
        /// 读字符串
        /// </summary>
        /// <returns></returns>
        protected bool ReadBool()
        {
            return mStream.ReadByte() == 1;
        }

        /// <summary>
        /// 读字符串
        /// </summary>
        /// <returns></returns>
        protected string ReadString()
        {
            return mStream.ReadString();
        }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        protected void Write(int nValue)
        {
            mContentLength += sizeof(int);
            mStream.Write(nValue);
        }

        /// <summary>
        /// 写一个UInt
        /// </summary>
        /// <returns></returns>
        protected void Write(uint nValue)
        {
            mContentLength += sizeof(uint);
            mStream.Write(nValue);
        }

        /// <summary>
        /// 写一个Int64
        /// </summary>
        /// <returns></returns>
        protected void Write(long nValue)
        {
            mContentLength += sizeof(long);
            mStream.Write(nValue);
        }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        protected void Write(byte byValue)
        {
            mContentLength += sizeof(byte);
            mStream.Write(byValue);
        }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        protected void Write(bool bValue)
        {
            mContentLength += sizeof(bool);
            mStream.Write(bValue ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// 写一个字符串
        /// </summary>
        /// <returns></returns>
        protected void Write(string sValue)
        {
            mContentLength += System.Text.Encoding.UTF8.GetBytes(sValue).Length + 1;
            mStream.Write(sValue);
        }

        /// <summary>
        /// 写一个short
        /// </summary>
        /// <returns></returns>
        protected void Write(short sValue)
        {
            mContentLength += sizeof(short);
            mStream.Write(sValue);
        }
    }
}
