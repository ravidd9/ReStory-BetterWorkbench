using System.Collections.Generic;
using HarmonyLib;
using Restory.Data.Elements.Condition;
using Restory.Gameplay.Elements;
using Restory.Gameplay.Soldering;
using Restory.UI.Presenters.Notepad;
using UnityEngine;

namespace ReStoryBetterWorkbench;

[HarmonyPatch]
internal static class NotepadPartsSorter
{
    private const int BrokenRank = 0;
    private const int SolderingRank = 1;
    private const int CleaningRank = 2;
    private const int ReadyRank = 3;

    internal static bool SelfCheck()
    {
        (string CaseName, ElementConditionBase Condition,
            ScorchedCircuitProperty ScorchedCircuit, int ExpectedRank)[] cases =
        {
            ("broken", Condition<DamagedElementCondition>(), null, BrokenRank),
            ("burnt with exposed solder points",
                Condition<BurntElementCondition>(), ScorchedCircuit(SolderPointState.Burnt), SolderingRank),
            ("burnt with sooty solder points",
                Condition<BurntElementCondition>(), ScorchedCircuit(SolderPointState.Sooty), CleaningRank),
            ("dirty", Condition<DirtyElementCondition>(), null, CleaningRank),
            ("perfect", Condition<PerfectElementCondition>(), null, ReadyRank)
        };

        bool isValid = true;

        foreach ((string caseName, ElementConditionBase condition,
            ScorchedCircuitProperty scorchedCircuit, int expectedRank) in cases)
        {
            isValid &= Expect(caseName, condition, scorchedCircuit, expectedRank);
        }

        return isValid;
    }

    private static bool Prepare()
    {
        bool hasUpdateElementsMethod =
            AccessTools.Method(typeof(GUI_NotepadElementsPanel), "UpdateElements") != null;

        bool hasCachedElementsField =
            AccessTools.Field(typeof(GUI_NotepadElementsPanel), "cachedPlacedElements") != null;

        if (hasUpdateElementsMethod && hasCachedElementsField)
        {
            return true;
        }

        Log.Error("Notepad parts sorting is disabled: GUI_NotepadElementsPanel.UpdateElements or "
            + "cachedPlacedElements is missing, most likely renamed by a game update. "
            + "Bench organizing is unaffected.");

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GUI_NotepadElementsPanel), "UpdateElements")]
    private static void SortByRemainingWork(List<ElementBase> ___cachedPlacedElements)
    {
        List<ElementBase> sortedByWorkRank = new List<ElementBase>(___cachedPlacedElements.Count);

        for (int rank = BrokenRank; rank <= ReadyRank; rank++)
        {
            foreach (ElementBase part in ___cachedPlacedElements)
            {
                if (WorkRank(part.ConditionHandler.ElementData) != rank)
                    continue;

                sortedByWorkRank.Add(part);
            }
        }

        ___cachedPlacedElements.Clear();
        ___cachedPlacedElements.AddRange(sortedByWorkRank);
    }

    private static int WorkRank(ElementData elementData)
    {
        if (elementData.Condition is DamagedElementCondition)
            return BrokenRank;

        if (elementData.JustSolderingNeeded())
            return SolderingRank;

        if (elementData.Condition is DirtyElementCondition)
            return CleaningRank;

        return ReadyRank;
    }

    private static bool Expect(string caseName, ElementConditionBase condition,
        ScorchedCircuitProperty scorchedCircuit, int expectedRank)
    {
        var elementData = new ElementData { Condition = condition, AdditionalProperty = scorchedCircuit };
        int rank = WorkRank(elementData);

        Object.DestroyImmediate(condition);

        if (rank == expectedRank)
            return true;

        Log.Error($"Self-check FAILED: {caseName} ranked {rank}, expected {expectedRank}.");

        return false;
    }

    private static ElementConditionBase Condition<T>() where T : ElementConditionBase
    {
        return ScriptableObject.CreateInstance<T>();
    }

    private static ScorchedCircuitProperty ScorchedCircuit(SolderPointState state)
    {
        return new ScorchedCircuitProperty
        {
            BurntTraces = new List<BurntTraceData>
            {
                new BurntTraceData { SolderPoints = { new SolderPointData { State = state } } }
            }
        };
    }
}
