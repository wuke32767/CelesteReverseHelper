local ui = require("ui")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")

local booleanField = {}

booleanField.fieldType = "ReverseHelper.OptionalBool"

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
        local _ = LuaPanda and LuaPanda.BP()
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
        local _ = LuaPanda and LuaPanda.BP()
    local rv = value
    if value == "" then
        rv = false
    end
    
    local checkbox = uiElements.checkbox(options.displayName or name, rv, fieldChanged(selfx))

    if value ~= "" then
        checkbox.____has = true
    end
    
    local oget = checkbox.getValue
    local oset = checkbox.setValue
    local ock = checkbox.onClick
    local oupd = checkbox.updateIcon
    checkbox.getValue = function(self)
        return oget(self)
    end
    checkbox.setValue = function(self, value, has)
        if has == nil then
            self.____has = true
        else
            self.____has = has
        end 
        oset(self, value)
    end
    checkbox.onClick = function(self, x, y, button)
        ock(self, x, y, button)
        if button == 2 then
            self:setValue(self._value, not self.____has)
            if self.cb then
                self:cb(self.value)
            end
        end
    end
    checkbox.updateIcon = function(self)
        --local _ = LuaPanda and LuaPanda.BP()
        if self.____has then
            self.label.style.color = { 1,1,1,1 }
        else
            self.label.style.color = { 0.5,0.5,0.5,1 }
        end
        self.label:repaint()
        oupd(self)
    end
    if checkbox.____has then
        checkbox.label.style.color = { 1,1,1,1 }
    else
        checkbox.label.style.color = { 0.5,0.5,0.5,1 }
    end
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