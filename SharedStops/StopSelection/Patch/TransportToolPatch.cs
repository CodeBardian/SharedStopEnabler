using HarmonyLib;
using SharedStopEnabler.Util;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SharedStopEnabler.StopSelection.Patch
{
    class TransportToolPatch_GetStopPosition
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool foundStopFlagAccess = false;
            int startIndex = -1;
            int endIndex = -1;

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (foundStopFlagAccess) break;
                if (codes[i].opcode == OpCodes.Call)
                {
                    object strOperand = codes[i].operand;
                    if (strOperand == null || !strOperand.ToString().Contains("GetClosestLanePosition"))
                        continue;

                    int stopFlagCount = 0;
                    startIndex = i + 2;
                    for (int j = startIndex + 1; j < codes.Count; j++)
                    {
                        if (codes[j].opcode == OpCodes.Ret)
							break;
                                
                        strOperand = codes[j].operand;
                        if (strOperand != null && strOperand.ToString().Contains("m_stopFlag"))
                        {
                            stopFlagCount++;
                        }
                        if (stopFlagCount == 2)
                        {                   
                            endIndex = codes.FindIndex(j, c => c.opcode == OpCodes.Ret);
                            foundStopFlagAccess = true;
                            break;
                        }
                    }
                }
            }

            if (startIndex > -1 && endIndex > -1)
            {
                codes.RemoveRange(startIndex, endIndex - startIndex + 1);
            }
            else
            {
                Log.Info("Didn't patch GetStopPosition with transpiler...");
            }
            return codes.AsEnumerable();
        }
    }
}
