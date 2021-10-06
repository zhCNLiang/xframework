LuaUpdate = {}

local fixedUpdates = {}
local updates = {}
local lateUpdates = {}

local fixedUpdatesRemoved = {}
local updatesRemoved = {}
local lateUpdatesRemoved = {}

CS.LuaUpdate.Instance:AddFixedUpdate(function()
    for i=#fixedUpdates, 1, -1 do
        local fixedUpdate = fixedUpdates[i]
        if fixedUpdatesRemoved[fixedUpdate] then
            table.remove(fixedUpdates, i)
        end
    end
    fixedUpdatesRemoved = {}

    for _, fixedUpdate in ipairs(fixedUpdates) do
        fixedUpdate()
    end
end)

CS.LuaUpdate.Instance:AddUpdate(function()
    for i=#updates, 1, -1 do
        local update = updates[i]
        if updatesRemoved[update] then
            table.remove(updates, i)
        end
    end
    updatesRemoved = {}

    for _, update in ipairs(updates) do
        update()
    end
end)

CS.LuaUpdate.Instance:AddLateUpdate(function()
    for i=#lateUpdates, 1, -1 do
        local lateUpdate = lateUpdates[i]
        if lateUpdatesRemoved[lateUpdate] then
            table.remove(lateUpdate, i)
        end
    end
    lateUpdatesRemoved = {}

    for _, lateUpdate in ipairs(lateUpdates) do
        lateUpdate()
    end
end)

function LuaUpdate.AddFixedUpdate(fixedUpdate)
    table.insert(fixedUpdates, fixedUpdate)
end

function LuaUpdate.RemoveFixedUpdate(fixedUpdate)
    fixedUpdatesRemoved[fixedUpdate] = true
end

function LuaUpdate.AddUpdate(update)
    table.insert(updates, update)
end

function LuaUpdate.RemoveUpdate(update)
    updatesRemoved[update] = true
end

function LuaUpdate.AddLateUpdate(lateUpdate)
    table.insert(lateUpdates, lateUpdate)
end

function LuaUpdate.RemoveLateUpdate(lateUpdate)
    lateUpdatesRemoved[lateUpdate] = true
end