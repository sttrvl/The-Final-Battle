// there is a current bug going on where if I use MyLara and Skoring name (long name)
// the program has a problem (there was no space)
// add something that puts a WriteLine or puts cursor one down and to the beginning when that happens   done
// The problem of MyLara names is due to how  my ":" spacing is calculated fixed,                       solved
// added text wrapping for logger                                                                       done


// Characters should heal more                      
// Rot turn skip is not working as intended, turns skip but weirdly   solved done
// Bug, end message is not displaying?                                solved done

// Add default name for computer as TrueProgrammer
// Add better colors. One for each thing. No repeating.
// after that, just refactoring
// Console goes down a bit                                                 pseudo-fixed? hacky solution

// when the battle ends it doesn't get the chance to print log messages

Game game = new Game();
game.Run();