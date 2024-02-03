-- Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
-- See the LICENCE file in the repository root for full licence text.

--yes, this is copied from GravityHelper directly.


local drawableSpriteStruct = require("structs.drawable_sprite")
local utils = require("utils")
local enums = require "consts.celeste_enums"

-- For future use I guess?
local textures = {"wood", "dream", "temple", "templeB", "cliffside", "reflection", "core", "moon"}
local overrider = {"Rebound","NormalCollision","NormalOverride","Bounce","Ignore","DoNotOverride",}

local function getTexture(entity)
    return entity.texture and entity.texture ~= "default" and entity.texture or "wood"
end

local upsideDownJumpThru = {
    name = "GravityHelper/ReverseHelper/DashConductionJumpThruUpsideDown",
    depth = -9000,
    canResize = {true, false},
    placements = {},
    fieldInformation=
    {
        texture = {
            options = textures
        },
        surfaceIndex = {
            options = enums.tileset_sound_ids,
            fieldType = "integer"
        },
    overrideCollisionResult = {
        options = overrider

    }

    }
}
upsideDownJumpThru.associatedMods = { "ReverseHelper","GravityHelper", }

upsideDownJumpThru.placements = {
    {
        name = "Dash Conduction JumpThru UpsideDown",
        data =  {
            width = 8,
            texture = "wood",
            triggerStaticMovers = true,
            overrideCollisionResult="DoNotOverride",
        },
    }
}

function upsideDownJumpThru.sprite(room, entity)
    local textureRaw = getTexture(entity)
    local texture = "objects/jumpthru/" .. textureRaw

    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 8

    local startX, startY = math.floor(x / 8) + 1, math.floor(y / 8) + 1
    local stopX = startX + math.floor(width / 8) - 1
    local len = stopX - startX

    local sprites = {}

    for i = 0, len do
        local quadX = 8
        local quadY = 8

        if i == 0 then
            quadX = 0
            quadY = room.tilesFg.matrix:get(startX - 1, startY, "0") ~= "0" and 0 or 8

        elseif i == len then
            quadY = room.tilesFg.matrix:get(stopX + 1, startY, "0") ~= "0" and 0 or 8
            quadX = 16
        end

        local sprite = drawableSpriteStruct.fromTexture(texture, entity)

        sprite:setJustification(0, 0)
        sprite:setScale(1, -1)
        sprite:addPosition(i * 8, 8)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)

        table.insert(sprites, sprite)
    end

    return sprites
end

function upsideDownJumpThru.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width, 8)
end

return upsideDownJumpThru