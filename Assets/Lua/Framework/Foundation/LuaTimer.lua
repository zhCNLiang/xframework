LuaTimer = {}

local timerUniqueId = 0
local tTimers = {}
local tRemove = {}
local tTimersInfo = {}

local function innerTimerCall(timer)
    local bOk, error = pcall(timer.callback)
    if bOk then
        local info = tTimersInfo[timer.Id]

        local accmulateLoop = info.accmulateLoop
        accmulateLoop = accmulateLoop + 1
        info.accmulateLoop = accmulateLoop
        if timer.loop > 0 and timer.loop <= accmulateLoop then
            bOk = false
            LuaTimer.RemoveTimer(timer.Id)
        end
    else
        Log.Error("[Lua Timer]Error:%s", error)
        LuaTimer.RemoveTimer(timer.Id)
    end

    return bOk
end

LuaUpdate.AddUpdate(function()
    local unScaledtime = Time.unscaledDeltaTime
    local scaledtime = Time.deltaTime

    for i=#tTimers, 1, -1 do
        local timer = tTimers[i]
        local bRemove = tRemove[timer.Id]
        if bRemove then
            table.remove(tTimers, i)
        end
    end

    for _, timer in ipairs(tTimers) do
        local info = tTimersInfo[timer.Id]
        local accmulateDelay = info.accmulateDelay
        local accmulateTime = info.accmulateTime
        local delayCalled = info.delayCalled

        local t = timer.unscaled and unScaledtime or scaledtime

        accmulateDelay = accmulateDelay + t
        if timer.delay <= 0 or timer.delay <= accmulateDelay then
            accmulateTime = accmulateTime + t

            local bOk = true
            if not delayCalled then
                info.delayCalled = true
                bOk = innerTimerCall(timer)
            end

            local interval = timer.interval
            while bOk and accmulateTime >= interval do
                accmulateTime = accmulateTime - interval

                if not innerTimerCall(timer) then
                    break
                end

                if interval <= 0 then
                    accmulateTime = 0
                    break
                end
            end
            info.accmulateTime = accmulateTime
        end
        info.accmulateDelay = accmulateDelay
    end
end)

function LuaTimer.AddTimer(callback, interval, unscaled, delay, loop)
    timerUniqueId = timerUniqueId + 1
    table.insert(tTimers, {
        Id = timerUniqueId,
        callback = callback,
        interval = interval or 0,
        unscaled = unscaled or false,
        delay = delay or 0,
        loop = loop or 0,
    })
    tTimersInfo[timerUniqueId] = {
        accmulateDelay = 0,
        accmulateTime = 0,
        accmulateLoop = 0,
        delayCalled = delay <= 0 and true or false
    }
    return timerUniqueId
end

function LuaTimer.RemoveTimer(timerId)
    tRemove[timerId] = true
    tTimersInfo[timerUniqueId] = nil
end