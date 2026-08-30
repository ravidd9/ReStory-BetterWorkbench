using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Restory.Data.Devices;
using Restory.Data.Devices.DeviceWorkTypes;
using Restory.Data.Localization;
using Restory.Gameplay.Devices;
using Restory.Gameplay.InteractiveObjects;
using Restory.Gameplay.TimeSystems;
using Restory.Gameplay.Tooltips;
using Restory.Gameplay.WorkOrders.EmailOrders;
using Restory.UI.Views.Tooltips;
using TMPro;

namespace ReStoryBetterWorkbench;

[HarmonyPatch]
internal static class PackageTooltipDetails
{
    private const string CreateTooltipName = "CreateTooltip";
    private const string MessageTextName = "messageText";
    private const string DeviceContainerName = "deviceContainer";

    private static readonly FieldInfo MessageText =
        AccessTools.Field(typeof(GUI_DeliveryBoxMainTooltip), MessageTextName);

    private static readonly string[] InjectedFieldNames =
    {
        "emailOrdersService", "localizationSystem", "gameCalendar"
    };

    internal static bool SelfCheck()
    {
        DateTime delivered = new DateTime(2000, 1, 1, 10, 0, 0);

        (string CaseName, DateTime Now, int ExpectedDay)[] cases =
        {
            ("minutes later", delivered.AddMinutes(30), 1),
            ("late evening", delivered.AddHours(13), 1),
            ("next morning, under 24h", delivered.AddHours(23), 1),
            ("next morning, past 24h", delivered.AddHours(25), 2),
            ("two days later", delivered.AddDays(2), 3)
        };

        bool isValid = true;

        foreach ((string caseName, DateTime now, int expectedDay) in cases)
        {
            int day = DayInWork(delivered, now);

            if (day == expectedDay)
                continue;

            Log.Error($"Self-check FAILED: {caseName} counted as day {day}, expected {expectedDay}.");
            isValid = false;
        }

        return isValid;
    }

    private static bool Prepare()
    {
        MethodInfo createTooltip = AccessTools.Method(typeof(InteractiveObjectsTooltipsService), CreateTooltipName);

        bool hasPatchTarget = createTooltip != null
                              && HasParameterNamed(createTooltip, DeviceContainerName);

        bool hasMessageField = MessageText != null;
        bool hasEveryInjectedField = HasEveryInjectedField();

        if (hasPatchTarget && hasMessageField && hasEveryInjectedField)
        {
            return true;
        }

        Log.Error("Package tooltip details are disabled: InteractiveObjectsTooltipsService.CreateTooltip, one of "
            + "the members it is patched with, or GUI_DeliveryBoxMainTooltip.messageText is missing, most likely "
            + "renamed by a game update. The rest of the mod is unaffected.");

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(InteractiveObjectsTooltipsService), CreateTooltipName)]
    private static void AppendOrderDetails(GUI_DeliveryBoxMainTooltip __result, DeviceContainer deviceContainer,
        EmailOrdersService ___emailOrdersService, LocalizationSystem ___localizationSystem,
        GameCalendar ___gameCalendar)
    {
        string details = Details(deviceContainer, ___emailOrdersService, ___localizationSystem, ___gameCalendar);

        if (details == null || __result == null)
            return;

        TMP_Text message = (TMP_Text)MessageText.GetValue(__result);

        if (message == null)
            return;

        bool hasGameMessage = message.gameObject.activeSelf && !string.IsNullOrEmpty(message.text);

        message.text = hasGameMessage ? $"{message.text}\n{details}" : details;
        message.gameObject.SetActive(true);
    }

    private static string Details(DeviceContainer deviceContainer, EmailOrdersService emailOrders,
        LocalizationSystem localization, GameCalendar calendar)
    {
        if (deviceContainer == null || localization == null)
            return null;

        IEnumerable<DeviceWorkType> workTypes = RequestedWorkTypes(deviceContainer);

        if (workTypes == null)
            return null;

        string services = $"{Strings.Services}: {ServicesLine(workTypes, deviceContainer, localization)}";

        if (calendar == null || emailOrders == null)
            return services;

        if (!emailOrders.TryToGetOrderForDeviceContainer(deviceContainer, out TrackedEmailOrder tracked))
            return services;

        bool isDeliveryDateUnset = tracked.Order.DeviceDeliveredToStoreDateTime == DateTime.MaxValue;

        if (isDeliveryDateUnset)
            return services;

        int day = DayInWork(tracked.Order.DeviceDeliveredToStoreDateTime, calendar.CurrentDateTime);

        return $"{services}\n{Strings.Days}: {day}/{tracked.Order.NumberDaysToComplete}";
    }

    private static IEnumerable<DeviceWorkType> RequestedWorkTypes(DeviceContainer deviceContainer)
    {
        if (deviceContainer.AdditionalProperties == null)
            return null;

        if (deviceContainer.AdditionalProperties
            .TryToGetProperty(out PartOfEmailOrderInteractiveObjectProperty emailOrder))
            return emailOrder.WorkTypes;

        if (deviceContainer.AdditionalProperties
            .TryToGetProperty(out PartOfWorkOrderInteractiveObjectProperty workOrder))
            return workOrder.WorkTypes;

        return null;
    }

    private static string ServicesLine(IEnumerable<DeviceWorkType> workTypes, DeviceContainer deviceContainer,
        LocalizationSystem localization)
    {
        List<string> serviceNames = new List<string>();

        foreach (var service in workTypes.GetUniqueWorkTypesAndTheirCompletionStatus(deviceContainer))
        {
            string translatedName = localization.GetTranslation(service.Key.LocalizationKey);
            bool isStillNeeded = !service.Value;

            serviceNames.Add(Emphasized(translatedName, isStillNeeded));
        }

        return string.Join(", ", serviceNames);
    }

    private static string Emphasized(string service, bool isStillNeeded)
    {
        if (isStillNeeded)
            return $"<b>{service}</b>";

        return service;
    }

    private static int DayInWork(DateTime deliveredToStore, DateTime now)
    {
        return (now - deliveredToStore).Days + 1;
    }

    private static bool HasParameterNamed(MethodInfo method, string parameterName)
    {
        foreach (ParameterInfo parameter in method.GetParameters())
        {
            if (parameter.Name == parameterName)
                return true;
        }

        return false;
    }

    private static bool HasEveryInjectedField()
    {
        foreach (string fieldName in InjectedFieldNames)
        {
            if (AccessTools.Field(typeof(InteractiveObjectsTooltipsService), fieldName) == null)
                return false;
        }

        return true;
    }
}
