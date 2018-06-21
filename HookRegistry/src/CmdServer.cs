using Pathea;
using Pathea.AchievementNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Hooks
{
	public class CmdServer
	{
		HttpListener _listener;
		int _port = 10000;
		private static System.Threading.AutoResetEvent listenForNextRequest = new System.Threading.AutoResetEvent(false);

		public void Start()
		{
			if(_listener != null && _listener.IsListening)
			{
				return;
			}

			_listener = new HttpListener();
			if (!HookRegistry.IsWithinUnity())
			{
				_port += 1;
			}
			_listener.Prefixes.Add("http://127.0.0.1:" + _port.ToString() + "/");
			_listener.Start();
			IsRunning = true;

			//while (true)
			//{
			//	try
			//	{
			//		HttpListenerContext context = _listener.GetContext();
			//		Process(context);
			//	}
			//	catch (Exception ex)
			//	{
			//		HookRegistry.Panic(ex.Message);
			//	}
			//}
			//_listener.BeginGetContext(new AsyncCallback(GetContextCallBack), _listener);
			System.Threading.ThreadPool.QueueUserWorkItem(Listen);
		}

		public void Stop()
		{
			_listener.Stop();
			listenForNextRequest.Set();
			IsRunning = false;
		}
		public bool IsRunning { get; private set; }

		private void Process(HttpListenerContext context)
		{
			string filename = context.Request.Url.AbsolutePath;
			context.Response.StatusCode = (int)HttpStatusCode.NotFound;
			context.Response.OutputStream.Close();
		}
		void GetContextCallBack(IAsyncResult ar)
		{
			String res;
			HttpListenerContext context = _listener.EndGetContext(ar);
			try
			{
				HttpContentParser parser = new HttpContentParser(context.Request.InputStream);
				if (parser.Success)
				{
					res = DoCmd(parser.Parameters);
				}
				else
				{
					res = "error";
				}
			}
			catch(Exception ex) {
				res = ex.ToString();
				context.Response.StatusCode = 403;
			}
			//context.Response.ContentEncoding = Encoding.UTF8;
			context.Response.AddHeader("Content-Type", "text/html; charset=utf-8");
			context.Response.Close(Encoding.Default.GetBytes(res), false);
			listenForNextRequest.Set();
		}

		// Loop here to begin processing of new requests.
		private void Listen(object state)
		{
			while (_listener.IsListening)
			{
				_listener.BeginGetContext(new AsyncCallback(GetContextCallBack), _listener);
				listenForNextRequest.WaitOne();
			}
		}


		public string DoCmd(IDictionary<string, string> args)
		{
			switch (args["cmd"])
			{
				case "AddItem":
					string numStr;
					if(!args.TryGetValue("num",out numStr))
					{
						numStr = "1";
					}
					foreach (var idStr in args["ids"].Split(','))
					{
						int id = Convert.ToInt32(idStr.Trim());
						int num = Convert.ToInt32(numStr);
						if (HookRegistry.IsWithinUnity())
						{
							Module<Player>.Self.bag.AddItem(id, num, true, AddItemMode.Default);
							HookRegistry.Debug("AddItem {0}", id);
						}
						else
						{
							
						}
						
					}
					break;
				case "UnlockAchievement":
					{
						int id = Convert.ToInt32(args["id"].Trim());
						if (HookRegistry.IsWithinUnity())
						{
							if (Module<AchievementModule>.Self.IsAchievementUnlocked(id))
							{
								return String.Format("{0} is unlocked", id);
							}
							Module<AchievementModule>.Self.UnlockAchievement(id);
						}
					}
					break;
				case "Test":
					return "test";
			}
			return "success";
		}
	}
}
