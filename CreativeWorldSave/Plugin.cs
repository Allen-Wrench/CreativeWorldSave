using System;
using Sandbox.Game.World;
using Sandbox.Game.GameSystems.Chat;
using VRage.Plugins;
using VRage.Game.ModAPI;
using System.Reflection;
using VRage.Utils;
using Sandbox.ModAPI;
using HarmonyLib;
using Sandbox.Game.Localization;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;
using Sandbox.Graphics;
using Sandbox.Game.Gui;

namespace CreativeWorldSave
{
	public class CreativeSavePlugin : IPlugin, IDisposable
	{
		public void Init(object gameInstance)
		{
			new Harmony("CreativeSave").Patch(AccessTools.Method("Sandbox.Engine.Networking.MyGameService:OpenOverlayUser"), new HarmonyMethod(typeof(CreativeSavePlugin), "ButtonClick"));
			//MySession.OnLoading += OnLoading;
		}

		public void Update()
		{
		}

		public void Dispose()
		{
			//MySession.OnLoading -= OnLoading;
		}

		public static bool ButtonClick()
		{
			//MyGuiScreenDialogText myGuiScreenDialogText = new MyGuiScreenDialogText(string.Empty, MyStringId.GetOrCompute("Input filename for new save:"), false);
			//myGuiScreenDialogText.OnConfirmed += delegate (string argument)
			//{
				MyLog.Default.Info("Saving creative world...");
				MyAPIGateway.Utilities.ShowMessage("Creative World Save", "Constructing offline creative world...");
				SaveConstructor.SaveWorldAsync(MySession.Static.Name);
			//};
			//MyScreenManager.GetFirstScreenOfType<MyGuiScreenPlayers>().CloseScreen();
			//MyGuiSandbox.AddScreen(myGuiScreenDialogText);
			return false;
		}

		//public void OnLoading() 
		//{
		//	MySession.Static.OnReady += AddCommand;
		//	MySession.OnLoading -= OnLoading;
		//}

		//public void AddCommand()
		//{
		//	if (!MySession.Static.ChatSystem.CommandSystem.ChatCommands.ContainsKey("/creativesave"))
		//	{
		//		MySession.Static.ChatSystem.CommandSystem.ScanAssemblyForCommands(Assembly.GetExecutingAssembly());
		//		MyLog.Default.Info("dude's Creative World Save plugin. Chat command added: /creativesave");
		//	}
		//}

		//[ChatCommand("/creativesave", "creativesave [world_name]", "(Arguments: string used to name the new save) Constructs a new save file using the current sessions mods and settings.", MyPromoteLevel.None)]
		//public static void CreativeSave(string[] args)
		//{
		//	if (args == null || args.Length != 1)
		//	{
		//		MyAPIGateway.Utilities.ShowMessage("Creative World Save", "Specify a name to create a new save containing the mods and settings of the current session.");
		//		return;
		//	}

		//	MyLog.Default.Info("Saving creative world...");
		//	MyAPIGateway.Utilities.ShowMessage("Creative World Save", "Constructing offline creative world...");
		//	SaveConstructor.SaveWorldAsync(args[0]);
		//}
	}
}
