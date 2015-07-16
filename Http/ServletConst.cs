/********************************************************************
	created:	2015/03/25
	author:		王萌	
	purpose:	常用const变量
	审核信息:
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Net.Http;

namespace Net
{
    public delegate HttpServlet ServletCreater();
    public delegate void FuncDelegate(string param);

    public delegate void MsgHandleFunc(long lSessionId, Message param);
    public delegate Message MsgCreateFunc();

    public class MessageDef
    {
        public const string DataFlag = "msgdata";
        public const string FunctionPath = "/functionservlet";
        public const string BinaryPath = "/binaryservlet";
    }
}
