using System.IO;

public class Game
{
    public void Run()
    {
        PartyManager party = new PartyManager();
        TurnManager turn = new TurnManager(party);
        DisplayInformation info = new DisplayInformation(party);

        List<MenuOption> menuList = new List<MenuOption>()
        {
            new ComputerVsComputer(),
            new PlayerVsComputer(),
            new PlayerVsPlayer()
        };

        party.SetUpParties(menuList, info);        

        while (!party.CheckForEmptyParties())
        {
            turn.PartyTurnSetUp(party);

            turn.RunCurrentParty(turn, info, party);
            turn.NextRound();

            CheckHeroLost();
        }

        void CheckHeroLost()
        {
            if (party.IsPartyEmpty(party.HeroPartyList))
                info.OnDisplayBattleEnd(party, turn);
        }
    }
}