Coroutine = {}

local coMap = {}
function Coroutine.Start(fn, ...)
    local args = {...}
    local co = coroutine.create(fn)
    local action = function()
        local flag = coroutine.resume(co, table.unpack(args))
        if not flag then
            Coroutine.Stop(co)
        end
    end
    coMap[co] = action
    LuaUpdate.AddUpdate(action)
    return co
end

function Coroutine.Stop(co)
    local action = coMap[co]
    LuaUpdate.RemoveUpdate(action)
    coMap[co] = nil
end

function Coroutine.WaitForSeconds(seconds, unscaled)
    while seconds >= 0 do
        seconds = seconds - (unscaled and Time.unscaledDeltaTime or Time.deltaTime)
        coroutine.yield()
    end
end

function Coroutine.Yield()
    return coroutine.yield()
end

function Coroutine.WWW(request)
    while not request.isDone do
        coroutine.yield()
    end
end

function Coroutine.WaitUntil(fn, ...)
    while not fn(...) do
        coroutine.yield()
    end
end

function Coroutine.YieldStart(fn, ...)
    local co = Coroutine.Start(fn, ...)
    Coroutine.WaitUntil(function()
        return coMap[co] == nil
    end)
end

function Coroutine.YieldIPairs(t, fn)
    for k, v in ipairs(t) do
        fn(k, v)
        coroutine.yield()
    end
end

function Coroutine.YieldPairs(t, fn)
    for k, v in pairs(t) do
        fn(k, v)
        coroutine.yield()
    end
end