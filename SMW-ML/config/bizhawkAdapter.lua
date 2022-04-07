continueLoop = true

function table_concat(t1,t2)
    for i=1,#t2 do
        t1[#t1+1] = t2[i]
    end
    return t1
end

function okay() 
    comm.socketServerSendBytes({1})
end

function exit_emu(code)
    continueLoop = false
    client.exitCode(code)
end

function read_memory(addr, count)
    return memory.read_bytes_as_array(addr, count)
end

function parseCommand(cmd)
    if string.find(cmd, "exit ") then
        okay()
        exit_emu(tonumber(string.sub(cmd, 6)))
        return
    end
    if string.find(cmd, "exit") then
        okay()
        exit_emu(0)
        return
    end
    if string.find(cmd, "load_rom ") then
        local path = string.sub(cmd, 10)
        client.openrom(path)
        okay()
        return
    end
    if string.find(cmd, "load_state ") then
        savestate.load(string.sub(cmd, 12))
        okay()
        return
    end
    if string.find(cmd, "next_frame") then
        emu.frameadvance()
        okay()
        return
    end
    if string.find(cmd, "read_memory ") then
        local singleByte = read_memory(tonumber(string.sub(cmd, 13)), 1)[1]
        okay()
        comm.socketServerSendBytes({singleByte})
        return
    end
    if string.find(cmd, "read_memory_range ") then
        local updated_cmd = string.sub(cmd, 19)
        local spaceIndex = string.find(updated_cmd, " ")
        local addr = string.sub(updated_cmd, 1, spaceIndex)
        local count = string.sub(updated_cmd, spaceIndex + 1)
        local bytes = read_memory(tonumber(addr), tonumber(count))
        
        okay()
        comm.socketServerSendBytes(bytes)
        return
    end
    if string.find(cmd, "read_memory_ranges ") then
        emu.frameadvance()
        local updated_cmd = string.sub(cmd, 20)
        local bytes = {}
        local space_index = string.find(updated_cmd, " ")
        while space_index ~= nil do
            local next_range = string.find(updated_cmd, ";")
            local addr = string.sub(updated_cmd, 1, space_index)
            local count = string.sub(updated_cmd, space_index + 1, next_range - 1)
            table_concat(bytes, read_memory(tonumber(addr), tonumber(count)))

            updated_cmd = string.sub(updated_cmd, next_range + 1)
            space_index = string.find(updated_cmd, " ")
        end

        okay()
        comm.socketServerSendBytes(bytes)
        return
    end
    if string.find(cmd, "send_input ") then
        local updated_cmd = string.sub(cmd, 12)
        local inputs = {}
        if string.find(updated_cmd, "A") then 
            inputs.A = true 
        end
        if string.find(updated_cmd, "B") then 
            inputs.B = true 
        end
        if string.find(updated_cmd, "X") then 
            inputs.X = true 
        end
        if string.find(updated_cmd, "Y") then 
            inputs.Y = true 
        end
        if string.find(updated_cmd, "u") then 
            inputs.Up = true 
        end
        if string.find(updated_cmd, "d") then 
            inputs.Down = true 
        end
        if string.find(updated_cmd, "l") then 
            inputs.Left = true 
        end
        if string.find(updated_cmd, "r") then 
            inputs.Right = true 
        end
        if string.find(updated_cmd, "L") then 
            inputs.L = true 
        end
        if string.find(updated_cmd, "R") then 
            inputs.R = true 
        end
        if string.find(updated_cmd, "S") then 
            inputs.Start = true 
        end
        if string.find(updated_cmd, "s") then 
            inputs.Select = true 
        end

        joypad.set(inputs, 1)
        okay()
        return
    end
end

function loop()
    while continueLoop do
        local cmd = comm.socketServerResponse()
        parseCommand(cmd)
    end
end


client.gettool("luaconsole").Hide()
loop()

