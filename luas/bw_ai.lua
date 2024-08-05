local function ptr_is_invalid(ptr)
	return (ptr >> 24) ~= 2
end

local function read_enemy_move_index()
	local ptr = memory.read_u32_le(0x269780, "Main RAM")
	if ptr_is_invalid(ptr) then return -1 end
	ptr = (ptr & 0xFFFFFF) + 0xF0
	ptr = memory.read_u32_le(ptr, "Main RAM")
	if ptr_is_invalid(ptr) then return -1 end
	ptr = (ptr & 0xFFFFFF) + 0x20
	ptr = memory.read_u32_le(ptr, "Main RAM")
	if ptr_is_invalid(ptr) then return -1 end
	ptr = (ptr & 0xFFFFFF) + 0x46
	return memory.read_u8(ptr, "Main RAM")
end

-- memory.write_u32_le(0x215354 + 0x9C0, memory.read_u32_le(0x215354 + 0x9C0, "Main RAM") + 1, "Main RAM")

local ptr_inc = 0
while true do
	tastudio.loadbranch(32 - 1)
	local mtrngptr = memory.read_u32_le(0x215354 + 0x9C0, "Main RAM")
	ptr_inc = ptr_inc + 1
	if (mtrngptr + ptr_inc) == 0x270 then break end
	mtrngptr = mtrngptr + ptr_inc
	memory.write_u32_le(0x215354 + 0x9C0, mtrngptr, "Main RAM")
	for i=0,200 do
		emu.frameadvance()
	end
	local move_index = read_enemy_move_index()
	print(move_index)
end
