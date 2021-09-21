using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Illusion;
using Studio;

namespace KKS_StudioDefaultData
{
	[BepInProcess("CharaStudio")]
	[BepInPlugin(nameof(KKS_StudioDefaultData), nameof(KKS_StudioDefaultData), VERSION)]
	public class KKS_StudioDefaultData : BaseUnityPlugin
	{
		public const string VERSION = "1.0.0";

		private static ManualLogSource logger;
		
		private void Awake()
		{
			logger = Logger;
			
			Harmony.CreateAndPatchAll(typeof(KKS_StudioDefaultData));
		}

		private static void getAllFiles(string path, string search, ref List<string> list)
		{
			Utils.File.GetAllFiles(path, search, ref list);

			if (path.Contains(DefaultData.Path)) // do not get DefaultData if we're already getting it
				return;

			var newPath = path.Replace(UserData.Path, "");
			newPath = DefaultData.Path + newPath;
			
			Utils.File.GetAllFiles(newPath, search, ref list);
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(CharaList), "InitFemaleList")]
		[HarmonyPatch(typeof(CharaList), "InitMaleList")]
		[HarmonyPatch(typeof(MPCharCtrl.CostumeInfo), "InitFileList")]
		private static IEnumerable<CodeInstruction> AddDefaultData_Patch(IEnumerable<CodeInstruction> instructions)
		{
			var il = instructions.ToList();
           
			var index = il.FindIndex(instruction => instruction.opcode == OpCodes.Call && (instruction.operand as MethodInfo)?.Name == "GetAllFiles");
			if (index <= 0)
			{
				logger.LogMessage("Failed transpiling 'AddDefaultData_Patch' 'GetAllFiles' index not found!");
				logger.LogWarning("Failed transpiling 'AddDefaultData_Patch' 'GetAllFiles' index not found!");
				return il;
			}
            
			il[index] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(KKS_StudioDefaultData), nameof(getAllFiles)));

			return il;
		}
	}
}