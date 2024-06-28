// More:
// Heroes
// • Let gear provide offensive and defensive attack modifiers. done

// • More monster types.
// amarok, Bite, has the rotten side effect, chance to make the opponent skip a turn
// manticore, has a slice attack, 2 damage
// evil robot, Smart Misile attack, that attacks at everyone in the party for 1 damage

Game game = new Game();
game.Run();

// Bug, souls only updates when the party dies, not when something dies (display souls value of creatures, not a bug) done, clarified
// Bug, turn skipped not displayed in logger                               fixed

// Console goes down a bit                                                 pseudo-fixed? hacky solution
// add target numbers                                                      done

// Hero's Obtained message at the end should show in the logger                                 done, made new events
// Some lines of the UI, should be written with setCursor so they do not move                   done
// ObjectSight, TrueProgrammer vs Uncoded, programmer is getting 2 point heals each hit wtf     done

/*
Choose what to do must be fixed
Choose action must be fixed

Choose target must be fixed

Choose item must be fixed
number too
 */

// Add better colors. One for each thing. No repeating.