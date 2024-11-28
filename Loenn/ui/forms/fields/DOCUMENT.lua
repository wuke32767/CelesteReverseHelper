local ui = require("ui")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")
local drawable = require("structs.drawable_sprite")

local booleanField = {}

booleanField.fieldType = "ReverseHelper.DOCUMENT"

booleanField._MT = {}
booleanField._MT.__index = {}

function booleanField._MT.__index:setValue(value)

end

function booleanField._MT.__index:getValue()
    return ""
end
function booleanField._MT.__index:hasValue()
    return self.currentHasValue
end

function booleanField._MT.__index:fieldValid()
    return true
end

function booleanField.getElement(name, value, options)
    local formField = {}

    local minWidth = options.minWidth or options.width or 160
    local maxWidth = options.maxWidth or options.width or 160

    local checkbox = uiElements.label(options.useLoennName and options.displayName or "README!")
    --local checkbox = uiElements.image()
    --checkbox.setImage(drawable.fromTexture("objects/ReverseHelper/Loenn/DOCUMENT"))
    local element = checkbox
    local _padding1 = uiElements.label("")
    local _padding2 = uiElements.label("")
    local _padding3 = uiElements.label("")

    if options.important then
        checkbox.style.color = { 1, 0.5, 0.5, 1 }
    end

    if options.tooltipText then
        checkbox.interactive = 1
        checkbox.tooltipText = options.tooltipText
    end

    checkbox.centerVertically = true

    formField.checkbox = checkbox
    formField.name = name
    formField.initialValue = value
    formField.currentValue = value
    formField.sortingPriority = 10
    formField.width = 4
    formField.elements = {
        checkbox,_padding1,_padding2,_padding3,
    }

    return setmetatable(formField, booleanField._MT)
end

return booleanField