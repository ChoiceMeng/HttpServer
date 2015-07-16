/********************************************************************
	created:	2015/03/19
	author:		王萌	
	purpose:	
	审核信息:
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Net.Http
{
    class HttpFunctionServlet : HttpServlet
    {
        protected HttpFunctionServlet(){}

        static public HttpServlet ServletCreate()
        {
            return new HttpFunctionServlet();
        }

        override protected void Process()
        {
            HttpListenerRequest reqe = mContext.Request;
            string path = reqe.Url.AbsolutePath;

            if (mProcesser.FindFunc(path) != null)
            {
                mProcesser.FindFunc(path)(reqe.Url.Query);
            }
        }

        override public bool IsProcessFinish()
        {
            return bProcessFinish;
        }

        override public void OnFinish()
        {

        }
    }
}
