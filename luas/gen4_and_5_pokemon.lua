-- Pokemon gen 4 lua script by MKDasher
-----------
	-- Press 3 - 4 to change mode (Party / Enemy / Enemy 2 / Partner / Wild)
	-- Press 7 - 8 to change number slot.
	-- Press 9 to change view.
-----------

gui.use_surface("emu")
client.SetGameExtraPadding(0,0,256,0)

local pidAddr
local pid = 0
local trainerID, secretID, lotteryID
local shiftvalue
local checksum = 0

local mode = 1
local modetext = "Party"
local submode = 1
local modemax = 5
local submodemax = 6
local tabl = {}
local prev = {}

local leftarrow1color, rightarrow1color, leftarrow2color, rightarrow2color

local prng

--BlockA
local pokemonID = 0
local heldItem = 0
local OTID, OTSID
local friendship_or_steps_to_hatch
local ability
local hpev, atkev, defev, speev, spaev, spdev
local evs = {}

--BlockB
local move = {}
local movepp = {}
local hpiv, atkiv, defiv, speiv, spaiv, spdiv
local ivspart = {}, ivs
local isegg
local nat

local bnd = function(x, y) return x & y end
local br = function(x, y) return x | y end
local bxr = function(x, y) return x ~ y end
local rshift = function(x, y) return x >> y end
local lshift = function(x, y) return x << y end
local mdword = memory.read_u32_le
local mword = memory.read_u16_le
local mbyte = memory.read_u8_le
local tohex = function(x) return string.format("%08X", x) end

--BlockD
local pkrs

--currentStats
local level, hpstat, maxhpstat, atkstat, defstat, spestat, spastat, spdstat

local hiddentype, hiddenpower

--offsets
local BlockAoff, BlockBoff, BlockCoff, BlockDoff

local BlockA = { 1,1,1,1,1,1, 2,2,3,4,3,4, 2,2,3,4,3,4, 2,2,3,4,3,4 }
local BlockB = { 2,2,3,4,3,4, 1,1,1,1,1,1, 3,4,2,2,4,3, 3,4,2,2,4,3 }
local BlockC = { 3,4,2,2,4,3, 3,4,2,2,4,3, 1,1,1,1,1,1, 4,3,4,3,2,2 }
local BlockD = { 4,3,4,3,2,2, 4,3,4,3,2,2, 4,3,4,3,2,2, 1,1,1,1,1,1 }

local nature =
{
	"Hardy","Lonely","Brave","Adamant","Naughty",
	"Bold","Docile","Relaxed","Impish","Lax",
	"Timid","Hasty","Serious","Jolly","Naive",
	"Modest","Mild","Quiet","Bashful","Rash",
	"Calm","Gentle","Sassy","Careful","Quirky"
}

-- Used for Hidden Power, so no Normal type			
local pkmntype =
{
	"Fighting","Flying","Poison","Ground",
	"Rock","Bug","Ghost","Steel",
	"Fire","Water","Grass","Electric",
	"Psychic","Ice","Dragon","Dark"
}

local pokemon =
{
	"none", "Bulbasaur", "Ivysaur", "Venusaur", "Charmander", "Charmeleon", "Charizard",
	"Squirtle", "Wartortle", "Blastoise", "Caterpie", "Metapod", "Butterfree",
	"Weedle", "Kakuna", "Beedrill", "Pidgey", "Pidgeotto", "Pidgeot", "Rattata", "Raticate",
	"Spearow", "Fearow", "Ekans", "Arbok", "Pikachu", "Raichu", "Sandshrew", "Sandslash",
	"NidoranF", "Nidorina", "Nidoqueen", "NidoranM", "Nidorino", "Nidoking",
	"Clefairy", "Clefable", "Vulpix", "Ninetales", "Jigglypuff", "Wigglytuff",
	"Zubat", "Golbat", "Oddish", "Gloom", "Vileplume", "Paras", "Parasect", "Venonat", "Venomoth",
	"Diglett", "Dugtrio", "Meowth", "Persian", "Psyduck", "Golduck", "Mankey", "Primeape",
	"Growlithe", "Arcanine", "Poliwag", "Poliwhirl", "Poliwrath", "Abra", "Kadabra", "Alakazam",
	"Machop", "Machoke", "Machamp", "Bellsprout", "Weepinbell", "Victreebel", "Tentacool", "Tentacruel",
	"Geodude", "Graveler", "Golem", "Ponyta", "Rapidash", "Slowpoke", "Slowbro",
	"Magnemite", "Magneton", "Farfetch'd", "Doduo", "Dodrio", "Seel", "Dewgong", "Grimer", "Muk",
	"Shellder", "Cloyster", "Gastly", "Haunter", "Gengar", "Onix", "Drowzee", "Hypno",
	"Krabby", "Kingler", "Voltorb", "Electrode", "Exeggcute", "Exeggutor", "Cubone", "Marowak",
	"Hitmonlee", "Hitmonchan", "Lickitung", "Koffing", "Weezing", "Rhyhorn", "Rhydon", "Chansey",
	"Tangela", "Kangaskhan", "Horsea", "Seadra", "Goldeen", "Seaking", "Staryu", "Starmie",
	"Mr. Mime", "Scyther", "Jynx", "Electabuzz", "Magmar", "Pinsir", "Tauros", "Magikarp", "Gyarados",
	"Lapras", "Ditto", "Eevee", "Vaporeon", "Jolteon", "Flareon", "Porygon", "Omanyte", "Omastar",
	"Kabuto", "Kabutops", "Aerodactyl", "Snorlax", "Articuno", "Zapdos", "Moltres",
	"Dratini", "Dragonair", "Dragonite", "Mewtwo", "Mew",

	"Chikorita", "Bayleef", "Meganium", "Cyndaquil", "Quilava", "Typhlosion",
	"Totodile", "Croconaw", "Feraligatr", "Sentret", "Furret", "Hoothoot", "Noctowl",
	"Ledyba", "Ledian", "Spinarak", "Ariados", "Crobat", "Chinchou", "Lanturn", "Pichu", "Cleffa",
	"Igglybuff", "Togepi", "Togetic", "Natu", "Xatu", "Mareep", "Flaaffy", "Ampharos", "Bellossom",
	"Marill", "Azumarill", "Sudowoodo", "Politoed", "Hoppip", "Skiploom", "Jumpluff", "Aipom",
	"Sunkern", "Sunflora", "Yanma", "Wooper", "Quagsire", "Espeon", "Umbreon", "Murkrow", "Slowking",
	"Misdreavus", "Unown", "Wobbuffet", "Girafarig", "Pineco", "Forretress", "Dunsparce", "Gligar",
	"Steelix", "Snubbull", "Granbull", "Qwilfish", "Scizor", "Shuckle", "Heracross", "Sneasel",
	"Teddiursa", "Ursaring", "Slugma", "Magcargo", "Swinub", "Piloswine", "Corsola", "Remoraid", "Octillery",
	"Delibird", "Mantine", "Skarmory", "Houndour", "Houndoom", "Kingdra", "Phanpy", "Donphan",
	"Porygon2", "Stantler", "Smeargle", "Tyrogue", "Hitmontop", "Smoochum", "Elekid", "Magby", "Miltank",
	"Blissey", "Raikou", "Entei", "Suicune", "Larvitar", "Pupitar", "Tyranitar", "Lugia", "Ho-Oh", "Celebi",

	"Treecko", "Grovyle", "Sceptile", "Torchic", "Combusken", "Blaziken", "Mudkip", "Marshtomp", 
	"Swampert", "Poochyena", "Mightyena", "Zigzagoon", "Linoone", "Wurmple", "Silcoon", "Beautifly",
	"Cascoon", "Dustox", "Lotad", "Lombre", "Ludicolo", "Seedot", "Nuzleaf", "Shiftry", 
	"Taillow", "Swellow", "Wingull", "Pelipper", "Ralts", "Kirlia", "Gardevoir", "Surskit", 
	"Masquerain", "Shroomish", "Breloom", "Slakoth", "Vigoroth", "Slaking", "Nincada", "Ninjask", 
	"Shedinja", "Whismur", "Loudred", "Exploud", "Makuhita", "Hariyama", "Azurill", "Nosepass", 
	"Skitty", "Delcatty", "Sableye", "Mawile", "Aron", "Lairon", "Aggron", "Meditite", "Medicham",
	"Electrike", "Manectric", "Plusle", "Minun", "Volbeat", "Illumise", "Roselia", "Gulpin", 
	"Swalot", "Carvanha", "Sharpedo", "Wailmer", "Wailord", "Numel", "Camerupt", "Torkoal", 
	"Spoink", "Grumpig", "Spinda", "Trapinch", "Vibrava", "Flygon", "Cacnea", "Cacturne", "Swablu",
	"Altaria", "Zangoose", "Seviper", "Lunatone", "Solrock", "Barboach", "Whiscash", "Corphish",
	"Crawdaunt", "Baltoy", "Claydol", "Lileep", "Cradily", "Anorith", "Armaldo", "Feebas", 
	"Milotic", "Castform", "Kecleon", "Shuppet", "Banette", "Duskull", "Dusclops", "Tropius", 
	"Chimecho", "Absol", "Wynaut", "Snorunt", "Glalie", "Spheal", "Sealeo", "Walrein", "Clamperl",
	"Huntail", "Gorebyss", "Relicanth", "Luvdisc", "Bagon", "Shelgon", "Salamence", "Beldum", 
	"Metang", "Metagross", "Regirock", "Regice", "Registeel", "Latias", "Latios", "Kyogre", 
	"Groudon", "Rayquaza", "Jirachi", "Deoxys",

	"Turtwig", "Grotle", "Torterra", "Chimchar", "Monferno", "Infernape", "Piplup", "Prinplup", 
	"Empoleon", "Starly", "Staravia", "Staraptor", "Bidoof", "Bibarel", "Kricketot", "Kricketune", 
	"Shinx", "Luxio", "Luxray", "Budew", "Roserade", "Cranidos", "Rampardos", "Shieldon", "Bastiodon", 
	"Burmy", "Wormadam", "Mothim", "Combee", "Vespiquen", "Pachirisu", "Buizel", "Floatzel", "Cherubi", 
	"Cherrim", "Shellos", "Gastrodon", "Ambipom", "Drifloon", "Drifblim", "Buneary", "Lopunny", 
	"Mismagius", "Honchkrow", "Glameow", "Purugly", "Chingling", "Stunky", "Skuntank", "Bronzor", 
	"Bronzong", "Bonsly", "Mime Jr.", "Happiny", "Chatot", "Spiritomb", "Gible", "Gabite", "Garchomp", 
	"Munchlax", "Riolu", "Lucario", "Hippopotas", "Hippowdon", "Skorupi", "Drapion", "Croagunk", 
	"Toxicroak", "Carnivine", "Finneon", "Lumineon", "Mantyke", "Snover", "Abomasnow", "Weavile", 
	"Magnezone", "Lickilicky", "Rhyperior", "Tangrowth", "Electivire", "Magmortar", "Togekiss", 
	"Yanmega", "Leafeon", "Glaceon", "Gliscor", "Mamoswine", "Porygon-Z", "Gallade", "Probopass", 
	"Dusknoir", "Froslass", "Rotom", "Uxie", "Mesprit", "Azelf", "Dialga", "Palkia", "Heatran", 
	"Regigigas", "Giratina", "Cresselia", "Phione", "Manaphy", "Darkrai", "Shaymin", "Arceus",

	"Victini", "Snivy", "Servine", "Serperior", "Tepig", "Pignite", "Emboar", "Oshawott", "Dewott", "Samurott", "Patrat", "Watchog",
	"Lillipup", "Herdier", "Stoutland", "Purrloin", "Liepard", "Pansage", "Simisage", "Pansear", "Simisear", "Panpour", "Simipour",
	"Munna", "Musharna", "Pidove", "Tranquill", "Unfezant", "Blitzle", "Zebstrika", "Roggenrola", "Boldore", "Gigalith", "Woobat",
	"Swoobat", "Drilbur", "Excadrill", "Audino", "Timburr", "Gurdurr", "Conkeldurr", "Tympole", "Palpitoad", "Seismitoad", "Throh",
	"Sawk", "Sewaddle", "Swadloon", "Leavanny", "Venipede", "Whirlipede", "Scolipede", "Cottonee", "Whimsicott", "Petilil",
	"Lilligant", "Basculin", "Sandile", "Krokorok", "Krookodile", "Darumaka", "Darmanitan", "Maractus", "Dwebble", "Crustle",
	"Scraggy", "Scrafty", "Sigilyph", "Yamask", "Cofagrigus", "Tirtouga", "Carracosta", "Archen", "Archeops", "Trubbish",
	"Garbodor", "Zorua", "Zoroark", "Minccino", "Cinccino", "Gothita", "Gothorita", "Gothitelle", "Solosis", "Duosion",
	"Reuniclus", "Ducklett", "Swanna", "Vanillite", "Vanillish", "Vanilluxe", "Deerling", "Sawsbuck", "Emolga", "Karrablast",
	"Escavalier", "Foongus", "Amoonguss", "Frillish", "Jellicent", "Alomomola", "Joltik", "Galvantula", "Ferroseed",
	"Ferrothorn", "Klink", "Klang", "Klinklang", "Tynamo", "Eelektrik", "Eelektross", "Elgyem", "Beheeyem", "Litwick",
	"Lampent", "Chandelure", "Axew", "Fraxure", "Haxorus", "Cubchoo", "Beartic", "Cryogonal", "Shelmet", "Accelgor",
	"Stunfisk", "Mienfoo", "Mienshao", "Druddigon", "Golett", "Golurk", "Pawniard", "Bisharp", "Bouffalant", "Rufflet",
	"Braviary", "Vullaby", "Mandibuzz", "Heatmor", "Durant", "Deino", "Zweilous", "Hydreigon", "Larvesta", "Volcarona", "Cobalion", 
	"Terrakion", "Virizion", "Tornadus", "Thundurus", "Reshiram", "Zekrom", "Landorus", "Kyurem", "Keldeo", "Meloetta", "Genesect",

	"Pokémon Egg", "Manaphy Egg"
}
			
abilities =
{
	"none", "Stench", "Drizzle", "Speed Boost", "Battle Armor", "Sturdy", "Damp", "Limber", "Sand Veil", "Static", "Volt Absorb", "Water Absorb", "Oblivious", "Cloud Nine",
	"Compound Eyes", "Insomnia", "Color Change", "Immunity", "Flash Fire", "Shield Dust", "Own Tempo", "Suction Cups", "Intimidate", "Shadow Tag", "Rough Skin", "Wonder Guard", "Levitate",
	"Effect Spore", "Synchronize", "Clear Body", "Natural Cure", "Lightning Rod", "Serene Grace", "Swift Swim", "Chlorophyll", "Illuminate", "Trace", "Huge Power", "Poison Point",
	"Inner Focus", "Magma Armor", "Water Veil", "Magnet Pull", "Soundproof", "Rain Dish", "Sand Stream", "Pressure", "Thick Fat", "Early Bird", "Flame Body", "Run Away", "Keen Eye",
	"Hyper Cutter", "Pickup", "Truant", "Hustle", "Cute Charm", "Plus", "Minus", "Forecast", "Sticky Hold", "Shed Skin", "Guts", "Marvel Scale", "Liquid Ooze", "Overgrow", "Blaze", "Torrent",
	"Swarm", "Rock Head", "Drought", "Arena Trap", "Vital Spirit", "White Smoke", "Pure Power", "Shell Armor", "Air Lock", "Tangled Feet", "Motor Drive", "Rivalry", "Steadfast", "Snow Cloak",
	"Gluttony", "Anger Point", "Unburden", "Heatproof", "Simple", "Dry Skin", "Download", "Iron Fist", "Poison Heal", "Adaptability", "Skill Link", "Hydration", "Solar Power", "Quick Feet",
	"Normalize", "Sniper", "Magic Guard", "No Guard", "Stall", "Technician", "Leaf Guard", "Klutz", "Mold Breaker", "Super Luck", "Aftermath", "Anticipation", "Forewarn", "Unaware", "Tinted Lens",
	"Filter", "Slow Start", "Scrappy", "Storm Drain", "Ice Body", "Solid Rock", "Snow Warning", "Honey Gather", "Frisk", "Reckless", "Multitype", "Flower Gift", "Bad Dreams",

	"Pickpocket", "Sheer Force", "Contrary", "Unnerve", "Defiant", "Defeatist", "Cursed Body", "Healer", "Friend Guard",
	"Weak Armor", "Heavy Metal", "Light Metal", "Multiscale", "Toxic Boost", "Flare Boost", "Harvest", "Telepathy", "Moody",
	"Overcoat", "Poison Touch", "Regenerator", "Big Pecks", "Sand Rush", "Wonder Skin", "Analytic", "Illusion", "Imposter",
	"Infiltrator", "Mummy", "Moxie", "Justified", "Rattled", "Magic Bounce", "Sap Sipper", "Prankster", "Sand Force",
	"Iron Barbs", "Zen Mode", "Victory Star", "Turboblaze", "Teravolt"
}

item_gen5 =
{
	"none", "Master Ball", "Ultra Ball", "Great Ball", "Poké Ball", "Safari Ball", "Net Ball", "Dive Ball",
	"Nest Ball", "Repeat Ball", "Timer Ball", "Luxury Ball", "Premier Ball", "Dusk Ball", "Heal Ball", "Quick Ball",
	"Cherish Ball", "Potion", "Antidote", "Burn Heal", "Ice Heal", "Awakening", "Parlyz Heal", "Full Restore",
	"Max Potion", "Hyper Potion", "Super Potion", "Full Heal", "Revive", "Max Revive", "Fresh Water", "Soda Pop",
	"Lemonade", "Moomoo Milk", "EnergyPowder", "Energy Root", "Heal Powder", "Revival Herb", "Ether", "Max Ether", "Elixir",
	"Max Elixir", "Lava Cookie", "Berry Juice", "Sacred Ash", "HP Up", "Protein", "Iron", "Carbos", "Calcium", "Rare Candy",
	"PP Up", "Zinc", "PP Max", "Old Gateau", "Guard Spec.", "Dire Hit", "X Attack", "X Defend", "X Speed", "X Accuracy", "X Special",
	"X Sp. Def", "Poké Doll", "Fluffy Tail", "Blue Flute", "Yellow Flute", "Red Flute", "Black Flute", "White Flute", "Shoal Salt",
	"Shoal Shell", "Red Shard", "Blue Shard", "Yellow Shard", "Green Shard", "Super Repel", "Max Repel", "Escape Rope", "Repel", "Sun Stone",
	"Moon Stone", "Fire Stone", "Thunder Stone", "Water Stone", "Leaf Stone", "TinyMushroom", "Big Mushroom", "Pearl", "Big Pearl",
	"Stardust", "Star Piece", "Nugget", "Heart Scale", "Honey", "Growth Mulch", "Damp Mulch", "Stable Mulch", "Gooey Mulch",
	"Root Fossil", "Claw Fossil", "Helix Fossil", "Dome Fossil", "Old Amber", "Armor Fossil", "Skull Fossil", "Rare Bone", "Shiny Stone",
	"Dusk Stone", "Dawn Stone", "Oval Stone", "Odd Keystone", "Griseous Orb", "unknown", "unknown", "unknown", "Douse Drive",
	"Shock Drive", "Burn Drive", "Chill Drive",

	"unknown", "unknown", "unknown", "unknown", "unknown", "unknown", "unknown", "unknown", "unknown", "unknown",
	"unknown", "unknown", "unknown", "unknown",

	"Sweet Heart", "Adamant Orb", "Lustrous Orb", "Greet Mail",
	"Favored Mail", "RSVP Mail", "Thanks Mail", "Inquiry Mail", "Like Mail", "Reply Mail", "BridgeMail S", "BridgeMail D", "BridgeMail T",
	"BridgeMail V", "BridgeMail M", "Cheri Berry", "Chesto Berry", "Pecha Berry", "Rawst Berry", "Aspear Berry", "Leppa Berry",
	"Oran Berry", "Persim Berry", "Lum Berry", "Sitrus Berry", "Figy Berry", "Wiki Berry", "Mago Berry", "Aguav Berry", "Iapapa Berry",
	"Razz Berry", "Bluk Berry", "Nanab Berry", "Wepear Berry", "Pinap Berry", "Pomeg Berry", "Kelpsy Berry", "Qualot Berry",
	"Hondew Berry", "Grepa Berry", "Tamato Berry", "Cornn Berry", "Magost Berry", "Rabuta Berry", "Nomel Berry", "Spelon Berry",
	"Pamtre Berry", "Watmel Berry", "Durin Berry", "Belue Berry", "Occa Berry", "Passho Berry", "Wacan Berry", "Rindo Berry",
	"Yache Berry", "Chople Berry", "Kebia Berry", "Shuca Berry", "Coba Berry", "Payapa Berry", "Tanga Berry", "Charti Berry",
	"Kasib Berry", "Haban Berry", "Colbur Berry", "Babiri Berry", "Chilan Berry", "Liechi Berry", "Ganlon Berry", "Salac Berry",
	"Petaya Berry", "Apicot Berry", "Lansat Berry", "Starf Berry", "Enigma Berry", "Micle Berry", "Custap Berry", "Jaboca Berry",
	"Rowap Berry", "BrightPowder", "White Herb", "Macho Brace", "Exp. Share", "Quick Claw", "Soothe Bell", "Mental Herb", "Choice Band",
	"King's Rock", "SilverPowder", "Amulet Coin", "Cleanse Tag", "Soul Dew", "DeepSeaTooth", "DeepSeaScale", "Smoke Ball", "Everstone",
	"Focus Band", "Lucky Egg", "Scope Lens", "Metal Coat", "Leftovers", "Dragon Scale", "Light Ball", "Soft Sand", "Hard Stone", "Miracle Seed",
	"BlackGlasses", "Black Belt", "Magnet", "Mystic Water", "Sharp Beak", "Poison Barb", "NeverMeltIce", "Spell Tag", "TwistedSpoon",
	"Charcoal", "Dragon Fang", "Silk Scarf", "Up-Grade", "Shell Bell", "Sea Incense", "Lax Incense", "Lucky Punch", "Metal Powder",
	"Thick Club", "Stick", "Red Scarf", "Blue Scarf", "Pink Scarf", "Green Scarf", "Yellow Scarf", "Wide Lens", "Muscle Band", "Wise Glasses",
	"Expert Belt", "Light Clay", "Life Orb", "Power Herb", "Toxic Orb", "Flame Orb", "Quick Powder", "Focus Sash", "Zoom Lens", "Metronome",
	"Iron Ball", "Lagging Tail", "Destiny Knot", "Black Sludge", "Icy Rock", "Smooth Rock", "Heat Rock", "Damp Rock", "Grip Claw",
	"Choice Scarf", "Sticky Barb", "Power Bracer", "Power Belt", "Power Lens", "Power Band", "Power Anklet", "Power Weight", "Shed Shell",
	"Big Root", "Choice Specs", "Flame Plate", "Splash Plate", "Zap Plate", "Meadow Plate", "Icicle Plate", "Fist Plate", "Toxic Plate",
	"Earth Plate", "Sky Plate", "Mind Plate", "Insect Plate", "Stone Plate", "Spooky Plate", "Draco Plate", "Dread Plate", "Iron Plate",
	"Odd Incense", "Rock Incense", "Full Incense", "Wave Incense", "Rose Incense", "Luck Incense", "Pure Incense", "Protector", "Electirizer",
	"Magmarizer", "Dubious Disc", "Reaper Cloth", "Razor Claw", "Razor Fang", "TM01", "TM02", "TM03", "TM04", "TM05", "TM06", "TM07",
	"TM08", "TM09", "TM10", "TM11", "TM12", "TM13", "TM14", "TM15", "TM16", "TM17", "TM18", "TM19", "TM20", "TM21", "TM22", "TM23", "TM24",
	"TM25", "TM26", "TM27", "TM28", "TM29", "TM30", "TM31", "TM32", "TM33", "TM34", "TM35", "TM36", "TM37", "TM38", "TM39", "TM40", "TM41",
	"TM42", "TM43", "TM44", "TM45", "TM46", "TM47", "TM48", "TM49", "TM50", "TM51", "TM52", "TM53", "TM54", "TM55", "TM56", "TM57", "TM58",
	"TM59", "TM60", "TM61", "TM62", "TM63", "TM64", "TM65", "TM66", "TM67", "TM68", "TM69", "TM70", "TM71", "TM72", "TM73", "TM74", "TM75",
	"TM76", "TM77", "TM78", "TM79", "TM80", "TM81", "TM82", "TM83", "TM84", "TM85", "TM86", "TM87", "TM88", "TM89", "TM90", "TM91", "TM92",
	"HM01", "HM02", "HM03", "HM04", "HM05", "HM06", "unknown", "unknown", "Explorer Kit", "Loot Sack", "Rule Book", "Poké Radar", "Point Card",
	"Journal", "Seal Case", "Fashion Case", "Seal Bag", "Pal Pad", "Works Key", "Old Charm", "Galactic Key", "Red Chain", "Town Map",
	"Vs. Seeker", "Coin Case", "Old Rod", "Good Rod", "Super Rod", "Sprayduck", "Poffin Case", "Bicycle", "Suite Key", "Oak's Letter",
	"Lunar Wing", "Member Card", "Azure Flute", "S.S. Ticket", "Contest Pass", "Magma Stone", "Parcel", "Coupon 1", "Coupon 2", "Coupon 3",
	"Storage Key", "SecretPotion", "Vs. Recorder", "Gracidea", "Secret Key", "Apricorn Box", "Unown Report", "Berry Pots", "Dowsing MCHN",
	"Blue Card", "SlowpokeTail", "Clear Bell", "Card Key", "Basement Key", "SquirtBottle", "Red Scale", "Lost Item", "Pass", "Machine Part",
	"Silver Wing", "Rainbow Wing", "Mystery Egg", "Red Apricorn", "Ylw Apricorn", "Blu Apricorn", "Grn Apricorn", "Pnk Apricorn", "Wht Apricorn",
	"Blk Apricorn", "Fast Ball", "Level Ball", "Lure Ball", "Heavy Ball", "Love Ball", "Friend Ball", "Moon Ball", "Sport Ball", "Park Ball", "Photo Album",
	"GB Sounds", "Tidal Bell", "RageCandyBar", "Data Card 01", "Data Card 02", "Data Card 03", "Data Card 04", "Data Card 05", "Data Card 06", "Data Card 07",
	"Data Card 08", "Data Card 09", "Data Card 10", "Data Card 11", "Data Card 12", "Data Card 13", "Data Card 14", "Data Card 15", "Data Card 16", "Data Card 17",
	"Data Card 18", "Data Card 19", "Data Card 20", "Data Card 21", "Data Card 22", "Data Card 23", "Data Card 24", "Data Card 25", "Data Card 26", "Data Card 27",
	"Jade Orb", "Lock Capsule", "Red Orb", "Blue Orb", "Enigma Stone", "Prism Scale", "Eviolite", "Float Stone", "Rocky Helmet", "Air Balloon", "Red Card",
	"Ring Target", "Binding Band", "Absorb Bulb", "Cell Battery", "Eject Button", "Fire Gem", "Water Gem", "Electric Gem", "Grass Gem", "Ice Gem", "Fighting Gem",
	"Poison Gem", "Ground Gem", "Flying Gem", "Psychic Gem", "Bug Gem", "Rock Gem", "Ghost Gem", "Dragon Gem", "Dark Gem", "Steel Gem", "Normal Gem", "Health Wing",
	"Muscle Wing", "Resist Wing", "Genius Wing", "Clever Wing", "Swift Wing", "Pretty Wing", "Cover Fossil", "Plume Fossil", "Liberty Pass", "Pass Orb", "Dream Ball",
	"Poké Toy", "Prop Case", "Dragon Skull", "BalmMushroom", "Big Nugget", "Pearl String", "Comet Shard", "Relic Copper", "Relic Silver", "Relic Gold", "Relic Vase",
	"Relic Band", "Relic Statue", "Relic Crown", "Casteliacone", "Dire Hit 2", "X Speed 2", "X Special 2", "X Sp. Def 2", "X Defend 2", "X Attack 2", "X Accuracy 2",
	"X Speed 3", "X Special 3", "X Sp. Def 3", "X Defend 3", "X Attack 3", "X Accuracy 3", "X Speed 6", "X Special 6", "X Sp. Def 6", "X Defend 6", "X Attack 6", "X Accuracy 6",
	"Ability Urge", "Item Drop", "Item Urge", "Reset Urge", "Dire Hit 3", "Light Stone", "Dark Stone", "TM93", "TM94", "TM95", "Xtransceiver", "God Stone", "Gram 1",
	"Gram 2", "Gram 3", "Xtransceiver", "Medal Box", "DNA Splicers", "DNA Splicers", "Permit", "Oval Charm", "Shiny Charm", "Plasma Card", "Grubby Hanky", "Colress MCHN",
	"Dropped Item", "Dropped Item", "Reveal Glass"
}
		
local movename =
{
	"--" , "Pound", "Karate Chop", "Double Slap", "Comet Punch", "Mega Punch", "Pay Day", "Fire Punch", "Ice Punch", "Thunder Punch", 
	"Scratch", "Vice Grip", "Guillotine", "Razor Wind", "Swords Dance", "Cut", "Gust", "Wing Attack", "Whirlwind", "Fly", 
	"Bind", "Slam", "Vine Whip", "Stomp", "Double Kick", "Mega Kick", "Jump Kick", "Rolling Kick", "Sand Attack", "Headbutt", 
	"Horn Attack", "Fury Attack", "Horn Drill", "Tackle", "Body Slam", "Wrap", "Take Down", "Thrash", "Double-Edge",
	"Tail Whip", "Poison Sting", "Twineedle", "Pin Missile", "Leer", "Bite", "Growl", "Roar", "Sing", "Supersonic", "Sonic Boom", 
	"Disable", "Acid", "Ember", "Flamethrower", "Mist", "Water Gun", "Hydro Pump", "Surf", "Ice Beam", "Blizzard", "Psybeam", 
	"Bubble Beam", "Aurora Beam", "Hyper Beam", "Peck", "Drill Peck", "Submission", "Low Kick", "Counter", "Seismic Toss", 
	"Strength", "Absorb", "Mega Drain", "Leech Seed", "Growth", "Razor Leaf", "Solar Beam", "Poison Powder", "Stun Spore", 
	"Sleep Powder", "Petal Dance", "String Shot", "Dragon Rage", "Fire Spin", "Thunder Shock", "Thunderbolt", "Thunder Wave", 
	"Thunder", "Rock Throw", "Earthquake", "Fissure", "Dig", "Toxic", "Confusion", "Psychic", "Hypnosis", "Meditate", 
	"Agility", "Quick Attack", "Rage", "Teleport", "Night Shade", "Mimic", "Screech", "Double Team", "Recover", "Harden", 
	"Minimize", "Smokescreen", "Confuse Ray", "Withdraw", "Defense Curl", "Barrier", "Light Screen", "Haze", "Reflect",
	"Focus Energy", "Bide", "Metronome", "Mirror Move", "Self-Destruct", "Egg Bomb", "Lick", "Smog", "Sludge", "Bone Club",
	"Fire Blast", "Waterfall", "Clamp", "Swift", "Skull Bash", "Spike Cannon", "Constrict", "Amnesia", "Kinesis", "Soft-Boiled", 
	"High Jump Kick", "Glare", "Dream Eater", "Poison Gas", "Barrage", "Leech Life", "Lovely Kiss", "Sky Attack", "Transform", 
	"Bubble", "Dizzy Punch", "Spore", "Flash", "Psywave", "Splash", "Acid Armor", "Crabhammer", "Explosion", "Fury Swipes", 
	"Bonemerang", "Rest", "Rock Slide", "Hyper Fang", "Sharpen", "Conversion", "Tri Attack", "Super Fang", "Slash", 
	"Substitute", "Struggle", "Sketch", "Triple Kick", "Thief", "Spider Web", "Mind Reader", "Nightmare", "Flame Wheel", 
	"Snore", "Curse", "Flail", "Conversion 2", "Aeroblast", "Cotton Spore", "Reversal", "Spite", "Powder Snow", "Protect", 
	"Mach Punch", "Scary Face", "Feint Attack", "Sweet Kiss", "Belly Drum", "Sludge Bomb", "Mud-Slap", "Octazooka", "Spikes", 
	"Zap Cannon", "Foresight", "Destiny Bond", "Perish Song", "Icy Wind", "Detect", "Bone Rush", "Lock-On", "Outrage", 
	"Sandstorm", "Giga Drain", "Endure", "Charm", "Rollout", "False Swipe", "Swagger", "Milk Drink", "Spark", "Fury Cutter", 
	"Steel Wing", "Mean Look", "Attract", "Sleep Talk", "Heal Bell", "Return", "Present", "Frustration", "Safeguard",
	"Pain Split", "Sacred Fire", "Magnitude", "Dynamic Punch", "Megahorn", "Dragon Breath", "Baton Pass", "Encore", "Pursuit",
	"Rapid Spin", "Sweet Scent", "Iron Tail", "Metal Claw", "Vital Throw", "Morning Sun", "Synthesis", "Moonlight", "Hidden Power", 
	"Cross Chop", "Twister", "Rain Dance", "Sunny Day", "Crunch", "Mirror Coat", "Psych Up", "Extreme Speed", "Ancient Power", 
	"Shadow Ball", "Future Sight", "Rock Smash", "Whirlpool", "Beat Up", "Fake Out", "Uproar", "Stockpile", "Spit Up", 
	"Swallow", "Heat Wave", "Hail", "Torment", "Flatter", "Will-O-Wisp", "Memento", "Facade", "Focus Punch", "Smelling Salts", 
	"Follow Me", "Nature Power", "Charge", "Taunt", "Helping Hand", "Trick", "Role Play", "Wish", "Assist", "Ingrain", 
	"Superpower", "Magic Coat", "Recycle", "Revenge", "Brick Break", "Yawn", "Knock Off", "Endeavor", "Eruption", "Skill Swap", 
	"Imprison", "Refresh", "Grudge", "Snatch", "Secret Power", "Dive", "Arm Thrust", "Camouflage", "Tail Glow", "Luster Purge", 
	"Mist Ball", "Feather Dance", "Teeter Dance", "Blaze Kick", "Mud Sport", "Ice Ball", "Needle Arm", "Slack Off",
	"Hyper Voice", "Poison Fang", "Crush Claw", "Blast Burn", "Hydro Cannon", "Meteor Mash", "Astonish", "Weather Ball", 
	"Aromatherapy", "Fake Tears", "Air Cutter", "Overheat", "Odor Sleuth", "Rock Tomb", "Silver Wind", "Metal Sound",
	"Grass Whistle", "Tickle", "Cosmic Power", "Water Spout", "Signal Beam", "Shadow Punch", "Extrasensory", "Sky Uppercut",
	"Sand Tomb", "Sheer Cold", "Muddy Water", "Bullet Seed", "Aerial Ace", "Icicle Spear", "Iron Defense", "Block", "Howl",
	"Dragon Claw", "Frenzy Plant", "Bulk Up", "Bounce", "Mud Shot", "Poison Tail", "Covet", "Volt Tackle", "Magical Leaf",
	"Water Sport", "Calm Mind", "Leaf Blade", "Dragon Dance", "Rock Blast", "Shock Wave", "Water Pulse", "Doom Desire",
	"Psycho Boost", "Roost", "Gravity", "Miracle Eye", "Wake-Up Slap", "Hammer Arm", "Gyro Ball", "Healing Wish", "Brine",
	"Natural Gift", "Feint", "Pluck", "Tailwind", "Acupressure", "Metal Burst", "U-turn", "Close Combat", "Payback", "Assurance", 
	"Embargo", "Fling", "Psycho Shift", "Trump Card", "Heal Block", "Wring Out", "Power Trick", "Gastro Acid", "Lucky Chant", 
	"Me First", "Copycat", "Power Swap", "Guard Swap", "Punishment", "Last Resort", "Worry Seed", "Sucker Punch", "Toxic Spikes",
	"Heart Swap", "Aqua Ring", "Magnet Rise", "Flare Blitz", "Force Palm", "Aura Sphere", "Rock Polish", "Poison Jab", 
	"Dark Pulse", "Night Slash", "Aqua Tail", "Seed Bomb", "Air Slash", "X-Scissor", "Bug Buzz", "Dragon Pulse", "Dragon Rush", 
	"Power Gem", "Drain Punch", "Vacuum Wave", "Focus Blast", "Energy Ball", "Brave Bird", "Earth Power", "Switcheroo",
	"Giga Impact", "Nasty Plot", "Bullet Punch", "Avalanche", "Ice Shard", "Shadow Claw", "Thunder Fang", "Ice Fang", "Fire Fang", 
	"Shadow Sneak", "Mud Bomb", "Psycho Cut", "Zen Headbutt", "Mirror Shot", "Flash Cannon", "Rock Climb", "Defog",
	"Trick Room", "Draco Meteor", "Discharge", "Lava Plume", "Leaf Storm", "Power Whip", "Rock Wrecker", "Cross Poison", "Gunk Shot", 
	"Iron Head", "Magnet Bomb", "Stone Edge", "Captivate", "Stealth Rock", "Grass Knot", "Chatter", "Judgment", "Bug Bite", 
	"Charge Beam", "Wood Hammer", "Aqua Jet", "Attack Order", "Defend Order", "Heal Order", "Head Smash", "Double Hit",
	"Roar of Time", "Spacial Rend", "Lunar Dance", "Crush Grip", "Magma Storm", "Dark Void", "Seed Flare", "Ominous Wind", "Shadow Force",

	"Hone Claws", "Wide Guard", "Guard Split", "Power Split", "Wonder Room", "Psyshock", "Venoshock", "Autotomize", "Rage Powder",
	"Telekinesis", "Magic Room", "Smack Down", "Storm Throw", "Flame Burst", "Sludge Wave", "Quiver Dance", "Heavy Slam",
	"Synchronoise", "Electro Ball", "Soak", "Flame Charge", "Coil", "Low Sweep", "Acid Spray", "Foul Play", "Simple Beam",
	"Entrainment", "After You", "Round", "Echoed Voice", "Chip Away", "Clear Smog", "Stored Power", "Quick Guard", "Ally Switch",
	"Scald", "Shell Smash", "Heal Pulse", "Hex", "Sky Drop", "Shift Gear", "Circle Throw", "Incinerate", "Quash", "Acrobatics",
	"Reflect Type", "Retaliate", "Final Gambit", "Bestow", "Inferno", "Water Pledge", "Fire Pledge", "Grass Pledge",
	"Volt Switch", "Struggle Bug", "Bulldoze", "Frost Breath", "Dragon Tail", "Work Up", "Electroweb", "Wild Charge",
	"Drill Run", "Dual Chop", "Heart Stamp", "Horn Leech", "Sacred Sword", "Razor Shell", "Heat Crash", "Leaf Tornado",
	"Steamroller", "Cotton Guard", "Night Daze", "Psystrike", "Tail Slap", "Hurricane", "Head Charge", "Gear Grind",
	"Searing Shot", "Techno Blast", "Relic Song", "Secret Sword", "Glaciate", "Bolt Strike", "Blue Flare", "Fiery Dance",
	"Freeze Shock", "Ice Burn", "Snarl", "Icicle Crash", "V-create", "Fusion Flare", "Fusion Bolt"
}

local xfix = 10 + 256
local yfix = 10 + 192
function displaybox(a, b, c, d, e, f)
	gui.drawBox(a + xfix, b + yfix, c + xfix, d + yfix, f, e)
end

function display(a, b, c, d)
	local scale = client.getwindowsize()
	gui.text((xfix + a) * scale, (yfix + b) * scale, c, d, "topleft")
end

function drawarrowleft(a, b, c)
	gui.drawLine(a + xfix, b + yfix + 3, a + 2 + xfix, b + 5 + yfix, c)
	gui.drawLine(a + xfix, b + yfix + 3, a + 2 + xfix, b + 1 + yfix, c)
	gui.drawLine(a + xfix, b + yfix + 3, a + 6 + xfix, b + 3 + yfix, c)
end

function drawarrowright(a, b, c)
	gui.drawLine(a + xfix, b + yfix + 3, a - 2 + xfix, b + 5 + yfix, c)
	gui.drawLine(a + xfix, b + yfix + 3, a - 2 + xfix, b + 1 + yfix, c)
	gui.drawLine(a + xfix, b + yfix + 3, a - 6 + xfix, b + 3 + yfix, c)
end

function mult32(a,b)
	return (a * b) & 0xFFFFFFFF
end

function getbits(a,b,d)
	return rshift(a, b) & (lshift(1, d) - 1)
end

function gettop(a)
	return rshift(a, 16) & 0xFFFF
end

function menu()
	tabl = input.get()
	leftarrow1color = "white"
	leftarrow2color = "white"
	rightarrow1color = "white"
	rightarrow2color = "white"
	if tabl["Number3"] then
		leftarrow2color = "yellow"
	end
	if tabl["Number4"] then
		rightarrow2color = "yellow"
	end
	if tabl["Number1"] then
		leftarrow1color = "yellow"
	end
	if tabl["Number2"] then
		rightarrow1color = "yellow"
	end
	if tabl["Number3"] and not prev["Number3"] and mode < 5 then
		submode = submode - 1
		if submode == 0 then
			submode = submodemax
		end
	end
	if tabl["Number4"] and not prev["Number4"] and mode < 5 then
		submode = submode + 1
		if submode == submodemax + 1 then
			submode = 1
		end
	end
	if tabl["Number1"] and not prev["Number1"] then
		mode = mode - 1
		if mode == 0 then
			mode = modemax
		end
	end
	if tabl["Number2"] and not prev["Number2"] then
		mode = mode + 1
		if mode == modemax + 1 then
			mode = 1
		end
	end
	prev = tabl
	if mode == 1 then
		modetext = "Party"
	elseif mode == 2 then
		modetext = "Enemy"
	elseif mode == 3 then
		modetext = "Enemy 2"
	elseif mode == 4 then
		modetext = "Partner"
	else -- mode == 5
		modetext = "Wild"
	end
end

function getPidAddr()
	if mode == 5 then
		return 0x02259DD8
	elseif mode == 4 then
		return 0x0226B7B4 + 0xDC*(submode-1)
	elseif mode == 3 then
		return 0x0226C274 + 0xDC*(submode-1)
	elseif mode == 2 then
		return 0x0226ACF4 + 0xDC*(submode-1)
	else -- mode 1
		return 0x022349B4 + 0xDC*(submode-1) 
	end
end

function getNatClr(a)
	color = "yellow"
	if nat % 6 == 0 then
		color = "yellow"
	elseif a == "atk" then
		if nat < 5 then
			color = "#FF80FFFF"
		elseif nat % 5 == 0 then
			color = "red"
		end
	elseif a == "def" then
		if nat > 4 and nat < 10 then
			color = "#FF80FFFF"
		elseif nat % 5 == 1 then
			color = "red"
		end
	elseif a == "spe" then
		if nat > 9 and nat < 15 then
			color = "#FF80FFFF"
		elseif nat % 5 == 2 then
			color = "red"
		end
	elseif a == "spa" then
		if nat > 14 and nat < 20 then
			color = "#FF80FFFF"
		elseif nat % 5 == 3 then
			color = "red"
		end
	elseif a == "spd" then
		if nat > 19 then
			color = "#FF80FFFF"
		elseif nat % 5 == 4 then
			color = "red"
		end
	end
	return color
end
 
local function main_fn()
	menu()

	pidAddr = getPidAddr()
	pid = mdword(pidAddr)
	checksum = mword(pidAddr + 6)
	shiftvalue = (rshift((bnd(pid,0x3E000)),0xD)) % 24

	BlockAoff = (BlockA[shiftvalue + 1] - 1) * 32
	BlockBoff = (BlockB[shiftvalue + 1] - 1) * 32
	BlockCoff = (BlockC[shiftvalue + 1] - 1) * 32
	BlockDoff = (BlockD[shiftvalue + 1] - 1) * 32

	-- Block A
	prng = checksum
	for i = 1, BlockA[shiftvalue + 1] - 1 do
		prng = mult32(prng,0x5F748241) + 0xCBA72510 -- 16 cycles
	end

	prng = mult32(prng,0x41C64E6D) + 0x6073
	pokemonID = bxr(mword(pidAddr + BlockAoff + 8), gettop(prng))
	if pokemonID > 651 then
		pokemonID = -1 -- (pokemonID = -1 indicates invalid data)
	end

	prng = mult32(prng,0x41C64E6D) + 0x6073
	heldItem = bxr(mword(pidAddr + BlockAoff + 2 + 8), gettop(prng))
	if heldItem > 639 then
		pokemonID = -1 -- (pokemonID = -1 indicates invalid data)
	end

	prng = mult32(prng,0x41C64E6D) + 0x6073
	OTID = bxr(mword(pidAddr + BlockAoff + 4 + 8), gettop(prng))
	prng = mult32(prng,0x41C64E6D) + 0x6073
	OTSID = bxr(mword(pidAddr + BlockAoff + 6 + 8), gettop(prng))

	prng = mult32(prng,0x41C64E6D) + 0x6073
	prng = mult32(prng,0x41C64E6D) + 0x6073
	prng = mult32(prng,0x41C64E6D) + 0x6073
	ability = bxr(mword(pidAddr + BlockAoff + 12 + 8), gettop(prng))
	friendship_or_steps_to_hatch = getbits(ability, 0, 8)
	ability = getbits(ability, 8, 8)
	if ability > 164 then
		pokemonID = -1
	end
	prng = mult32(prng,0x41C64E6D) + 0x6073
	prng = mult32(prng,0x41C64E6D) + 0x6073
	evs[1] = bxr(mword(pidAddr + BlockAoff + 16 + 8), gettop(prng))
	prng = mult32(prng,0x41C64E6D) + 0x6073
	evs[2] = bxr(mword(pidAddr + BlockAoff + 18 + 8), gettop(prng))
	prng = mult32(prng,0x41C64E6D) + 0x6073
	evs[3] = bxr(mword(pidAddr + BlockAoff + 20 + 8), gettop(prng))

	hpev =  getbits(evs[1], 0, 8)
	atkev = getbits(evs[1], 8, 8)
	defev = getbits(evs[2], 0, 8)
	speev = getbits(evs[2], 8, 8)
	spaev = getbits(evs[3], 0, 8)
	spdev = getbits(evs[3], 8, 8)

	-- Block B
	prng = checksum
	for i = 1, BlockB[shiftvalue + 1] - 1 do
		prng = mult32(prng,0x5F748241) + 0xCBA72510 -- 16 cycles
	end

	prng = mult32(prng,0x41C64E6D) + 0x6073
	move[1] = bxr(mword(pidAddr + BlockBoff + 8), gettop(prng))
	if move[1] > 559 then
		pokemonID = -1
	end
	prng = mult32(prng,0x41C64E6D) + 0x6073
	move[2] = bxr(mword(pidAddr + BlockBoff + 2 + 8), gettop(prng))
	if move[2] > 559 then
		pokemonID = -1
	end
	prng = mult32(prng,0x41C64E6D) + 0x6073
	move[3] = bxr(mword(pidAddr + BlockBoff + 4 + 8), gettop(prng))
	if move[3] > 559 then
		pokemonID = -1
	end
	prng = mult32(prng,0x41C64E6D) + 0x6073
	move[4] = bxr(mword(pidAddr + BlockBoff + 6 + 8), gettop(prng))
	if move[4] > 559 then
		pokemonID = -1
	end
	prng = mult32(prng,0x41C64E6D) + 0x6073
	moveppaux = bxr(mword(pidAddr + BlockBoff + 8 + 8), gettop(prng))
	movepp[1] = getbits(moveppaux,0,8)
	movepp[2] = getbits(moveppaux,8,8)
	prng = mult32(prng,0x41C64E6D) + 0x6073
	moveppaux = bxr(mword(pidAddr + BlockBoff + 10 + 8), gettop(prng))
	movepp[3] = getbits(moveppaux,0,8)
	movepp[4] = getbits(moveppaux,8,8)

	prng = mult32(prng,0x41C64E6D) + 0x6073
	prng = mult32(prng,0x41C64E6D) + 0x6073

	prng = mult32(prng,0x41C64E6D) + 0x6073
	ivspart[1] = bxr(mword(pidAddr + BlockBoff + 16 + 8), gettop(prng))
	prng = mult32(prng,0x41C64E6D) + 0x6073
	ivspart[2] = bxr(mword(pidAddr + BlockBoff + 18 + 8), gettop(prng))
	ivs = ivspart[1] + lshift(ivspart[2],16)

	hpiv  = getbits(ivs,0,5)
	atkiv = getbits(ivs,5,5)
	defiv = getbits(ivs,10,5)
	speiv = getbits(ivs,15,5)
	spaiv = getbits(ivs,20,5)
	spdiv = getbits(ivs,25,5)
	isegg = getbits(ivs,30,1)

	-- Nature for gen 5
	prng = mult32(prng,0x41C64E6D) + 0x6073
	prng = mult32(prng,0x41C64E6D) + 0x6073
	prng = mult32(prng,0x41C64E6D) + 0x6073
	nat = bxr(mword(pidAddr + BlockBoff + 24 + 8), gettop(prng))
	nat = getbits(nat,8,8)
	if nat > 24 then
		pokemonID = -1
	end

	-- Block D
	prng = checksum
	for i = 1, BlockD[shiftvalue + 1] - 1 do
		prng = mult32(prng,0x5F748241) + 0xCBA72510 -- 16 cycles
	end

	prng = mult32(prng,0xCFDDDF21) + 0x67DBB608 -- 8 cycles
	prng = mult32(prng,0xEE067F11) + 0x31B0DDE4 -- 4 cycles
	prng = mult32(prng,0x41C64E6D) + 0x6073
	prng = mult32(prng,0x41C64E6D) + 0x6073
	pkrs = bxr(mword(pidAddr + BlockDoff + 0x1A + 8), gettop(prng))
	pkrs = getbits(pkrs,0,8)

	-- Current stats
	prng = pid
	prng = mult32(prng,0x41C64E6D) + 0x6073
	prng = mult32(prng,0x41C64E6D) + 0x6073
	prng = mult32(prng,0x41C64E6D) + 0x6073
	level = getbits(bxr(mword(pidAddr + 0x8C), gettop(prng)),0,8)
	prng = mult32(prng,0x41C64E6D) + 0x6073
	hpstat = bxr(mword(pidAddr + 0x8E), gettop(prng))
	prng = mult32(prng,0x41C64E6D) + 0x6073
	maxhpstat = bxr(mword(pidAddr + 0x90), gettop(prng))
	prng = mult32(prng,0x41C64E6D) + 0x6073
	atkstat = bxr(mword(pidAddr + 0x92), gettop(prng))
	prng = mult32(prng,0x41C64E6D) + 0x6073
	defstat = bxr(mword(pidAddr + 0x94), gettop(prng))
	prng = mult32(prng,0x41C64E6D) + 0x6073
	spestat = bxr(mword(pidAddr + 0x96), gettop(prng))
	prng = mult32(prng,0x41C64E6D) + 0x6073
	spastat = bxr(mword(pidAddr + 0x98), gettop(prng))
	prng = mult32(prng,0x41C64E6D) + 0x6073
	spdstat = bxr(mword(pidAddr + 0x9A), gettop(prng))

	-- Calculate Hidden Power
	hiddentype = math.floor(((hpiv % 2) + 2*(atkiv % 2) + 4*(defiv % 2) + 8*(speiv % 2) + 16*(spaiv % 2) + 32*(spdiv % 2))*15 / 63)
	hiddenpower = 30 + math.floor(((rshift(hpiv,1) % 2) + 2*(rshift(atkiv,1) % 2) + 4*(rshift(defiv,1) % 2)
					+ 8*(rshift(speiv,1) % 2) + 16*(rshift(spaiv,1) % 2) + 32*(rshift(spdiv,1) % 2)) * 40 / 63)

	-- Display data
	displaybox(-5, -5, 240, 175, "#FF0000A0", "white")

	display(180, 0, "Black", "#FF88FFFF")
	display(182, 20, "mode", "#FF88FFFF")

	drawarrowleft(98 - math.floor(string.len(modetext)/2) * 6,0, leftarrow1color)
	display(112 - math.floor(string.len(modetext)/2) * 6,0, modetext)
	drawarrowright(133 + math.floor(string.len(modetext)/2) * 6,0, rightarrow1color)
	if (mode < 5) then
		drawarrowleft(100,10, leftarrow2color)
		display(100,10, "  " .. submode)
		drawarrowright(130,10, rightarrow2color)
	end

	if pokemonID == -1 then
		display(55,30, "Invalid Pokemon Data", "red")
	else
		if isegg == 1 then
			display(0,25, "Pokemon: " .. pokemon[pokemonID + 1] .. " egg", "yellow")
		else
			display(0,25, "Pokemon: " .. pokemon[pokemonID + 1], "yellow")
		end
		display(0,35, "PID : " .. tohex(pid), "magenta")
		display(0,45, "Item: " .. item_gen5[heldItem + 1], "white")
		display(0,55, "OT  ID: " .. OTID, "orange")
		display(0,65, "OT SID: " .. OTSID, "cyan")
		display(0,75, "Nature: " .. nature[nat + 1], "aquamarine")
		display(0,85, "Ability: " .. abilities[ability + 1])

		display(140,30, "Level: " .. level, "lime")
		if mode == 1 then
			display(140,40, "HP: " .. hpstat .. "/" .. maxhpstat, "lime")
		end

		if pkrs == 0 then
			display(140,50, "PKRS: no", "red")
		else
			display(140,50, "PKRS: yes (" .. pkrs .. ")", "red")
		end
		display(140,60, "Hidden Power: ", "cyan")
		display(140,70, pkmntype[hiddentype+1] .. " " .. hiddenpower, "cyan")
		if isegg == 0 then
			display(140,80, "Friendship: " .. friendship_or_steps_to_hatch, "orange")
		else
			display(140,80, "Steps to hatch: ", "orange")
			display(140,90, friendship_or_steps_to_hatch * 256 .. "-" .. (friendship_or_steps_to_hatch + 1) * 256 .. " steps", "orange")
		end

		display(0,115, "HP", "yellow")
		display(0,125,"ATK", getNatClr("atk"))
		display(0,135,"DEF", getNatClr("def"))
		display(0,145,"SAT", getNatClr("spa"))
		display(0,155,"SDF", getNatClr("spd"))
		display(0,165,"SPE", getNatClr("spe"))

		display(30,105, "IV", "white")
		display(30,115, hpiv, "yellow")
		display(30,125, atkiv, getNatClr("atk"))
		display(30,135, defiv, getNatClr("def"))
		display(30,145, spaiv, getNatClr("spa"))
		display(30,155, spdiv, getNatClr("spd"))
		display(30,165, speiv, getNatClr("spe"))

		display(55,105, "EV", "white")
		display(55,115, hpev, "yellow")
		display(55,125, atkev, getNatClr("atk"))
		display(55,135, defev, getNatClr("def"))
		display(55,145, spaev, getNatClr("spa"))
		display(55,155, spdev, getNatClr("spd"))
		display(55,165, speev, getNatClr("spe"))

		display(80,105, "STAT", "white")
		display(80,115, maxhpstat, "yellow")
		display(80,125, atkstat, getNatClr("atk"))
		display(80,135, defstat, getNatClr("def"))
		display(80,145, spastat, getNatClr("spa"))
		display(80,155, spdstat, getNatClr("spd"))
		display(80,165, spestat, getNatClr("spe"))

		display(110,105, "  MOVES", "white")
		display(110,115,  "1.".. movename[move[1] + 1], "yellow")
		display(110,125, "2.".. movename[move[2] + 1], "yellow")
		display(110,135, "3.".. movename[move[3] + 1], "yellow")
		display(110,145, "4.".. movename[move[4] + 1], "yellow")

		display(210,105,  "PP", "white")
		display(210,115, movepp[1], "yellow")
		display(210,125, movepp[2], "yellow")
		display(210,135, movepp[3], "yellow")
		display(210,145, movepp[4], "yellow")
	end
end

while true do
	main_fn()
	emu.frameadvance()
end
