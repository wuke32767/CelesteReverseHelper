local OnlyStrawberryCollectTrigger = {}
OnlyStrawberryCollectTrigger.nodeVisibility="always"
OnlyStrawberryCollectTrigger.name = "ReverseHelper/EnableTrigger"
OnlyStrawberryCollectTrigger.nodeLimits={1,-1}
OnlyStrawberryCollectTrigger.placements = {
    {
        name = "EnableTrigger",
        data = {
            reversed = false,
            revertOnExit = false,
            toggleSwitchMode = false,
            oneUse = false,
        }
    },
    {
        name = "DisableTrigger",
        data = {
            reversed = true,
            revertOnExit = false,
            toggleSwitchMode = false,
            oneUse = false,
        }
    },
}
function OnlyStrawberryCollectTrigger.triggerText(room, trigger)
    local a
    if trigger.reversed then
        a = "Disable"
    else
        a = "Enable"
    end
    if trigger.toggleSwitchMode then
        return a .. " (Toggle)"
    end
    return a
end
return OnlyStrawberryCollectTrigger
