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

        party.SetUpParties(menuList, info, turn);        

        while (!party.CheckForEmptyParties())
        {
            turn.PartyTurnSetUp(party);

            turn.RunCurrentParty(turn, info, party);
            turn.CheckForNextRound(party);
            turn.AdvanceToNextParty();

            CheckHeroLost();
        }

        void CheckHeroLost()
        {
            if (party.IsPartyEmpty(party.HeroPartyList))
                info.OnDisplayBattleEnd(party, turn);
        }
    }
}