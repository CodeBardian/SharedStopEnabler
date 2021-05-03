using ColossalFramework;
using HarmonyLib;
using SharedStopEnabler.Util;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SharedStopEnabler.StopSelection.Patch
{
	[HarmonyPatch(typeof(RoadAI), "UpdateSegmentFlags")]
	class RoadAIPatch_UpdateSegmentFlags
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			bool foundJump = false;
			int index = -1;

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				if (foundJump) break;
				if (codes[i].opcode == OpCodes.Br)
				{

					for (int j = i + 1; j < codes.Count; j++)
					{
						if (codes[j].opcode == OpCodes.Br)
							break;

						object strOperand = codes[j].operand;
						if (strOperand != null && strOperand.ToString().Contains("65536"))
						{
							index = codes.FindIndex(j, c => c.opcode == OpCodes.Br);
							foundJump = true;
							break;
						}
					}
				}
			}

			if (index > -1)
			{
				codes[index].opcode = OpCodes.Nop;
			}
			else
			{
				Log.Info("Didn't patch UpdateSegmentFlags with transpiler...");
			}
			return codes.AsEnumerable();
		}
	}
}
