--client.SetClientExtraPadding(0, 0, 300, 0)

--event.on_bus_exec(function()
--	local r1 = emu.getregister("ARM9 r1") - 0x40
--	for i=0,0xF do
--		local addr = r1 + i * 4
--		local part = memory.read_u32_le(addr, "ARM9 System Bus")
--		console.writeline(string.format("Addr %08X: %08X", addr, part))
--	end
--end, 0x02081ee8)

local function sha1_message_hook()
	console.write("SHA1 Message: ")
	local r1 = emu.getregister("ARM9 r1") - 0x40
	for i=0,0xF do
		local addr = r1 + i * 4
		local part = memory.read_u32_le(addr, "ARM9 System Bus")
		console.write(string.format("%08X", part))
	end
	console.writeline("")
end

local sha1_message_hook_id

function toggle_sha1_message_hook()
	if sha1_message_hook_id then
		event.unregisterbyid(sha1_message_hook_id)
		sha1_message_hook_id = nil
	else
		--sha1_message_hook_id = event.on_bus_exec(sha1_message_hook, 0x02081ee8)
		sha1_message_hook_id = event.on_bus_read(sha1_message_hook, 0x02081E6C)
	end
end

--sha1_message_hook_id = event.on_bus_exec(sha1_message_hook, 0x02081ee8)
sha1_message_hook_id = event.on_bus_read(sha1_message_hook, 0x02081E6C)

event.on_bus_exec(function()
	console.write("SHA1 Hash: ")
	local r4 = emu.getregister("ARM9 r4")
	for i=0,4 do
		local addr = r4 + i * 4
		local part = memory.read_u32_le(addr, "ARM9 System Bus")
		console.write(string.format("%08X", part))
	end
	console.writeline("")
end, 0x0208159c, "SHA1 Hash Complete Hook")
event.unregisterbyname("SHA1 Hash Complete Hook")

--event.on_bus_exec(function()
--	console.writeline("PIDRNG was reseeded")
--end, 0x02081ee8)

--event.on_bus_write(function()
--	local r5 = emu.getregister("ARM9 r5")
--	--if (r5 == 0xFFFF) then return end
--	--if (r5 = 1000) then return end
--	local step_cnt = memory.read_u8(0x235128, "Main RAM") + 1
--	console.writeline(string.format("PIDRNG was changed %08X, r5: %08X, result: %08X (F: %d, S: %d)", emu.getregister("ARM9 r15"), r5, ((emu.getregister("ARM9 r3") & 0xFFFFFFFF) * r5) >> 32, emu.framecount(), step_cnt))
--end, 0x02216224)

--local encounter_slot = 0
--event.on_bus_write(function()
--	local r5 = emu.getregister("ARM9 r5")
--	if (r5 ~= 0xFFFF) then return end
--	local result = ((emu.getregister("ARM9 r3") & 0xFFFFFFFF) * r5) >> 32
--	if (result ~= 0x42E8) then return end
--	console.writeline(string.format("Overriding encounter slot with %d", encounter_slot))
--	local rng_override = encounter_slot * 0x290
--	emu.setregister("ARM9 r3", (0x100000000 // 0xFFFF) * rng_override + (0x100000000 % 0xFFFF))
--	encounter_slot = (encounter_slot + 1) % 100
--end, 0x02216224)


--event.on_bus_write(function()
--	console.writeline(string.format("IVRNG was changed %08X", emu.getregister("ARM9 r15")))
--end, 0x02215354)

-- accuracy check override
--event.on_bus_exec(function()
--	emu.setregister("ARM9 r0", 0x64)
--end, 0x021bfbb4)

-- critical hit check override
--event.on_bus_exec(function()
--	emu.setregister("ARM9 r0", 0)
--end, 0x021d793a)

-- damage roll override
--event.on_bus_exec(function()
--	emu.setregister("ARM9 r0", -1000)
--end, 0x021c1f90)

-- ???
--event.on_bus_exec(function()
--	emu.setregister("ARM9 r0", 0)
--end, 0x0689cf8c)

--event.on_bus_exec(function()
--	if (emu.getregister("ARM9 r0") & 0xFF) ~= 0x4F then return end
--	emu.setregister("ARM9 r0", 0)
--end, 0x021a9dd8)

-- 0 -> 18
-- 1 -> 13
-- 2 -> 18
-- 3 -> 17

-- 0, 0 -> 0 right, 2 up
-- 0, 1 -> 1 right, 2 up
-- 0, 2 -> 2 right, 2 up
-- 0, 3 -> 0 right, 1 up
-- 0, 4 -> 1 right, 1 up
-- 0, 5 -> no cloud (2 right, 1 up?)
-- 0, 6 -> no cloud (0 right, 0 up?)
-- 0, 7 -> 1 right, 0 up
-- 0, 8 -> 2 right, 0 up
-- 0, 9 -> 3 right, 0 up
-- 0, 10 -> 4 right, 0 up
-- 0, 11 -> 5 right, 0 up
-- 0, 12 -> 0 right, 1 down
-- 0, 13 -> 1 right, 1 down
-- 0, 14 -> 2 right, 1 down
-- 0, 15 -> 3 right, 1 down
-- 0, 16 -> 4 right, 1 down
-- 0, 17 -> 5 right, 1 down

-- 1, 0 -> 2 left, 2 up
-- 1, 1 -> 1 left, 2 up
-- 1, 2 -> 0 left, 2 up
-- 1, 3 -> 2 left, 1 up
-- 1, 4 -> 1 left, 1 up
-- 1, 5 -> 0 left, 1 up
-- 1, 6 -> 2 left, 0 up
-- 1, 7 -> 1 left, 0 up
-- 1, 8 -> no cloud (0 left, 0 up?)
-- 1, 9 -> 4 left, 1 down
-- 1, 10 -> 3 left, 1 down
-- 1, 11 -> 1 left, 1 down
-- 1, 12 -> 0 left, 1 down

-- 2, 0 -> 2 left, 2 up
-- 2, 1 -> 1 left, 2 up
-- 2, 2 -> 0 left, 2 up
-- 2, 3 -> 1 right, 2 up
-- 2, 4 -> 2 right, 2 up
-- 2, 5 -> 2 left, 1 up
-- 2, 6 -> 1 left, 1 up
-- 2, 7 -> 0 left, 1 up
-- 2, 8 -> 1 right, 1 up
-- 2, 9 -> no cloud (1 right, 1 up?)
-- 2, 10 -> 2 left, 0 up
-- 2, 11 -> 1 left, 0 up
-- 2, 12 -> no cloud (0 left, 0 up?)
-- 2, 13 -> 1 right, 0 up
-- 2, 14 -> 2 right, 0 up
-- 2, 15 -> 3 right, 0 up
-- 2, 16 -> 4 right, 0 up
-- 2, 17 -> 5 right, 0 up

-- 3, 0 -> 2 left, 0 up
-- 3, 1 -> 1 left, 0 up
-- 3, 2 -> no cloud (0 left, 0 up?)
-- 3, 3 -> 1 right, 0 up

-- 0 means scan rightwards, 1 means scan leftwards, 2 means scan upwards, 3 means scan downwards

local override_cnt = 0
local override_n = 0
local override_m = 0

event.on_bus_write(function()
	local r5 = emu.getregister("ARM9 r5")
	if (r5 == 1000) then
		override_cnt = 3
	end

	if override_cnt > 0 then
		if (override_cnt == 3) then
			emu.setregister("ARM9 r3", 0)
		end

		if (override_cnt == 2) then
			emu.setregister("ARM9 r3", (0x100000000 // 4) * override_n + (0x100000000 % 4))
		end

		if (override_cnt == 1) then
			console.writeline(string.format("Using %d for M", override_m))
			emu.setregister("ARM9 r3", (0x100000000 // r5) * override_m + (0x100000000 % r5))
			override_m = (override_m + 1) % r5
		end

		override_cnt = override_cnt - 1
	end
end, 0x02216224, "Override Dust Cloud Hook")
event.unregisterbyname("Override Dust Cloud Hook")

-- by return address
local battle_rng_callers =
{
	[0x0689CF8D] = "Unknown speedtie-related 50/50 Roll at 0689CF8D",
	[0x021BC8AF] = "Speedtie Check",
	[0x021BFBB1] = "Accuracy Check",
	[0x021D793B] = "Critical Hit",
	[0x021C1F91] = "Damage Roll",
	[0x021C7769] = "Flinch Roll",
	[0x021CBCD3] = "Shake Check",
	[0x021D7BA1] = "Confusion Turn Count Roll",
	[0x021C2447] = "Paralysis / Confusion Roll",
	[0x021D7AFB] = "?/100 Roll (Confusion Self Hit Check / Quick Claw / Static / Cursed Body)",
	[0x021C640B] = "Confusion Damage Roll",
	[0x021DA4C7] = "Forewarn Roll",
	[0x021C2D97] = "SpDef Drop Roll",
	[0x021DA685] = "Frisk Roll",
}

-- 1, 0, enemy wins
-- 0, 0, player wins
-- 0, 1, enemy wins
-- 1, 1, player wins

local function rng_call_hook()
	local sp = emu.getregister("ARM9 r13")
	local ret_addr = memory.read_u32_le(sp + 12, "ARM9 System Bus")
	local caller = battle_rng_callers[ret_addr]
	if (caller == nil) then
		caller = string.format("Unknown caller %08X", ret_addr)
	end
	console.writeline(string.format("Battle RNG Caller: %s (r0: %08X, r4: %08X)", caller, emu.getregister("ARM9 r0"), emu.getregister("ARM9 r4")))
	if (ret_addr == 0x021BC8AF) then
		--emu.setregister("ARM9 r0", 0)
	end
end

--event.on_bus_write(function()
--	console.write("\r\nBattle RNG was changed")
--end, 0x021F6368)

--event.on_bus_write(function()
--	console.writeline(string.format("%08X", emu.getregister("ARM9 r15")))
--end, 0x0226f47c)

--event.on_bus_exec(rng_call_hook, 0x021d78de)

local mtrng_write_cnt = 0
--event.on_bus_write(function()
--	if (memory.read_u32_le(0x215D14, "Main RAM") == 624) then return end
--	mtrng_write_cnt = mtrng_write_cnt + 1
--	--console.writeline(string.format("%08X", emu.getregister("ARM9 r15")))
--	--0203F1C8
--end, 0x2215D14, "MTRNG Write Hook")
--event.unregisterbyname("MTRNG Write Hook")

-- mtrng blinking override
local mtrng_result = 0xFFFFFFFF
function toggle_blinking()
	mtrng_result = mtrng_result ~ 0xFFFFFFFF
end
--event.on_bus_exec(function()
--	emu.setregister("ARM9 r0", mtrng_result)
--end, 0x0203f1e4)

--event.on_bus_write(function()
--	console.writeline(string.format("%08X", emu.getregister("ARM9 r15")))
--  --02087088
--end, 0x02151338)
--event.on_bus_read(function()
--	console.writeline(string.format("%08X", emu.getregister("ARM9 r15")))
--end, 0x02151338)
--event.on_bus_read(function()
--	local cy = emu.totalexecutedcycles()
--	if (cy < 5023099052) then return end
--	console.writeline(string.format("%08X (%d)", emu.getregister("ARM9 r15"), emu.totalexecutedcycles()))
--	event.unregisterbyname("??? Hook")
--end, 0x02fe3698, "??? Hook")

--local mtrng_exec_cnt = 0
--event.on_bus_exec(function()
--	mtrng_exec_cnt = mtrng_exec_cnt + 1
--end, 0x0203f0a8)

--event.on_bus_exec(function()
--	emu.setregister("ARM9 r0", 0x4000)
--	emu.setregister("ARM9 r1", 0x4000)
--end, 0x0201b050)

--event.on_bus_exec(function()
--	console.writeline(string.format("%04X", emu.getregister("ARM9 r0") >> 16 & 0xFFFF))
--end, 0x0201b048)

memory.read_u64_le = function(addr, domain)
	local l = memory.read_u32_le(addr, domain)
	local h = memory.read_u32_le(addr + 4, domain)
	return (h << 32) | l
end

memory.write_u64_le = function(addr, val, domain)
	memory.write_u32_le(addr, val & 0xFFFFFFFF, domain)
	memory.write_u32_le(addr + 4, val >> 32, domain)
end

local npc_addr = 0x2521e4
local npc_num = -1
local npc_write_cnts = {}
local npc_active_cnts = {}
local npc_walking_cnts = {}

local function npc_timer_write_hook(addr)
	local index = (addr - (0x02000000 + npc_addr + 0x96)) // 0x100
	npc_write_cnts[index] = npc_write_cnts[index] + 1
end

for i=0,npc_num do
	--event.on_bus_write(npc_timer_write_hook, 0x02000000 + npc_addr + i * 0x100 + 0x96)
	npc_write_cnts[i] = 0
	npc_active_cnts[i] = 0
	npc_walking_cnts[i] = 0
end

function get_pidrng()
	local prng = memory.read_u64_le(0x216224, "Main RAM")
	return string.format("%016X", prng)
end

function get_battlerng()
	local brng = memory.read_u64_le(0x1F6368, "Main RAM")
	return string.format("%016X", brng)
end

local function get_nextivrng(frame)
	local ret
	if (frame == 0x270) then
		-- shuffling
		local m0 = memory.read_u32_le(0x215354 + 0, "Main RAM")
		local m1 = memory.read_u32_le(0x215354 + 4, "Main RAM")
		local m2 = memory.read_u32_le(0x215354 + 8, "Main RAM")
		local y = (m0 & 0x80000000) | (m1 & 0x7FFFFFFF)
		local y1 = y >> 1
		local mag01 = (y & 1) * 0x9908B0DF
		ret = y1 ~ mag01 ~ m2;
	else
		ret = memory.read_u32_le(0x215354 + frame * 4, "Main RAM")
	end

	-- Tempering
	ret = ret ~ (ret >> 11);
	ret = ret ~ ((ret << 7) & 0x9D2C5680);
	ret = ret ~ ((ret << 15) & 0xEFC60000);
	ret = ret ~ (ret >> 18);
	return ret
end

local function ptr_is_invalid(ptr)
	return (ptr >> 24) ~= 2
end

function read_enemy_move_ptr()
	local ptr = memory.read_u32_le(0x269780, "Main RAM")
	if ptr_is_invalid(ptr) then return -1 end
	ptr = (ptr & 0xFFFFFF) + 0xF0
	ptr = memory.read_u32_le(ptr, "Main RAM")
	if ptr_is_invalid(ptr) then return -1 end
	ptr = (ptr & 0xFFFFFF) + 0x20
	ptr = memory.read_u32_le(ptr, "Main RAM")
	if ptr_is_invalid(ptr) then return -1 end
	return string.format("%08X", ptr)
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

local function read_partner_move_index()
	local ptr = memory.read_u32_le(0x269784, "Main RAM")
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

while true do
	local d = memory.read_u32_le(0x3FFDE8, "Main RAM")
	local t = memory.read_u32_le(0x3FFDEC, "Main RAM")
	local vframe = memory.read_u32_le(0x3FFC3C, "Main RAM")
	gui.text(5, 5, string.format("Date: %08X", d), nil, "topright")
	gui.text(5, 25, string.format("Time: %08X", t), nil, "topright")
	gui.text(5, 45, string.format("VFrame: %08X", vframe), nil, "topright")
	local prng = memory.read_u64_le(0x216224, "Main RAM")
	gui.text(5, 65, string.format("PIDRNG: %016X", prng), nil, "topright")
	local mtrngptr = memory.read_u32_le(0x215354 + 0x9C0, "Main RAM") -- 0x215D14
	gui.text(5, 85, string.format("IVRNG Frame: %08X", mtrngptr % 0x270), nil, "topright")
	gui.text(5, 105, string.format("Next IVRNG: %08X", get_nextivrng(mtrngptr)), nil, "topright")
	local brng = memory.read_u64_le(0x1F6368, "Main RAM")
	gui.text(5, 125, string.format("Battle RNG: %016X", brng), nil, "topright")
	--gui.text(5, 145, string.format("MTRNG Write Count: %d", mtrng_write_cnt), nil, "topright")
	--gui.text(5, 165, string.format("MTRNG Exec Count: %d", mtrng_exec_cnt), nil, "topright")
	local step_cnt = memory.read_u8(0x235128, "Main RAM")
	gui.text(5, 145, string.format("Step Counter: %d", step_cnt), nil, "topright")
	local enemy_move_index = read_enemy_move_index()
	gui.text(5, 165, string.format("Enemy Move: %d", enemy_move_index), nil, "topright")
	local partner_move_index = read_partner_move_index()
	gui.text(5, 185, string.format("Partner Move: %d", partner_move_index), nil, "topright")
	for i=0,npc_num do
		--local npc_flags = memory.read_u16_le(npc_addr + i * 0x100 + 4, "Main RAM")
		--if (npc_flags == 0x4002) then
		--	npc_active_cnts[i] = npc_active_cnts[i] + 1
		--end
		--if (npc_flags == 0x4012) then
		--	npc_walking_cnts[i] = npc_walking_cnts[i] + 1
		--end
		--local npc_dir = memory.read_u16_le(npc_addr + i * 0x100 + 0x94, "Main RAM")
		local npc_timer = memory.read_u16_le(npc_addr + i * 0x100 + 0x96, "Main RAM")
		--gui.text(5, 205 + i * 20, string.format("NPC Dir/Timer/Writes/Active/Walking: %d/%d/%d/%d/%d", npc_dir, npc_timer, npc_write_cnts[i], npc_active_cnts[i], npc_walking_cnts[i]), nil, "topright")
		gui.text(5, 205 + i * 20, string.format("NPC%d Timer: %d", i, npc_timer), nil, "topright")
	end
	emu.frameadvance()
end
