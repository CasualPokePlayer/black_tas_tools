local textbox_ended
event.on_bus_exec(function()
	textbox_ended = true
end, 0x02193294)

while true do
	if textbox_ended then
		local a_press_frame = emu.framecount() - 2
		local pressed_a = movie.getinput(a_press_frame)["A"]
		if not pressed_a then
			tastudio.submitinputchange(a_press_frame, "A", true)
			tastudio.applyinputchanges()
		end

		textbox_ended = false
	end
	emu.frameadvance()
end
