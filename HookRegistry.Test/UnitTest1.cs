using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hooks;
using System.Collections.Generic;

namespace HookRegistry.Test
{
	[TestClass]
	public class UnitTest1
	{
		CmdServer serv = new CmdServer();
		[TestMethod]
		public void TestMethod1()
		{
			
			serv.Start();
			Console.Read();
		}

		[TestMethod]
		public void TestAddItem()
		{
			var dic = new Dictionary<string, string>();
			dic.Add("cmd", "AddItem");
			dic.Add("ids", "1,2");
			string res =  serv.DoCmd(dic);
		}
	}
}
