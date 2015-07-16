using Common.Log;
/********************************************************************
	created:	2015/03/23
	author:		王萌	
	purpose:	二进制流读写器
	审核信息:
*********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Net.Binary
{
    public class BinaryStream
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        private byte[] mContent;

        /// <summary>
        /// 读 IO流
        /// </summary>
        private BinaryReader br = null;

        /// <summary>
        /// 写IO流
        /// </summary>
        private BinaryWriter bw = null;

        /// <summary>
        /// 读写IO流相关内存
        /// </summary>
        private MemoryStream mMs = null;

        /// <summary>
        /// 解析消息:接受消息时使用
        /// </summary>
        public void ParseMessage()
        {
            try
            {
                mMs = new MemoryStream(this.mContent);
                br = new BinaryReader(mMs);
            }
            catch (Exception ex)
            {
                Logger.GetLog("Net").Error(ex.ToString());
            }
            //finally
            //{
            //    mMs.Close();
            //}
        }

        /// <summary>
        /// 解构消息：发送消息时使用
        /// </summary>
        /// <param name="msg"></param>
        public void DeconstructMessage()
        {
            try
            {
                mMs = new MemoryStream();
                bw = new BinaryWriter(mMs);
            }
            catch (Exception ex)
            {
                Logger.GetLog("Net").Error(ex.ToString());
            }
        }

        /// <summary>
        /// 设置消息内容
        /// </summary>
        /// <param name="mContent"></param>
        public void SetContent(byte[] mContent)
        {
            this.mContent = mContent;
        }

        /// <summary>
        /// 获得消息内容
        /// </summary>
        /// <returns></returns>
        public byte[] GetContent()
        {
            return this.mContent;
        }

        /// <summary>
        /// 获得写内容
        /// </summary>
        /// <returns></returns>
        public byte[] GetWriteBuffer()
        {
            return this.mMs.ToArray();
        }

        /// <summary>
        /// 写一个short Int
        /// </summary>
        /// <returns></returns>
        public void Write(short nValue)
        {
            bw.Write(nValue);
        }

        /// <summary>
        /// 写一个Int
        /// </summary>
        /// <returns></returns>
        public void Write(int nValue)
        {
            bw.Write(nValue);
        }

        /// <summary>
        /// 写一个UInt
        /// </summary>
        /// <returns></returns>
        public void Write(uint nValue)
        {
            bw.Write(nValue);
        }

        /// <summary>
        /// 写一个Int64
        /// </summary>
        /// <returns></returns>
        public void Write(long nValue)
        {
            bw.Write(nValue);
        }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        public void Write(byte byValue)
        {
            bw.Write(byValue);
        }

        /// <summary>
        /// 写一个字符串
        /// </summary>
        /// <returns></returns>
        public void Write(string sValue)
        {
            bw.Write(sValue);
        }

        /// <summary>
        /// 写一个流内容
        /// </summary>
        /// <returns></returns>
        public void Write(byte[] sValue)
        {
            bw.Write(sValue);
        }

        /// <summary>
        /// 找到指定位置修改
        /// </summary>
        public void SeekAndWrite(int nOffset, int nData)
        {
            bw.Seek(nOffset, SeekOrigin.Begin);
            bw.Write(nData);
        }

        /// <summary>
        /// 获得一个Int
        /// </summary>
        /// <returns></returns>
        public int ReadInt32()
        {
            return br.ReadInt32();
        }

        /// <summary>
        /// 获得一个UInt
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt32()
        {
            return br.ReadUInt32();
        }

        /// <summary>
        /// 获得一个Int64
        /// </summary>
        /// <returns></returns>
        public long ReadInt64()
        {
            return br.ReadInt64();
        }

        /// <summary>
        /// 读一个字节
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            return br.ReadByte();
        }

        /// <summary>
        /// 读字符串
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            return br.ReadString();
        }

        /// <summary>
        /// 读字short
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            return br.ReadInt16();
        }

        /// <summary>
        /// 读字节数组
        /// </summary>
        public byte[] ReadBytes(int nCount)
        {
            return br.ReadBytes(nCount);
        }

        public void CloseIOStream()
        {
            if (mMs != null)
            {
                mMs.Close();
            }
        }
    }
}
