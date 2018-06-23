using Pathea;
using Pathea.AchievementNs;
using Pathea.ItemSystem;
using Pathea.MessageSystem;
using Pathea.Missions;
using Pathea.ModuleNs;
using Pathea.StoreNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace Hooks
{
	public class CmdServer
	{
		HttpListener _listener;
		int _port = 10000;
		private static System.Threading.AutoResetEvent listenForNextRequest = new System.Threading.AutoResetEvent(false);

		public void Start()
		{
			if (_listener != null && _listener.IsListening)
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
			catch (Exception ex)
			{
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
					if (!args.TryGetValue("num", out numStr))
					{
						numStr = "1";
					}
					foreach (var idStr in args["ids"].Split(','))
					{
						int id = Convert.ToInt32(idStr.Trim());
						int num = Convert.ToInt32(numStr);
						if (HookRegistry.IsWithinUnity())
						{
							HookRegistry.Debug("AddItem {0}", id);
							Module<Player>.Self.bag.AddItem(id, num, false, AddItemMode.ForceBag);
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
				case "MissionAll":
					{
						StringBuilder sb = new StringBuilder();
						foreach (MissionBaseInfo missionBaseInfo in MissionManager.allMissionBaseInfo.Values)
						{
							sb.Append(missionBaseInfo.InstanceID);
							sb.Append(", ");
							sb.Append(missionBaseInfo.MissionNO);
							sb.Append(", ");
							string name = TextMgr.GetOriginStr(missionBaseInfo.MissionNameId);
							sb.Append(name);
							sb.Append(", ");
							sb.AppendLine();
						}
						return sb.ToString();
					}
					
				case "TransportMap":
					{
						MessageManager.Instance.Dispatch("TransportMap", new object[] { }, DispatchType.IMME, 2f);
					}
					break;
				case "RefreshPriceIndex": //更新物价指数
					Module<StoreManagerV40>.Self.RefreshPriceIndex();
					break;
				case "ItemBar": //修改物品数量
					{
						int index= Convert.ToInt32(args["solt"].Trim())-1;
						int num = Convert.ToInt32(args["num"].Trim());
						var item = Module<Player>.Self.bag.itemBar.itemBarItems.ElementAt(index);
						if(item  != null)
						{
							num = Mathf.Clamp(num, 1, item.ItemBase.MaxNumber);
							item.ChangeNumber(num - item.Number);
							return String.Format("slot {0} {2}({1}) => {3}", index + 1, item.ItemDataId, item.ItemBase.Name, item.Number);
						}else
						{
							return "no item on bar";
						}
					}
				case "Test":
					return "test";
			}
			return "success";
		}
	}
}
