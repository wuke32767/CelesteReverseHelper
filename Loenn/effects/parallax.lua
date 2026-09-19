local parallax = require('parallax')
local effect = {}
local function invokeOrAsis(f, ...)
    if type(f) == "function" then
        return f(...)
    end
    return f
end

effect.name = "ReverseHelper/YetAnotherHiresBackgroundParallax"

effect.associatedMods = function(...)
    local t = invokeOrAsis(parallax.associatedMods, ...) or {}
    table.insert(t, "ReverseHelper")
    return t
end

effect.canForeground = false
effect.defaultData = function(...)
    local h = table.shallowcopy(invokeOrAsis(parallax.defaultData, ...))
    -- h.scaleX = 1
    -- h.scaleY = 1
    h.zzdoc = ""
    return h
end

effect.fieldOrder = parallax.fieldOrder
effect.fieldInformation = function(...)
    local h = table.shallowcopy(invokeOrAsis(parallax.fieldInformation, ...))
    h.zzdoc = {
        fieldType = "ReverseHelper.DOCUMENT",
        important = true
    }
    return h
end

return effect
