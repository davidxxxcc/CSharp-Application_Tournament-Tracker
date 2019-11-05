using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        // Order our list randomly of teams
        // Check if it is big enough - if not, add in byes  - 2*2*2*2 - 2^4
        // Create our first round of matchups
        // Create everyround after that - 8 matchups - 4 matchups - 2 matchups - 1 matchup

        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
            return teams.OrderBy(x => Guid.NewGuid()).ToList();
        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            List<MatchupModel> toScore = new List<MatchupModel>();

            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel rm in round)
                {
                    if (rm.Winner == null && (rm.Entries.Any(x => x.Score != 0) || rm.Entries.Count == 1))
                    {
                        toScore.Add(rm);
                    }
                }
            }

            ScoreMatchup(toScore)



            //foreach (List<MatchupModel> round in model.Rounds)
            //{
            //    foreach (MatchupModel rm in round)
            //    {
            //        foreach (MatchupEntryModel me in rm.Entries)
            //        {
            //            if (me.ParentMatchup != null)
            //            {
            //                if (me.ParentMatchup.Id == m.Id)
            //                {
            //                    me.TeamCompeting = m.Winner;
            //                    GlobalConfig.Connection.UpdateMatchup(rm);
            //                }
            //            }
            //        }
            //    }
            //}

            //GlobalConfig.Connection.UpdateMatchup(m);

        }

        private static void ScoreMatchup(List<MatchupModel> models)
        {
            // greater or lesser
            string greaterWins = ConfigurationManager.AppSettings["greaterWins"];

            foreach (MatchupModel m in models)
            {
                // 0 means false or low score wins
                if (greaterWins == "0")
                {
                    if (true)
                    {

                    }
                }
                else
                {
                    // 1 mean true, or high score wins
                }
            }
            //if (teamOneScore > teamTwoScore)
            //{
            //    // Team one wins
            //    m.Winner = m.Entries[0].TeamCompeting;
            //}
            //else if (teamTwoScore > teamOneScore)
            //{
            //    // Team two wins
            //    m.Winner = m.Entries[1].TeamCompeting;
            //}
            //else
            //{
            //    MessageBox.Show("I do not handle tie game.");
            //}
        }
        public static void CreateRounds(TournamentModel model)
        {
            List<TeamModel> randomizedTeams = RandomizeTeamOrder(model.EnteredTeams);
            int rounds = FindNumberOfRounds(randomizedTeams.Count);
            int byes = NumberOfByes(rounds, randomizedTeams.Count);

            model.Rounds.Add(CreateFirstRound(byes, randomizedTeams));
            CreateOtherRounds(model, rounds);

        }

        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = model.Rounds[0];
            List<MatchupModel> currRound = new List<MatchupModel>();
            MatchupModel currMatchup = new MatchupModel();

            while (round <= rounds)
            {
                foreach (MatchupModel match in previousRound)
                {
                    currMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match });

                    if (currMatchup.Entries.Count > 1)
                    {
                        currMatchup.MatchupRound = round;
                        currRound.Add(currMatchup);
                        currMatchup = new MatchupModel();
                    }
                }

                model.Rounds.Add(currRound);
                previousRound = currRound;
                currRound = new List<MatchupModel>();
                round += 1;
            }
        }

        private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchupModel> output = new List<MatchupModel>();
            MatchupModel curr = new MatchupModel();

            foreach (TeamModel team in teams)
            {
                curr.Entries.Add(new MatchupEntryModel { TeamCompeting = team });

                if (byes > 0 || curr.Entries.Count > 1)
                {
                    curr.MatchupRound = 1;
                    output.Add(curr);
                    curr = new MatchupModel();

                    if (byes > 0)
                    {
                        byes -= 1;
                    }
                }
            }

            return output;
        }

        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            int output = 0;
            int totalTeams = 1;

            for (int i = 1; i <= rounds; i++)
            {
                totalTeams *= 2;
            }

            output = totalTeams - numberOfTeams;

            return output;
        }

        private static int FindNumberOfRounds(int teamCount)
        {
            int output = 1;
            int val = 2;

            while (val < teamCount)
            {
                // output = output +1
                output += 1;

                // val = val *2;
                val *= 2;
            }

            return output;
        }


    }
}
