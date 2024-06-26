// PartyManager:
// manages party's, their types, their characters

// Game:
// assembles everything together, and puts it in a good starting state, can have some helper methods that use other classes.

// TurnManager:
// manages anything related with turns

// IAction
// used to create new actions and interactions of something to the game

// Input manager:
// manages anything related with any sort of input

// ----
// More:
// Heroes
// Offensive Attack modifiers.
// • Let gear provide offensive and defensive attack modifiers.
// • More monster types.

// TODO:
// Next thing I want to do is making a log for things happening, currently can't see well. 

// Bugs:
// attack modifiers still display even though sometimes the damage is 0  (not a problem anymore since nothing is not an option)
// ^ should sort it out anyways, is easy

// Change
// I could make all the input parses from switches become just thing.Name, and just give them names directly
// need to rework attack actions, using enums like I did was not correct
// Maybe instead of printing all the info out I could sum it each turn to make a cool display
// We need to change tuples to a record, since they are long lived, like the one we got with menu
// a way to use MenuItem idea
// another approach for turns
// make everything as general as possible

Game game = new Game();
game.Run();