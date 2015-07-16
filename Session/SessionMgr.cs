/********************************************************************
	created:	2015/03/24
	author:		王萌	
	purpose:	服务器Session管理
	审核信息:   1、加注释
 *              2、建议库文件中没有单例，用类的组合
 *  审核人:     陈宗鑫
 *  审核时间：  2015年3月25日 11:50:23
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Net.Http;

namespace Net
{
    class SessionMgr
    {
        Dictionary<long, Session> mSessionDic = new Dictionary<long, Session>();
        long lSessionCount = 0;

        static SessionMgr ins = null;
        static public SessionMgr GetIns()
        {
            if (ins == null)
            {
                ins = new SessionMgr();
                return ins;
            }

            return ins;
        }

        public Session CreateSession(HttpServlet servlet)
        {
            lSessionCount++;
            Session newSession = new Session(lSessionCount, servlet);
            mSessionDic.Add(lSessionCount, newSession);

            return newSession;
        }

        public Session CreateSession()
        {
            Session newSession = new Session(0, null);
            mSessionDic.Add(lSessionCount, newSession);

            return newSession;
        }

        public Session GetSession(long nSessionID)
        {
            if (mSessionDic.ContainsKey(nSessionID))
                return mSessionDic[nSessionID];

            return null;
        }
    }
}
