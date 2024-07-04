using TheFinalBattle.TurnSystem;
using TheFinalBattle.GameObjects;
using TheFinalBattle.InformationDisplay;
using TheFinalBattle.PartyManagement;
using TheFinalBattle.GameObjects.MenuActions;

namespace TheFinalBattle;

public class Game
{
    public void Run()
    {
        PartyManager party = new PartyManager();
        TurnManager turn = new TurnManager(party);
        DisplayInformation info = new DisplayInformation(turn, party);

        List<MenuOption> menuList = 
            info.DefineMenuToDisplay(new ComputerVsComputer(), new PlayerVsComputer(), new PlayerVsPlayer());

        party.SetUpParties(menuList, info, turn);        

        while (!party.CheckForEmptyParties())
        {
            turn.CurrentPartyTurnSetUp(party);
            turn.RunCurrentParty(party, info);
            turn.CheckForNextRound(party);
            turn.AdvanceToNextParty();
        }
        info.UpdateTurnDisplay(turn, party);
        EndOfProgramCursorPosition();
    }

    void EndOfProgramCursorPosition() => Console.SetCursorPosition(0, 30);
}