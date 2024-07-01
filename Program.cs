
// I think the dagger is not properly removed from inventory                                    fixed
// not good, the additional item lists need to be removed when get by the hero and monsters,    fixed
// here there is 6 when there should be 4                                                       fixed

// we need to fix No items text position done                         fixed and renovated
// gear is not displayed when empty, items are displayed when empty   changed
// when going out of bonds it doesn't throw invalid choice            to check, not an issue anymore
// we need to clear the menu properly                                 done

// Bug, sometimes characters with 0 health do not get removed, this only happens with poisoned characters

// Rundown at everything, refactoring
// add namespaces
// finish

Game game = new Game();
game.Run();