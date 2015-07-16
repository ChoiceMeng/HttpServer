/********************************************************************
	created:	2015/03/19
	author:		王萌	
	purpose:	http消息处理器
	审核信息:   建议: 1、FuncDelegate单独列一个文件
 *                   2、函数和变量统一加public、private或者protected，并都以此为开头
 *                   3、变量与函数分开放置，建议类前头放置变量,后面放置函数
 *                   4、从队列中Dequeue变量之前判定数量, 防止误操作
 *                   5、建议把Http启动部分加到这里来
*********************************************************************/
using System;
using System.Net;
using System.Collections.Generic;
using Common.Log;
using System.Net.Sockets;

namespace Net.Http
{
    public class HttpProcesser
    {
        Dictionary<string, ServletCreater> pathToServletDic = new Dictionary<string,ServletCreater>();
        Dictionary<string, FuncDelegate> pathToFuncDic = new Dictionary<string, FuncDelegate>();

        // 正在处理中的servlets列表
        private Queue<HttpServlet> processServlets = new Queue<HttpServlet>();
        // 事务操作锁
        private object servletsLock = new object();
        private object servletDicLock = new object();

        /// <summary>
        /// HTTP的监听
        /// </summary>
        private HttpListener mListener = new HttpListener();

        // 每帧处理个数
        private int nProcessCountPerFrame = 10;
        public int ProcessCountPerFrame
        {
            set { nProcessCountPerFrame = value; }
            get { return nProcessCountPerFrame; }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            pathToServletDic.Clear();
            pathToFuncDic.Clear();
            RegisterServlet(MessageDef.BinaryPath, BinaryServlet.ServletCreate);
            RegisterServlet(MessageDef.FunctionPath, HttpFunctionServlet.ServletCreate);
            StartListener(10000);
        }

        /// <summary>
        /// 开始监听网络
        /// </summary>
        /// <param name="nPort"></param>
        private bool StartListener(int nPort)
        {
            try
            {
                List<string> allIpList = GetHostIp();
                if (allIpList == null)
                    return false;

                foreach (string strIpAddress in allIpList)
                {
                    mListener.Prefixes.Add("http://" + strIpAddress + ":" + nPort + "/");
                }
                
                mListener.Start();
                mListener.BeginGetContext(new AsyncCallback(HttpProcesserCallBack), mListener);
            }
            catch (Exception ex)
            {
                Logger.GetLog("Net").Error("Http Listener Error Port:" + nPort + ", Error List:");
                Logger.GetLog("Net").Error(ex.ToString());

                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取服务器的所有Ip
        /// </summary>
        /// <returns></returns>
        private List<string> GetHostIp()
        {
            List<string> mList = new List<string>();
            string mHostName = Dns.GetHostName();
            if( mHostName == null )
                return mList;

            IPAddress[] allIpAddress = Dns.GetHostAddresses(mHostName);
            foreach (IPAddress ipAddress in allIpAddress)
            {
                if( ipAddress.AddressFamily.Equals(AddressFamily.InterNetwork) )
                    mList.Add(ipAddress.ToString());
            }

            mList.Add("127.0.0.1");
            return mList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        void HttpProcesserCallBack(IAsyncResult result)
        {
            HttpListener listener = result.AsyncState as HttpListener;
            // 结束异步操作
            HttpListenerContext context = listener.EndGetContext(result);

            // 重新启动异步请求处理
            listener.BeginGetContext(new AsyncCallback(HttpProcesserCallBack), listener);

            MsgProcesser(context);
        }

        /// <summary>
        /// http消息处理函数
        /// </summary>
        /// <param name="context"></param>
        void MsgProcesser(HttpListenerContext context)
        {
            // 处理请求
            HttpListenerRequest reqe = context.Request;
            string path = reqe.Url.LocalPath;

            if (IsExsitInFuncDic(path))
            {
                path = MessageDef.FunctionPath;
            }

            // 已经注册过相应的servlet去处理
            HttpServlet hs = GetServletFromDic(path);
            if (hs == null) return;

            hs.Context = context;
            hs.SetProcesser(this);
            hs.GetContentText();
            PushServlet(hs);

            while (!hs.IsProcessFinish())
            {

            }

            hs.OnFinish();
        }

        /// <summary>
        /// 
        /// </summary>
        HttpServlet GetServletFromDic(string path)
        {
            lock (servletDicLock)
            {
                if (pathToServletDic.ContainsKey(path))
                {
                    return pathToServletDic[path]();
                }

                return null;
            }
        }
        
        /// <summary>
        /// 处理器主函数
        /// </summary>
        public void MainThread()
        {
            for ( int i = 0; i < nProcessCountPerFrame; ++i )
            {
                HttpServlet hs = PopServlet();
                if (hs != null)
                {
                    hs.OnStart();
                }
            }
        }

        /// <summary>
        /// pop
        /// </summary>
        HttpServlet PopServlet()
        {
            lock (servletsLock)
            {
                if (processServlets.Count > 0)
                    return processServlets.Dequeue();

                return null;
            }
        }

        /// <summary>
        /// push
        /// </summary>
        void PushServlet(HttpServlet hs)
        {
            lock (servletsLock)
            {
                processServlets.Enqueue(hs);
            }
        }

        /// <summary>
        /// 获得队列长度
        /// </summary>
        int GetServletListCount()
        {
            lock (servletsLock)
            {
                return processServlets.Count;
            }
        }

        /// <summary>
        /// 处理器主函数
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="servlet"></param>
        /// <returns></returns>
        bool RegisterServlet(string strPath, ServletCreater creater)
        {
            lock (servletDicLock)
            {
                if (pathToServletDic.ContainsKey(strPath))
                    return false;

                if (pathToFuncDic.ContainsKey(strPath))
                    return false;

                pathToServletDic.Add(strPath, creater);

                return true;
            }
        }

        bool IsExsitInServletDic(string strPath)
        {
            lock (servletDicLock)
            {
                if (pathToServletDic.ContainsKey(strPath))
                    return true;

                return false;
            }
        }

        bool IsExsitInFuncDic(string strPath)
        {
            lock (servletDicLock)
            {
                if (pathToFuncDic.ContainsKey(strPath))
                    return true;

                return false;
            }
        }
        
        /// <summary>
        /// 自定义处理函数注册
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public bool RegisterFunction(string strPath, FuncDelegate func)
        {
            lock (servletDicLock)
            {
                if (pathToServletDic.ContainsKey(strPath))
                    return false;

                if (pathToFuncDic.ContainsKey(strPath))
                    return false;

                pathToFuncDic.Add(strPath, func);

                return true;
            }
         }

        ServletCreater FindServlet(string path)
        {
            lock (servletDicLock)
            {
                if (pathToServletDic.ContainsKey(path))
                    return pathToServletDic[path];

                return null;
            }
        }

        public FuncDelegate FindFunc(string path)
        {
            lock (servletDicLock)
            {
                if (pathToFuncDic.ContainsKey(path))
                    return pathToFuncDic[path];

                return null;
            }
        }
    }
}
