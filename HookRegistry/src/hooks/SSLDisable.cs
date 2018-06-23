// This hook causes Hearthstone to communicate in plaintext, without using TLS/SSL.
// NOTE: This is designed for use with 3rd party servers that don't support TLS connections
// and will cause the Hearthstone client to fail to connect to an official server, therefore
// it is disabled by default.
// To enable this hook, add "BattleNetCSharp::Init" to example_hooks


using Pathea.ActorNs;
using Pathea.EG;
using Pathea.FavorSystemNs;
using Pathea.ModuleNs;
using System;
using System.Reflection;
using System.Threading;

namespace Hooks
{
	[RuntimeHook]
	class SSLDisable
	{
		private const string HOOK_FAILED = "[SSLDisable] Failed to initialise the game! {0}";

		// This variable is used to control the interception of the hooked method.
		// When TRUE, we return null to allow normal execution of the function.
		// When FALSE, we hook into the call.
		// This switch allows us to call the original method from within this hook class.
		private bool reentrant;

		public SSLDisable()
		{
			HookRegistry.Register(OnCall);
			reentrant = false;
		}


		// Returns a list of methods (full names) this hook expects.
		// The Hooker will cross reference all returned methods with the methods it tries to hook.
		public static string[] GetExpectedMethods()
		{
			return new string[] {
				"UILoader::OnInit",
				"UILoader::OnRelease",
				"Pathea.RiderNs.RidableModuleManager::TryDecreaseLoyalty",
				"Pathea.EG.EGMgr::StartEngagement",
				"Pathea.EG.EGMgr::StopEngagement",
				"Pathea.FavorSystemNs.FavorUtility::GetFavorBehaviorInfo",
			};
		}

		object OnCall(string typeName, string methodName, object thisObj, object[] args, IntPtr[] refArgs, int[] refIdxMatch)
		{
			switch (typeName + "::" + methodName)
			{
				case "UILoader::OnInit":
					HookRegistry.Get().CtrlServer.Start();
					return null;
				case "UILoader::OnRelease":
					HookRegistry.Get().CtrlServer.Stop();
					return null;
				case "Pathea.RiderNs.RidableModuleManager::TryDecreaseLoyalty": //忠诚不减
					return false;
				case "Pathea.EG.EGMgr::StartEngagement":// 约会/玩耍直接满
					{
						var egMgr = (EGMgr)thisObj;
						if (egMgr.IsEngagement())
						{
							var th = new Thread(()=> {
								HookRegistry.Debug("[StartEngagement] wait 3 second");
								Thread.Sleep(3000);
								var mDate = (EGDate)HookRegistry.GetInstanceField(typeof(EGMgr), thisObj, "mDate");
								mDate.SetMood(100);
								HookRegistry.Debug("[StartEngagement] set mood {0}", mDate.GetMood());
							});
							th.Start();
						}
					}
					return null;
				case "Pathea.EG.EGMgr::StopEngagement": //取消嫉妒
					if ((EGStopType)args[0] == EGStopType.Jealous)
					{
						return false;
					}
					else
					{
						return null;
					}
				case "Pathea.FavorSystemNs.FavorUtility::GetFavorBehaviorInfo": //玫瑰花1颗心
					if ((int)args[1] == 7000016)
					{
						return new GiveGiftResult("75", 100, FeeLevelEnum.Excellent, GiftType.Normal);
					} else
					{
						return null;
					}
				default:
					return null;
			}
			

			try
			{


			}
			catch (Exception e)
			{
				// Write meaningful information to the game output.
				string message = String.Format(HOOK_FAILED, e.Message);
				HookRegistry.Panic(message);
			}

			// Never return null after typeName check!
			return (object)true;
		}
		void GainItems(object thisObj, object[] args)
		{
			if(args.Length > 1)
			{
				if (args[1] != null)
				{
					Pathea.ItemSystem.ItemObject[] items = (Pathea.ItemSystem.ItemObject[])args[1];
					foreach (var item in items)
					{
						HookRegistry.Debug("GainItems item {0} id={1} ", item.ItemBase.Name, item.ItemBase.ID, item.Number);
						HookRegistry.Debug("GainItems args {0}", UnityEngine.JsonUtility.ToJson(item));
					}
				}
			}
		}
	}


}
