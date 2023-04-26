using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.World;
using Sandbox.Game.GameSystems.Chat;
using VRage.Plugins;
using VRage.Game.ModAPI;
using System.Reflection;
using VRage.Utils;
using Sandbox.ModAPI;

namespace CreativeWorldSave
{
	public class Plugin : IPlugin, IDisposable
	{
		public void Init(object gameInstance)
		{
			MySession.OnLoading += OnLoading;
		}

		public void Update()
		{
		}

		public void Dispose()
		{
			MySession.OnLoading -= OnLoading;
		}

		public void OnLoading() 
		{
			MySession.Static.OnReady += AddCommand;
		}

		public void AddCommand()
		{
			MySession.Static.OnReady -= AddCommand;
			MySession.Static.ChatSystem.CommandSystem.ScanAssemblyForCommands(Assembly.GetExecutingAssembly());
			if (MySession.Static.ChatSystem.CommandSystem.ChatCommands.ContainsKey("/creativesave"))
			{
				MyLog.Default.Info("dude's Creative World Save plugin. Chat command added: /creativesave");
			}
		}

		[ChatCommand("/creativesave", "creativesave [world_name]", "(Arguments: string used to name the new save) Constructs a new save file using the current sessions mods and settings.", MyPromoteLevel.None)]
		public static void CreativeSave(string[] args)
		{
			if (args == null || args.Length != 1)
			{
				MyAPIGateway.Utilities.ShowMessage("Creative World Save", "Specify a name to create a new save containing the mods and settings of the current session.");
				return;
			}

			MyLog.Default.Info("Saving creative world...");
			MyAPIGateway.Utilities.ShowMessage("Creative World Save", "Constructing offline creative world...");
			SaveConstructor.SaveWorldAsync(args[0]);
		}
	}
}
