continueLoop = true

function table_concat(t1,t2)
    for i=1,#t2 do
        t1[#t1+1] = t2[i]
    end
    return t1
end

function str_to_bool(str)
    if str == nil then
        return false
    end
    return string.lower(str) == 'true'
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

function write_memory(addr, value)
    memory.writebyte(addr, value)
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
    if string.find(cmd, "next_frame ") then
        emu.frameadvance()
        okay()
        return
    end
    if string.find(cmd, "next_frames ") then
        local updated_cmd = string.sub(cmd, 13)
        local space_index = string.find(updated_cmd, " ")
        local total_frames_str = string.sub(updated_cmd, 1, space_index)
        local repeat_input_str = string.sub(updated_cmd, space_index + 1)

        local total_frames = tonumber(total_frames_str)
        local repeat_input = str_to_bool(repeat_input_str)

        local input = joypad.get()

        for i=1,total_frames do
            emu.frameadvance()
            
            if repeat_input and i ~= total_frames then
                joypad.set(input)
            end
        end
        
        okay()
        return
    end
    if string.find(cmd, "write_memory ") then
        local updated_cmd = string.sub(cmd, 14)
        local space_index = string.find(updated_cmd, " ")
        local addr = string.sub(updated_cmd, 1, space_index)
        local value = string.sub(updated_cmd, space_index + 1)
        write_memory(tonumber(addr), tonumber(value))    
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
        while string.find(updated_cmd, "%(") ~= nil do
            local open_index = string.find(updated_cmd, "%(")
            local close_index = string.find(updated_cmd, "%)")
            local sub_cmd = string.sub(updated_cmd, open_index + 1, close_index - 1)
            local inputs = {}
            local analog_inputs = {}
            if string.find(sub_cmd, "A") then
                inputs.A = true 
            end
            if string.find(sub_cmd, "B") then 
                inputs.B = true 
            end
            if string.find(sub_cmd, "X") then 
                inputs.X = true 
            end
            if string.find(sub_cmd, "Y") then 
                inputs.Y = true 
            end
            if string.find(sub_cmd, "Z") then 
                inputs.Z = true 
            end
            if string.find(sub_cmd, "u") then 
                inputs.Up = true 
            end
            if string.find(sub_cmd, "d") then 
                inputs.Down = true 
            end
            if string.find(sub_cmd, "l") then 
                inputs.Left = true 
            end
            if string.find(sub_cmd, "r") then 
                inputs.Right = true 
            end
            if string.find(sub_cmd, "L") then 
                inputs.L = true 
            end
            if string.find(sub_cmd, "R") then 
                inputs.R = true 
            end
            if string.find(sub_cmd, "S") then 
                inputs.Start = true 
            end
            if string.find(sub_cmd, "s") then 
                inputs.Select = true 
            end
            if string.find(sub_cmd, "Cu") then 
                inputs["C Up"] = true 
            end
            if string.find(sub_cmd, "Cd") then 
                inputs["C Down"] = true 
            end
            if string.find(sub_cmd, "Cl") then 
                inputs["C Left"] = true 
            end
            if string.find(sub_cmd, "Cr") then 
                inputs["C Right"] = true 
            end
            if string.find(sub_cmd, "Du") then 
                inputs["DPad U"] = true 
            end
            if string.find(sub_cmd, "Dd") then 
                inputs["DPad D"] = true 
            end
            if string.find(sub_cmd, "Dl") then 
                inputs["DPad L"] = true 
            end
            if string.find(sub_cmd, "Dr") then 
                inputs["DPad R"] = true 
            end
            if string.find(sub_cmd, "Jx") then
                local start_x = string.find(sub_cmd, "Jx")
                local end_x = string.find(sub_cmd, ";", start_x)
                local tilt_value = tonumber(string.sub(sub_cmd, start_x + 2, end_x - 1))

                analog_inputs["X Axis"] = tilt_value
            end
            if string.find(sub_cmd, "Jy") then
                local start_y = string.find(sub_cmd, "Jy")
                local end_y = string.find(sub_cmd, ";", start_y)
                local tilt_value = tonumber(string.sub(sub_cmd, start_y + 2, end_y - 1))
                
                analog_inputs["Y Axis"] = tilt_value
            end

            local player_number = tonumber(string.sub(updated_cmd, 2, 2))
            if player_number == 0 then
                joypad.set(inputs)
                joypad.setanalog(analog_inputs)
            else
                joypad.set(inputs, player_number)
                joypad.setanalog(analog_inputs, player_number)
            end

            updated_cmd = string.sub(updated_cmd, close_index + 1)
        end
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
