using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sandbox.Engine.Networking;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.ObjectBuilders.Components.BankingAndCurrency;
using VRage.Game.ObjectBuilders.Components;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using VRageRender;
using VRage.Game.Voxels;
using Sandbox;
using System.IO.Compression;
using System.Threading.Tasks;

namespace CreativeWorldSave
{
	public static class SaveConstructor
	{
		public static async void SaveWorldAsync(string saveName)
		{
			await Task.Run(() => Save(saveName)).ContinueWith((task) =>
			{
				string result = "";
				switch (task.Status)
				{
					case TaskStatus.RanToCompletion:
						result = $"Finished constructing world: {saveName}";
						break;
					case TaskStatus.Canceled:
						result = "Operation cancelled.";
						break;
					case TaskStatus.Faulted:
						result = "ERROR: " + task.Exception.Message;
						break;
				}
				MyLog.Default.Info(result);
				MyAPIGateway.Utilities.ShowMessage("Creative World Save", result);
			});
		}

		private static void Save(string saveName)
		{
			string path = MyLocalCache.GetSessionSavesPath(saveName, contentFolder: false);
			MySessionSnapshot snapshot = GetSnapshot(saveName, path);
			SaveSector(snapshot.SectorSnapshot, path, new Vector3I(0));
			SaveCheckpoint(snapshot.CheckpointSnapshot, path);
		}

		private static MySessionSnapshot GetSnapshot(string saveName, string saveFolder)
		{
			MySessionSnapshot snapshot = new MySessionSnapshot();
			snapshot.TargetDir = saveFolder;

			try
			{
				MyObjectBuilder_Checkpoint checkpoint = MySession.Static.GetCheckpoint(saveName);
				MyObjectBuilder_Gps gps;
				checkpoint.Gps.Dictionary.TryGetValue(MySession.Static.LocalPlayerId, out gps);
				checkpoint.Gps.Dictionary.Clear();
				checkpoint.Gps.Dictionary.Add(MySession.Static.LocalPlayerId, gps);
				checkpoint.Factions = new MyObjectBuilder_FactionCollection();
				checkpoint.Factions.Factions = new List<MyObjectBuilder_Faction>();
				checkpoint.Factions.Players = new VRage.Serialization.SerializableDictionary<long, long>(new Dictionary<long, long>());
				checkpoint.Factions.PlayerToFactionsVis = new List<MyObjectBuilder_FactionsVisEntry>();
				checkpoint.Factions.Relations = new List<MyObjectBuilder_FactionRelation>();
				checkpoint.Factions.RelationsWithPlayers = new List<MyObjectBuilder_PlayerFactionRelation>();
				checkpoint.Factions.Requests = new List<MyObjectBuilder_FactionRequests>();
				checkpoint.Identities.Clear();
				for (int i = checkpoint.SessionComponents.Count - 1; i >= 0; i--)
				{
					if (checkpoint.SessionComponents[i] is MyObjectBuilder_BankingSystem)
						(checkpoint.SessionComponents[i] as MyObjectBuilder_BankingSystem).Accounts.Clear();
					if (checkpoint.SessionComponents[i] is MyObjectBuilder_CoordinateSystem)
						(checkpoint.SessionComponents[i] as MyObjectBuilder_CoordinateSystem).CoordSystems.Clear();
				}
				checkpoint.SessionName = saveName;
				checkpoint.Settings.EnableSaving = true;
				checkpoint.Settings.OnlineMode = MyOnlineModeEnum.OFFLINE;
				checkpoint.Settings.GameMode = VRage.Library.Utils.MyGameModeEnum.Creative;
				snapshot.CheckpointSnapshot = checkpoint;
				snapshot.SectorSnapshot = MySession.Static.GetSector();
				Dictionary<string, IMyStorage> dictionary = new Dictionary<string, IMyStorage>();
				snapshot.CompressedVoxelSnapshots = MySession.Static.VoxelMaps.GetVoxelMapsData(true, true, null);
				snapshot.VoxelSnapshots = MySession.Static.VoxelMaps.GetVoxelMapsData(true, false, dictionary);
				snapshot.VoxelStorageNameCache = new Dictionary<string, IMyStorage>();
				foreach (var kvp in dictionary)
				{
					IMyStorage storage = kvp.Value;
					storage.DeleteRange(VRage.Voxels.MyStorageDataTypeFlags.None, new Vector3I(0), storage.Size, false);
					snapshot.VoxelStorageNameCache.Add(kvp.Key, storage);
				}

				foreach (KeyValuePair<string, byte[]> keyValuePair in snapshot.VoxelSnapshots)
				{
					SaveVoxelSnapshot(snapshot, keyValuePair.Key, keyValuePair.Value, true);
				}
				snapshot.VoxelSnapshots.Clear();
				snapshot.VoxelStorageNameCache.Clear();
				foreach (KeyValuePair<string, byte[]> keyValuePair2 in snapshot.CompressedVoxelSnapshots)
				{
					SaveVoxelSnapshot(snapshot, keyValuePair2.Key, keyValuePair2.Value, false);
				}
				snapshot.CompressedVoxelSnapshots.Clear();
			}
			catch { };

			return snapshot;
		}

		private static bool SaveWorldConfiguration(MyObjectBuilder_WorldConfiguration configuration, string sessionPath)
		{
			string text = Path.Combine(sessionPath, "Sandbox_config.sbc");
			return MyObjectBuilderSerializer.SerializeXML(text, false, configuration, out _);
		}

		private static bool SaveCheckpoint(MyObjectBuilder_Checkpoint checkpoint, string sessionPath)
		{
			string text = Path.Combine(sessionPath, "Sandbox.sbc");
			bool num = MyObjectBuilderSerializer.SerializeXML(text, false, checkpoint, out _);
			MyObjectBuilder_WorldConfiguration myObjectBuilder_WorldConfiguration = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_WorldConfiguration>();
			myObjectBuilder_WorldConfiguration.Settings = checkpoint.Settings;
			myObjectBuilder_WorldConfiguration.Mods = checkpoint.Mods;
			myObjectBuilder_WorldConfiguration.SessionName = checkpoint.SessionName;
			myObjectBuilder_WorldConfiguration.LastSaveTime = checkpoint.LastSaveTime;
			bool result = num & SaveWorldConfiguration(myObjectBuilder_WorldConfiguration, sessionPath);
			return result;
		}

		private static bool SaveSector(MyObjectBuilder_Sector sector, string sessionPath, Vector3I sectorPosition)
		{
			string sectorPath = Path.Combine(sessionPath, "SANDBOX_0_0_0_.sbs");
			for (int i = sector.SectorObjects.Count - 1; i >= 0; i--)
			{
				if (sector.SectorObjects[i] is MyObjectBuilder_CubeGrid ||
					sector.SectorObjects[i] is MyObjectBuilder_FloatingObject ||
					sector.SectorObjects[i] is MyObjectBuilder_ProxyAntenna || 
					(sector.SectorObjects[i] is MyObjectBuilder_Character && (sector.SectorObjects[i] as MyObjectBuilder_Character).OwningPlayerIdentityId != MySession.Static.LocalPlayerId))
					sector.SectorObjects.RemoveAtFast(i);
			}

			bool result = MyObjectBuilderSerializer.SerializeXML(sectorPath, false, sector, out _);
			string text = sectorPath + "B5";
			MyObjectBuilderSerializer.SerializePB(text, false, sector, out _);
			return result;
		}

		private static void SaveVoxelSnapshot(MySessionSnapshot snapshot, string storageName, byte[] snapshotData, bool compress)
		{
			string text = storageName + ".vx2";
			string text2 = Path.Combine(snapshot.TargetDir, text);
			try
			{
				if (compress)
				{
					using (MemoryStream memoryStream = new MemoryStream(16384))
					{
						using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
						{
							gzipStream.Write(snapshotData, 0, snapshotData.Length);
						}
						byte[] array = memoryStream.ToArray();
						File.WriteAllBytes(text2, array);
						if (snapshot.VoxelStorageNameCache != null)
						{
							IMyStorage myStorage = null;
							if (snapshot.VoxelStorageNameCache.TryGetValue(storageName, out myStorage) && !myStorage.Closed)
							{
								myStorage.SetDataCache(array, true);
							}
						}
					}
				}
				else
				{
					File.WriteAllBytes(text2, snapshotData);
				}
			}
			catch (Exception ex)
			{
				MySandboxGame.Log.WriteLine(string.Format("Failed to write voxel file '{0}'", text2));
				MySandboxGame.Log.WriteLine(ex);
			}
		}
	}
}
