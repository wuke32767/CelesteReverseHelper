local ui = require("ui")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")

local booleanField = {}

booleanField.fieldType = "ReverseHelper.Padding"

booleanField._MT = {}
booleanField._MT.__index = {}

function booleanField._MT.__index:setValue(value)
    if value then
        self.currentValue = value
        self.currentHasValue = true
    else
        self.currentHasValue = false
    end
    
end

function booleanField._MT.__index:getValue()
    return self.currentValue
end
function booleanField._MT.__index:hasValue()
    return self.currentHasValue
end

function booleanField._MT.__index:fieldValid()
    return type(self:getValue()) == "boolean" or self:getValue() == ""
end

local function fieldChanged(formField)
    return function(element, new)
        --local _ = LuaPanda and LuaPanda.BP()
        local old = formField.currentValue
        if formField.checkbox and formField.checkbox.____has then
            formField.currentValue = formField.checkbox:getValue()
        else
            formField.currentValue = ""
        end

        if formField.currentValue ~= old then
            formField:notifyFieldChanged()
        end
    end
end
local function getCheckBox(name, value, options, selfx)
    --local _ = LuaPanda and LuaPanda.BP()
    
    local checkbox = uiElements.label(" ")

    return checkbox
end
function booleanField.getElement(name, value, options)
    local formField = {}

    local minWidth = options.minWidth or options.width or 160
    local maxWidth = options.maxWidth or options.width or 160

    local checkbox = getCheckBox(name, value, options, formField)
    local element = checkbox


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
    formField.width = 1
    formField.elements = {
        checkbox
    }

    return setmetatable(formField, booleanField._MT)
end

return booleanField