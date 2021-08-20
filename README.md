# Donut Heaven

"The ANGELS of PLANET EA█T█ are a lot sweeter than you'd think! Help FALLEN ANGEL SPRINKLE on a delicious and divine adventure to get back home to DONUT HEAVEN."

For AGBIC 2021: https://itch.io/jam/a-game-by-its-cover-2021

Concept by Alice Pedersen ([@bakumoe](https://twitter.com/bakumoe)): https://famicase.com/21/softs/010.html

## Credits

- Concept: Alice Pedersen ([@bakumoe](https://twitter.com/bakumoe))
- Design, programming, art: Kai Kuehner
- Sound:
	- Jump, hurt, attack by cicifyre: https://opengameart.org/content/female-rpg-voice-starter-pack
	- Flaps by AgentDD: https://freesound.org/people/AgentDD/sounds/246225/ https://freesound.org/people/AgentDD/sounds/246224/
	- Slam by studiomandragore: https://freesound.org/people/studiomandragore/sounds/401630/
	- Whoosh by man: https://freesound.org/people/man/sounds/14609/
	- Wind by Divinux: https://freesound.org/people/Divinux/sounds/254642/
	- Shatter by ngruber: https://freesound.org/people/ngruber/sounds/204777/
- Music:
	- Soliloquy by Matthew Pablo: https://opengameart.org/content/soliloquy
	- Heroic Demise by Matthew Pablo: https://opengameart.org/content/heroic-demise-hero-theme

## TODO
- Bugs
	- fix occasional camera jitter?
		- occasionally deactivate+reactivate vcam?
- Mechanics
	- Hazards
		- On touch player- play hurt sound, respawn at last checkpoint
		- Candy cane spikes?
		- Falling things?
	- Enemies
		- Movement patterns
			- Walk back and forth on platform- turn around if hit wall or edge
			- Walk along all edges of platform
		- Slam/dash combat
			- Bounce away only for enemies with multiple HP
	- Recharge all crystals whenever touching ground?
	- Show collected sprinkles (some total in each level)
- Art
	- Level
		- Tiles
		- Cotton candy bouncy clouds
		- Double jump refill candy (improve)
		- Decorations
			- Lollipops
			- Gumdrops
			- Swirly brown sticks
			- Peppermint swirls
			- Candy canes
	- Enemies
		- Jelly slime
		- Licorice spiky bug
		- other candy demon guys
	- Menu
		- Font
		- Title screen
		- Buttons
		- Intro cutscene
		- Boss conversation
		- End cutscene
		- End screen
	- Gray out wings if can't double jump, gray out donut halo if can't dash (separate out donut halo, duplicate fall/slam/dash sprites)
	- Player start slam animation
- Sound
	- Music
		- Title
		- Levels
		- End
	- Walk (step)
	- Land/hit ceiling/hit wall (impact)
	- Refill jump/dash crystal
	- Collect sprinkle
	- Add earthquake rumble to slam loop?
	- Enemy
- Juice
	- Slam particles come from collision point
		- take color of terrain?
	- Bouncers jiggle when bounced on
	- Dash start effect
	- More dash/slam trail effects?
- Levels
	- Level 1- Frosted Fields
		- Terrain- light brown gingerbread with pink/blue frosting; donuts; light blue background with hills
		- Introduce jump
		- Introduce double jump
		- Introduce static hazards
		- Introduce bouncers
	- Level 2- Jelly Swamp (patches of bouncy floor)
		- Terrain- dark brown with light brown frosting; all-red jelly patches; purple background with trees
		- Introduce slam break floor
		- Introduce slam bounce (on jelly floors)
		- Introduce combat
	- Level 3- Lollipop Forest (lollipop & candycane trees)
		- Terrain- pink with dark brown frosting, pink background with trees
		- Introduce slam bounce (on round bouncers)
		- Introduce falling hazards
		- Introduce double jump refill
	- Level 4- Cake Cliffs
		- Terrain- red with white frosting, blue-green background with cake mountains
		- Introduce dash
		- Introduce dash break wall
		- Introduce dash combat?
		- Introduce dash bounce
		- Introduce dash refill crystal
	- another level if I have time
	- Level 5- Chocolate Mountain (vertical climb, lava rivers)
		- Terrain- brown chocolate squares; dark brown lava rivers, yellow background
		- Dash through multiple barriers hanging off ceiling
	- Level 6- Donut Heaven
		- Outside terrain- cotton candy clouds, donuts, dark blue space background w/ stars (music: Soliloquy)
		- Inside terrain- dark brown with pink frosting, light brown walls (music: Heroic Demise)
		- Castle
		- Boss- Lord Licorice
	- keep momentum- speedrun routes
	- secret sprinkles in early levels where you have to use future mechanics
- Ending
	- other angels: frosting (dark skin pink hair), cinnamon
	- special plumpy ending for collected all sprinkles