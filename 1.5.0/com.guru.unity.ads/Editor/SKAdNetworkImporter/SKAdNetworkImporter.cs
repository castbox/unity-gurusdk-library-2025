#if UNITY_IOS
namespace Guru.Editor
{
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using UnityEditor.iOS.Xcode;
	using UnityEngine;
	using System;
	using Newtonsoft.Json;

	
	// -------------------- SKAdNetwork Importer --------------------	
	// 更新版本日志： 2024-10-31
	// 需求链接： https://www.tapd.cn/58098289/prong/stories/view/1158098289001022572
	// -------------------- SKAdNetwork Importer --------------------
	
	// TODO 在今后的版本中改为每次打包的时候从线上拉取配置
	
	public static class SKAdNetworkImporter
	{
		// 预制的 id 列表。目前默认为空
		private static readonly List<string> defaultNetworkIdentifiers = new List<string>();
		
		private static readonly string DEFAULT_ROOT_PATH = $"{PACKAGE_NAME}/Editor/SKAdNetworkImporter";
		private const string SKADNETWORK_PLIST_NAME = "SKAdNetwork.plist";
		private const string PACKAGE_NAME = "com.guru.unity.buildtool";
		public const string K_SKADNETWORK_IDENTIFIER = "SKAdNetworkIdentifier";
		public const string K_SKADNETWORK_ITEMS = "SKAdNetworkItems";

		
		

		[PostProcessBuild(10)]
		private static void OnPostProcessBuild(BuildTarget buildTarget, string path)
		{
			if (buildTarget != BuildTarget.iOS)
			{
				return;
			}
			
			var plistPath = Path.Combine(path, "Info.plist");
			var plist = new PlistDocument();
			plist.ReadFromFile(plistPath);
			
			//设置SKAdNetworkItems
			ReadSKADNetworkPlistFile();
			var plistElementArray = plist.root.CreateArray(K_SKADNETWORK_ITEMS);
			AddPlatformADNetworkIdentifier(plistElementArray, defaultNetworkIdentifiers);
			plist.WriteToFile(plistPath);
		}

		
		public static void ReadSKADNetworkPlistFile()
		{
			string plistPath = GetSKAdNetworkPlistPath();
			if (File.Exists(plistPath))
			{
				var plist = new PlistDocument();
				plist.ReadFromFile(plistPath);
				var skADNetworksArr = plist.root[K_SKADNETWORK_ITEMS].AsArray();
				if (skADNetworksArr != null)
				{
					foreach (var plistElement in skADNetworksArr.values)
					{
						var adNetworkValue = plistElement.AsDict()[K_SKADNETWORK_IDENTIFIER].AsString();
						if(!defaultNetworkIdentifiers.Contains(adNetworkValue))
							defaultNetworkIdentifiers.Add(adNetworkValue);
					}
				}
			}
			else
			{
				Debug.Log($"[POST] --- Inject SKADNetwork Failed: {plistPath}");
			}
			
		}
		
		private static void AddPlatformADNetworkIdentifier(PlistElementArray plistElementArray, List<string> arrays)
		{
			foreach (var value in arrays)
			{
				PlistArrayAddDict(plistElementArray, value);
			}
		}
		
		private static void PlistArrayAddDict(PlistElementArray plistElementArray, string value)
		{
			plistElementArray.AddDict().SetString(K_SKADNETWORK_IDENTIFIER, value);
		}


		private static string GetToolRootDir()
		{
			var guids = AssetDatabase.FindAssets($"{nameof(SKAdNetworkImporter)} t:script");
			if (guids.Length > 0)
			{
				var path = Directory.GetParent(AssetDatabase.GUIDToAssetPath(guids[0]))?.FullName ?? "";
				if(!string.IsNullOrEmpty(path)) return path;
			}
			return $"{Application.dataPath.Replace("Assets", "Packages")}/{DEFAULT_ROOT_PATH}";
		}


		private static string GetSKAdNetworkPlistPath()
		{
			return Path.Combine(GetToolRootDir(), SKADNETWORK_PLIST_NAME);	
		}


		#region 更新功能


		public static void UpdateLatestSKAdNetworks()
		{
			string progressTitle = "Download SKAdNetworks Json...";
			
			var loader = new MaxAdNetworkDownloader();
			loader.StartDownload((success, msg) =>
			{
				EditorUtility.ClearProgressBar();
				if (success)
				{
					var res = SaveJsonToPlist(msg);
					if (res)
					{
						if(EditorUtility.DisplayDialog("Download Success", 
							"The SKNetwork json file has been successfully imported", 
							"Great!"))
						{
							EditorUtility.RevealInFinder(GetToolRootDir());
						}
					}
					else
					{
						ShowDownloadFailedDialog("The download Json file contains error. \nTry to re-download it.");
					}
				}
				else
				{
					ShowDownloadFailedDialog(msg);
				}
			}, p =>
			{
				EditorUtility.DisplayCancelableProgressBar(progressTitle, $"loading {p*100:F2}%", 0);
			});

			EditorUtility.DisplayCancelableProgressBar(progressTitle, "loading...", 0);
		}


		private static void ShowDownloadFailedDialog(string msg)
		{
			if (EditorUtility.DisplayDialog("Something is wrong", 
				    msg,
				    "Retry", "Cancel"))
			{
				UpdateLatestSKAdNetworks(); // 重试下载
			}
		}


		private static bool SaveJsonToPlist(string json)
		{
			if(string.IsNullOrEmpty(json)) return false;
			
			var doc = MaxSKAdNetworkIdsDoc.FromJson(json);
			if(doc == null) return false;

			var plistBuilder = new SKAdNetworkPlistBuilder();

			foreach (var info in doc.skadnetwork_ids)
			{
				plistBuilder.AddNetworkId(info.skadnetwork_id);
			}
			Debug.Log($"[SKAdNetworkImporter] --- Add Network Ids: <color=#88ff00>{doc.skadnetwork_ids.Count}</color>");
			plistBuilder.BuildAndSave(GetSKAdNetworkPlistPath());
			return true;
		}


		#endregion
		
		
	}


	internal class MaxSKAdNetworkIdsDoc
	{
		public string company_name;
		public List<MaxSKAdNetworkIdInfo> skadnetwork_ids;


		public static MaxSKAdNetworkIdsDoc FromJson(string json)
		{
			try
			{
				return JsonConvert.DeserializeObject<MaxSKAdNetworkIdsDoc>(json);
				
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}

			return null;
		}

	}
	
	internal class MaxSKAdNetworkIdInfo
	{
		public string skadnetwork_id;
	}

	internal class SKAdNetworkPlistBuilder
	{

		private readonly HashSet<string> _idList = new HashSet<string>(300);
		
		
		public SKAdNetworkPlistBuilder()
		{
	
		}

		public void AddNetworkId(string networkId)
		{
			_idList.Add(networkId);
		}
		
		public void BuildAndSave(string savePath)
		{
			var plist = new PlistDocument();

			var idsArray = plist.root.CreateArray(SKAdNetworkImporter.K_SKADNETWORK_ITEMS);

			foreach (var id in _idList)
			{
				idsArray.AddDict().SetString(SKAdNetworkImporter.K_SKADNETWORK_IDENTIFIER, id);
			}

			plist.WriteToFile(savePath);
		}
		
	}
}

#endif
