local effect = {}

effect.name = "ReverseHelper/ThetaAndParalldoxsOnWorldlines/Sea"
effect.canBackground = true
effect.canForeground = false

local opt = {"Parallel", "Interference", "Emergence", "Entanglement", "Overflow"}
if require("mods").requireFromPlugin("libraries.private", "ReverseHelperPrivate") then
    table.insert(opt, 3, "Observation")
end

effect.fieldInformation = {
    chapter = {
        options = opt,
        editable = false
    }
}

effect.defaultData = {
    chapter = "Parallel",
    alpha = 1.0,
    hires = false,
    scrollX = 0,
    scrollY = 0,
    timeScale = 1,
}

return effect
