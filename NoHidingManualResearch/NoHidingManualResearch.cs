//using System;
//using System.Text;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NoHidingManualResearchMod
{
    [BepInPlugin(__GUID__, __NAME__, "1.0.0")]
    public class NoHidingManualResearch : BaseUnityPlugin
    {
        public const string __NAME__ = "NoHidingManualResearch";
        public const string __GUID__ = "com.hetima.dsp." + __NAME__;

        new internal static ManualLogSource Logger;

        public static ConfigEntry<float> PanelScale;

        void Awake()
        {
            Logger = base.Logger;
            //Logger.LogInfo("Awake");

            PanelScale = Config.Bind("UI", "PanelScale", 1f,
                new ConfigDescription("Display Scale of Manual Research Panel (0.75 to 1.0)", new AcceptableValueRange<float>(0.75f, 1f)));

            new Harmony(__GUID__).PatchAll(typeof(Patch));
        }

        public static void Log(string str)
        {
            Logger.LogInfo(str);
        }

        //UIMechaLab DetermineVisible
        //GameHistoryData.autoManageLabItems == false だと閉じる
        //UIMechaLab _OnUpdate
        //GameHistoryData.autoManageLabItems == false だと項目追加されない
        //UIMechaLabItem Refresh
        //GameHistoryData.autoManageLabItems == false だと自身を Destroy()


        //UI Root/Overlay Canvas/In Game/Fullscreen UIs/Tech Tree/research-queue
        //UI Root/Overlay Canvas/In Game/Research Queue

        public static bool AutoManageLabItems(GameHistoryData history)
        {
            return true;
        }

        public static void SetPanelScale(float val)
        {
            if (val > 1f || val <= 0.1f)
            {
                val = 1f;
            }
            else if(val < 0.75f)
            {
                val = 0.75f;
            }

            Transform transform = UIRoot.instance.uiGame.mechaLab?.transform;
            if (transform != null)
            {
                transform.localScale = new Vector3(val, val, val);
            }
        }


        static class Patch
        {
            internal static bool _initialized = false;

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnCreate")]
            public static void UIGame__OnCreate_Postfix()
            {
                if (!_initialized)
                {
                    _initialized = true;

                    PanelScale.SettingChanged += (sender, args) => {
                        SetPanelScale(PanelScale.Value);
                    };
                    SetPanelScale(PanelScale.Value);
                }
            }

            [HarmonyTranspiler, HarmonyPatch(typeof(UIMechaLab), "DetermineVisible")]
            public static IEnumerable<CodeInstruction> UIMechaLab_DetermineVisible_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                //IL_002b: ldloc.0      // history
                //IL_002c: ldfld        bool GameHistoryData::autoManageLabItems
                //IL_0031: brtrue.s IL_0047

                //GameHistoryData public bool autoManageLabItems;
                FieldInfo f = AccessTools.Field(typeof(GameHistoryData), nameof(GameHistoryData.autoManageLabItems));
                MethodInfo rep = typeof(NoHidingManualResearch).GetMethod(nameof(NoHidingManualResearch.AutoManageLabItems));

                //List<CodeInstruction> ins = instructions.ToList();
                bool patched = false;
                foreach (var ins in instructions)
                {
                    if (!patched && ins.opcode == OpCodes.Ldfld && ins.operand is FieldInfo o && o == f)
                    {
                        ins.opcode = OpCodes.Call;
                        ins.operand = rep;
                        patched = true;
                    }
                    yield return ins;
                }

            }

            [HarmonyTranspiler, HarmonyPatch(typeof(UIMechaLab), "_OnUpdate")]
            public static IEnumerable<CodeInstruction> UIMechaLab__OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                //GameHistoryData public bool autoManageLabItems;
                FieldInfo f = AccessTools.Field(typeof(GameHistoryData), nameof(GameHistoryData.autoManageLabItems));
                MethodInfo rep = typeof(NoHidingManualResearch).GetMethod(nameof(NoHidingManualResearch.AutoManageLabItems));

                bool patched = false;
                foreach (var ins in instructions)
                {
                    if (!patched && ins.opcode == OpCodes.Ldfld && ins.operand is FieldInfo o && o == f)
                    {
                        ins.opcode = OpCodes.Call;
                        ins.operand = rep;
                        patched = true;
                    }
                    yield return ins;
                }
            }

            [HarmonyTranspiler, HarmonyPatch(typeof(UIMechaLabItem), "Refresh")]
            public static IEnumerable<CodeInstruction> UIMechaLabItem_Refresh_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                //GameHistoryData public bool autoManageLabItems;
                FieldInfo f = AccessTools.Field(typeof(GameHistoryData), nameof(GameHistoryData.autoManageLabItems));
                MethodInfo rep = typeof(NoHidingManualResearch).GetMethod(nameof(NoHidingManualResearch.AutoManageLabItems));

                bool patched = false;
                foreach (var ins in instructions)
                {
                    if (!patched && ins.opcode == OpCodes.Ldfld && ins.operand is FieldInfo o && o == f)
                    {
                        ins.opcode = OpCodes.Call;
                        ins.operand = rep;
                        patched = true;
                    }
                    yield return ins;
                }
            }

        }

    }
}
