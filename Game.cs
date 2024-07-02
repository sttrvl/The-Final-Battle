public class Game
{
    public void Run()
    {
        PartyManager party = new PartyManager();
        TurnManager turn = new TurnManager(party);
        DisplayInformation info = new DisplayInformation(party, turn);

        List<MenuOption> menuList = new List<MenuOption>()
        {
            new ComputerVsComputer(),
            new PlayerVsComputer(),
            new PlayerVsPlayer()
        };

        party.SetUpParties(menuList, info, turn, party);        

        while (!party.CheckForEmptyParties())
        {
            
            turn.PartyTurnSetUp(party);

            turn.RunCurrentParty(turn, info, party);
            turn.CheckForNextRound(turn, party);
            
            turn.AdvanceToNextParty();
        }
        info.DisplayCharacterTurnText(party, turn);
        EndOfProgramCursorPosition();
    }

    void EndOfProgramCursorPosition() => Console.SetCursorPosition(0, 30);
}