for i=0,0x3FFFFF do
	local start_str = memory.read_u8(i, "Main RAM")
	if (start_str == 0x74) then
		local str = memory.read_bytes_as_array(i, 7, "Main RAM")
		if (str[1] == 0x74 and str[2] == 0x72 and str[3] == 0x5F and str[4] == 0x61 and str[5] == 0x69 and str[6] == 0x2E and str[7] == 0x63) then
			local move = memory.read_u8(i + 0x5E, "Main RAM")
			console.writeline(string.format("%08X / %d", i + 0x5E, move))
		end
	end
end

