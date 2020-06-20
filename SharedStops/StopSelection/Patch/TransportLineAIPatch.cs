﻿using ColossalFramework;
using HarmonyLib;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SharedStopEnabler.RedirectionFramework.Attributes;
using UnityEngine;

namespace SharedStopEnabler.StopSelection.Patch
{
	[TargetType(typeof(RoadAI))]
	class RoadAIDetour : RoadAI
	{
		[RedirectMethod]
		public override void UpdateSegmentFlags(ushort segmentID, ref NetSegment data)
		{
			// calls base.UpdateSegmentFlags(segmentID, ref data);
			//object[] args = new object[] { segmentID, data };
			//object result = typeof(RoadBaseAI).GetMethod("UpdateSegmentFlags", new[] { typeof(ushort), typeof(NetSegment).MakeByRefType() }).Invoke(this, args);
			//if ((bool)result)
			//	data = (NetSegment)args[1];
			try
			{
				MethodInfo method = typeof(RoadBaseAI).GetMethod("UpdateSegmentFlags", new[] { typeof(ushort), typeof(NetSegment).MakeByRefType() });			
				IntPtr ptr = method.MethodHandle.GetFunctionPointer();
				Action<ushort, NetSegment> baseUpdate = (Action<ushort, NetSegment>)Activator.CreateInstance(typeof(Action<ushort, NetSegment>), this, ptr);
				baseUpdate(segmentID, data);
			}
			catch (Exception e)
			{
				Log.Info($"UpdatesegmentFlags error {e}");
				Log.Debug($"UpdatesegmentFlags error {e}");
			}
			var oldflags = data.m_flags;
			NetSegment.Flags flags = data.m_flags & ~(NetSegment.Flags.StopRight | NetSegment.Flags.StopLeft | NetSegment.Flags.StopRight2 | NetSegment.Flags.StopLeft2);
			if (m_info.m_lanes != null)
			{
				NetManager instance = Singleton<NetManager>.instance;
				bool flag = (data.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
				uint num = instance.m_segments.m_buffer[(int)segmentID].m_lanes;
				int num2 = 0;
				while (num2 < m_info.m_lanes.Length && num != 0U)
				{
					NetLane.Flags flags2 = (NetLane.Flags)instance.m_lanes.m_buffer[(int)((UIntPtr)num)].m_flags;
					if ((flags2 & NetLane.Flags.Stop) != NetLane.Flags.None)
					{
						if (m_info.m_lanes[num2].m_position < 0f != flag)
						{
							flags |= NetSegment.Flags.StopLeft;
						}
						else
						{
							flags |= NetSegment.Flags.StopRight;
						}
					}
					else if ((flags2 & NetLane.Flags.Stop2) != NetLane.Flags.None)
					{
						if (m_info.m_lanes[num2].m_position < 0f != flag)
						{
							flags |= NetSegment.Flags.StopLeft2;
						}
						else
						{
							flags |= NetSegment.Flags.StopRight2;
						}
					}
					num = instance.m_lanes.m_buffer[(int)((UIntPtr)num)].m_nextLane;
					num2++;
				}
			}
			if (oldflags != flags && data.IsSharedStopSegment((int)segmentID))
			{
				var index = Singleton<SharedStopsTool>.instance.sharedStopSegments.FindIndex(s => s.m_segment == segmentID);
				var inverted = (flags & NetSegment.Flags.Invert) == NetSegment.Flags.Invert;
				Singleton<SharedStopsTool>.instance.sharedStopSegments[index].UpdateStopFlags(inverted, out NetSegment.Flags stopflags);
				//flags &= ~(NetSegment.Flags.StopRight | NetSegment.Flags.StopLeft | NetSegment.Flags.StopRight2 | NetSegment.Flags.StopLeft2);
				data.m_flags = flags;
				data.m_flags |= stopflags;
				Log.Debug($"oldflags {oldflags} flags {flags} newflagsSharedStop {data.m_flags}");
				return;
			}
			Log.Debug($"oldflags {oldflags} newflags {flags} on segment {segmentID}");
			data.m_flags = flags;
			//TODO check if stop flags were added 
		}
	}

	[HarmonyPatch(typeof(TransportLineAI), "GetStopLane")]
	class TransportLineAIPatch1
	{
		static bool Prefix(ref bool __result, ref PathUnit.Position pos, VehicleInfo.VehicleType vehicleType)
		{
			if (pos.m_segment != 0)
			{
				if (vehicleType == VehicleInfo.VehicleType.None)
				{
					return true;
				}
				Log.Debug($"getstoplane {pos.m_segment}, {pos.m_lane}, {pos.m_offset}");
				NetManager instance = Singleton<NetManager>.instance;
				if (instance.m_segments.m_buffer[(int)pos.m_segment].GetClosestLane((int)pos.m_lane, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, vehicleType, out int num, out uint num2))
				{
					pos.m_lane = (byte)num;
					Log.Debug($"get new stoplane {pos.m_segment}, {pos.m_lane}, {pos.m_offset}");
					__result = true;
					return false;
				}
			}
			pos = default;
			__result = false;
			return false;
		}
	}
}
