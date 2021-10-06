function Handler(obj, method, ...)
    local args = {...}
    return function()
        method(obj, table.unpack(args))
    end
end