local drawableSpriteStruct = require("structs.drawable_sprite")
local enums = require("consts.celeste_enums")
local utils = require("utils")

local spinnerConnectionDistanceSquared = 24 * 24
local dustEdgeColor = {1.0, 0.0, 0.0}

local defaultSpinnerColor = "blue"
local unknownSpinnerColor = "blue"
local spinnerColors = {
    "blue",
    "red",
    "purple",
    "core",
    "rainbow"
}
local colorOptions = {}

for _, color in ipairs(spinnerColors) do
    colorOptions[utils.titleCase(color)] = color
end

-- Doesn't have textures directly, handled by code
local customSpinnerColors = {
    core = "red",
    rainbow = "white"
}

local function setColor(target, color)
    local tableColor = utils.getColor(color)

    if tableColor then
        target.color = tableColor
    end

    return tableColor ~= nil
end


local spinner = {}

spinner.associatedMods =  {"ReverseHelper"} 
spinner.name = "ReverseHelper/SquareSpinner"
spinner.fieldInformation = {
    color = {
        fieldType = "color",
    },
    coreColor = {
        fieldType = "color",
    }
}
spinner.placements = {
    {
        name = "Square Spinner",
        data = {
            color = "63d4ff",
            coreColor="ff9eb0",
            --dust = true,
            coreMode=false,
            rainbow=false,
            attachToSolid = false,
            fgTexture="objects/ReverseHelper/SquareSpinner/fg_neon",
            bgTexture="objects/ReverseHelper/SquareSpinner/bg_neon",
        }
    }
}

--for _, color in ipairs(spinnerColors) do
--    table.insert(spinner.placements, {
--        name = color,
--        data = {
--            color = color,
--            dust = false,
--            attachToSolid = false
--        }
--    })
--end

local function getSpinnerTexture(entity, foreground)
    local prefix = (foreground or foreground == nil) and "fg_" or "bg_"

    return "objects/ReverseHelper/SquareSpinner/" .. prefix .. "neon00"
end

local function getSpinnerSprite(entity, foreground)
    -- Prevent color from spinner to tint the drawable sprite
    local color = entity.color
    local position = {
        x = entity.x,
        y = entity.y
    }
    if entity.coreMode then
        color = "red"
    end
    if entity.rainbow then
        color = "white"
    end

    local texture = getSpinnerTexture(entity, foreground)
    local sprite = drawableSpriteStruct.fromTexture(texture, position)
    setColor(sprite,color)
    -- Check if texture color exists, otherwise use default color
    -- Needed because Rainbow and Core colors doesn't have textures
    return sprite
end

local function getConnectionSprites(room, entity)
    -- TODO - This can create some overlaps, can be improved later

    local sprites = {}

    for _, target in ipairs(room.entities) do
        if target == entity then
            break
        end

        if entity._name == target._name and not target.dust and entity.attachToSolid == target.attachToSolid then
            if utils.distanceSquared(entity.x, entity.y, target.x, target.y) < spinnerConnectionDistanceSquared then
                local connectorData = {
                    x = math.floor((entity.x + target.x) / 2),
                    y = math.floor((entity.y + target.y) / 2),
                    color = entity.color
                }
                local sprite = getSpinnerSprite(connectorData, false)

                sprite.depth = -8499

                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

function spinner.depth(room, entity)
    return entity.dusty and -50 or -8500
end

function spinner.sprite(room, entity)
--    local dusty = entity.dust
--
--    if dusty then
--        local position = {
--            x = entity.x,
--            y = entity.y
--        }
--
--        local baseTexture = "danger/dustcreature/base00"
--        local baseOutlineTexture = "dust_creature_outlines/base00"
--        local baseSprite = drawableSpriteStruct.fromTexture(baseTexture, position)
--        local baseOutlineSprite = drawableSpriteStruct.fromInternalTexture(baseOutlineTexture, entity)
--
--        baseOutlineSprite:setColor(dustEdgeColor)
--
--        return {
--            baseOutlineSprite,
--            baseSprite
--        }
--
--    else
        local sprites = getConnectionSprites(room, entity)
        local mainSprite = getSpinnerSprite(entity)

        table.insert(sprites, mainSprite)

        return sprites
--    end
end

function spinner.selection(room, entity)


        return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
    
end

return spinner