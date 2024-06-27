// More:
// Heroes
// • Let gear provide offensive and defensive attack modifiers. done

// • More monster types.
// amarok, Bite, has the rotten side effect, chance to make the opponent skip a turn
// manticore, has a slice attack, 2 damage
// evil robot, Smart Misile attack, that attacks at everyone in the party for 1 damage

Game game = new Game();
game.Run();

// Bug, souls only updates when the party dies, not when something dies (display souls value of creatures, not a bug)
// Bug, turn skipped not displayed in logger                               fixed

// Console goes down a bit                                                 pseudo-fixed? hacky solution
// add target numbers                                                      done