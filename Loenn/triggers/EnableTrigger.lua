local OnlyStrawberryCollectTrigger = {}
local optionsof={ "MoveAway", "Disable", "Shrink","Collidable", }
OnlyStrawberryCollectTrigger.nodeVisibility="always"
OnlyStrawberryCollectTrigger.name = "ReverseHelper/EnableTrigger"
OnlyStrawberryCollectTrigger.nodeLimits={1,-1}
OnlyStrawberryCollectTrigger.nodeLineRenderType = "fan"
OnlyStrawberryCollectTrigger.placements = {
    {
        name = "EnableTrigger",
        data = {
            reversed = false,
            revertOnExit = false,
            toggleSwitchMode = false,
            mode = "Collidable",
            oneUse = false,
        }
    },
    {
        name = "DisableTrigger",
        data = {
            reversed = true,
            revertOnExit = false,
            toggleSwitchMode = false,
            mode = "Collidable",
            oneUse = false,
        }
    },
}
OnlyStrawberryCollectTrigger.fieldInformation=
{
    mode = {
        options = optionsof
    },
}
function OnlyStrawberryCollectTrigger.triggerText(room, trigger)
    local a = ""
    if trigger.reversed then
        a = "Disable"
    else
        a = "Enable"
    end
    if trigger.toggleSwitchMode then
        a = a .. " (Toggle)"
    end
    if trigger.mode == "MoveAway" then
        a = a .. " (Move)"
    end
    if trigger.mode == "Collidable" then
        a = a .. " (Collidable)"
    end
    if trigger.mode == "Shrink" then
        a = a .. " (Shrink)"
    end
    return a
end
return OnlyStrawberryCollectTrigger
