/********************************************************************
	created:	2015/03/19
	author:		王萌	
	purpose:	httpservlet基类
	审核信息:   建议：1、封装从HttpListenerContext获取参数信息的函数,提供是否存在某个参数的函数判定
*********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Net.Http
{
    public abstract class HttpServlet
    {
        protected string mContentText = string.Empty;
        protected HttpListenerContext mContext;
        protected bool bProcessFinish = false;
        /// <summary>
        /// 记住Processer指针
        /// </summary>
        protected HttpProcesser mProcesser = null;
        /// <summary>
        /// 
        /// </summary>
        protected Session mSession = null;
        public Session SessionOwner
        {
            set { mSession = value; }
            get { return mSession; }
        }

        abstract protected void Process();
        abstract public bool IsProcessFinish();
        public void OnStart()
        {
            Process();
        }
        /// <summary>
        /// respond
        /// </summary>
        abstract public void OnFinish();

        public HttpListenerContext Context
        {
            set { mContext = value; }
            get { return mContext; }
        }

        public void SetProcesser(HttpProcesser processer)
        {
            mProcesser = processer;
        }

        public void SetProcessDone(bool bDone)
        {
            bProcessFinish = bDone;
        }

        /// <summary>
        /// 获取数值
        /// </summary>
        public void GetContentText()
        {
            Stream requestStream = Context.Request.InputStream;
            StreamReader requestReader = new StreamReader(requestStream);
            this.mContentText = requestReader.ReadToEnd();
            requestReader.Close();
        }

        public string getContentText()
        {
            return this.mContentText;
        }
    }
}
