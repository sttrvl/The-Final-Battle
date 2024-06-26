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

// TODO:
// • More heroes. You could add Mylara and Skorin, whose gear is the Cannon of Consolas                         done
// deals 1 damage most of the time, 2 damage every multiple of 3 or 5, and 5 damage every multiple of 3 and 5.  done
// • Add more item types. Simula’s soup is a full heal, restoring HP to its maximum.                            done
// • Add offensive attack modifiers.                                                                            done

// More:
// Heroes
// Offensive Attack modifiers.
// • Let gear provide offensive and defensive attack modifiers.
// • More monster types.


// • Let characters equip more than one piece of gear.  done
// - Binary Helm, reduces all damage by 1.              done
// We need to display current armor                     done
// Display the equipped message                         done

// • Display a small indicator in the status display that gives you a warning if a character is close to death.     done
// Perhaps a yellow [!] if the character is under 25 % and a red[!] if the character is under 10%                   done

// • Experience Points. As heroes defeat monsters, give them XP to track their accomplishments. doing
// When a monster is defeated, it gives a certain amount of xp
// add xp to monsters, each monster drop souls.                        done
// heroes also have                      souls, they start with 0      done
// we need to sum the value of souls to the selected character.        done
// if they have more than 3, they their next attack is supercharged.   done
// Bug, important, current attack that misses keeps doing damage.      fixed! (took me 2 mins lol)
// heroes drop soul                                                    idea scrapped

// • Add attack side effects, which allow an attack to run other logic when an attack happens.     // done
// Add Shadow Octopoid                                                                                       // done
// has grapple attack, with a chance of stealing equipped gear to monster inventory.                         // done
// Has another attack, whip, that poisons, 1 damage each turn, for three turns.                              //

// we need to add logic for additional standard attack                                                       // done

// • Allow characters to have temporary effects applied to them.                                             // done
// - poison effect that deals 1 damage per turn for thre turns.                                              // done

// Later:
// • Allow characters to taunt their enemies with messages:
// when the character appears for the first time, during their turn:
// Uncoded One saying <<THE UNRAVELLING OF ALL THINGS IS INEVITABLE>>           // done
// skeleton saying, “We will repel your spineless assault!”                     // done

// added the taunts                             // done
// need a system to check for them              // done

// • Load levels from a file instead of hardcoding them in the game. done

// make everything as general as possible
// Change: another approach for turns
// Change: a way to use MenuItem idea

// Bugs:
// bots are able to use nothing, this is not a big problem but makes the game a bit dull with 2 "skips"     done
// After someone with gear gets attacked, geal is instantly stolen                                          done
// attack modifiers still display even though sometimes the damage is 0  (not a problem anymore since nothing is not an option)
// ^ should sort it out anyways, is easy
// bite displays as nothing, it does not do any damage either                                               fixed

// Next thing I want to do is making a log for things happening, currently can't see well. 

// Extra:

// Done:

// Change
// I could make all the input parses from switches become just thing.Name, and just give them names directly
// need to rework attack actions, using enums like I did was not correct
// Maybe instead of printing all the info out I could sum it each turn to make a cool display
// We need to change tuples to a record, since they are long lived, like the one we got with menu

Game game = new Game();
game.Run();