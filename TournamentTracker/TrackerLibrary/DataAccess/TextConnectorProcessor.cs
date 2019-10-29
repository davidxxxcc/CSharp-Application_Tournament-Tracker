using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    // Load the text file
    // Convert the text to List<PrizeMoel>
    // Find the max ID
    // Add the new record with the new ID (max + 1)
    // Convert the prizes to list<string>
    // Save the list<string> to the text file
    public static class TextConnectorProcessor
    {
        public static string FullFilePath(this string fileName)  //PrizeModel.csv
        {
            return $@"{ ConfigurationManager.AppSettings["filePath"] }\{ fileName }";
        }

        public static List<string> LoadFile(this string file)
        {
            if (File.Exists(file) == false)
            {
                return new List<string>();
            }
            return File.ReadAllLines(file).ToList();
        }

        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new List<PrizeModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                PrizeModel p = new PrizeModel();
                p.Id = int.Parse(cols[0]);
                p.PlaceNumber = int.Parse(cols[1]);
                p.PlaceName = cols[2];
                p.PrizeAmount = decimal.Parse(cols[3]);
                p.PrizePercentage = double.Parse(cols[4]);
                output.Add(p);
            }

            return output;
        }

        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            List<PersonModel> output = new List<PersonModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                PersonModel p = new PersonModel();
                p.Id = int.Parse(cols[0]);
                p.FirstName = cols[1];
                p.LastName = cols[2];
                p.EmailAddress = cols[3];
                p.CellphoneNumber = cols[4];
                output.Add(p);
            }
            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines, string peopleFileName)
        {
            //id,team,name,list of ids seperated by the pipe
            //Example: 3,Tim's Team,1|3|5
            List<TeamModel> output = new List<TeamModel>();
            List<PersonModel> people = peopleFileName.FullFilePath().LoadFile().ConvertToPersonModels();


            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                TeamModel t = new TeamModel();
                t.Id = int.Parse(cols[0]);
                t.TeamName = cols[1];

                string[] personIds = cols[2].Split('|');

                foreach (string id in personIds)
                {
                    t.TeamMembers.Add(people.Where(x => x.Id == int.Parse(id)).First());
                }
                output.Add(t);
            }

            return output;
        }

        public static List<TournamentModel> ConvertToTournamentModels(
            this List<string> lines,
            string teamFileName,
            string peopleFileName,
            string prizeFileName
            )
        {
            // id = 0
            // TournamentName = 1
            // EntryFee = 2
            // EnteredTeams = 3
            // Prizes = 4
            // Rounds = 5
            //id, TournamentName, EntryFee, (id|id|id - Entered Teams), (id|id|id - Prizes), (Rounds - id^id^id|id^id^id|id^id^id)
            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> teams = teamFileName.FullFilePath().LoadFile().ConvertToTeamModels(peopleFileName);
            List<PrizeModel> prizes = prizeFileName.FullFilePath().LoadFile().ConvertToPrizeModels();
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TournamentModel tm = new TournamentModel();
                tm.Id = int.Parse(cols[0]);
                tm.TournamentName = cols[1];
                tm.EntryFee = decimal.Parse(cols[2]);

                string[] teamIds = cols[3].Split('|');

                foreach (string id in teamIds)
                {
                    tm.EnteredTeams.Add(teams.Where(x => x.Id == int.Parse(id)).First());
                }

                string[] prizeIds = cols[4].Split('|');

                foreach (string id in prizeIds)
                {
                    tm.Prizes.Add(prizes.Where(x => x.Id == int.Parse(id)).First());
                }

                // Capture Rounds information
                string[] rounds = cols[5].Split('|');
                List<MatchupModel> ms = new List<MatchupModel>();

                foreach (string round in rounds)
                {
                    string[] msText = round.Split('^');
                    foreach (string matchupModelTextId in msText)
                    {
                        ms.Add(matchups.Where(x => x.Id == int.Parse(matchupModelTextId)).First());
                    }

                    tm.Rounds.Add(ms);
                    // TODO - I add this
                    ms = new List<MatchupModel>();
                }

                output.Add(tm);
            }
            return output;
        }


        private static List<MatchupEntryModel> ConvertStringToMatchupEntryModels(string input)
        {
            string[] ids = input.Split('|');
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            foreach (string id in ids)
            {
                output.Add(entries.Where(x => x.Id == int.Parse(id)).First());
            }
            return output;
        }
        public static List<MatchupEntryModel> ConvertToMatchupEntryModels(this List<string> lines)
        {
            // id = 0, TeamCompeting = 1, Score = 2, ParentMatchup = 3
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchupEntryModel me = new MatchupEntryModel();
                me.Id = int.Parse(cols[0]);
                if (cols[1].Length == 0)
                {
                    me.TeamCompeting = null;
                }
                else
                {
                    me.TeamCompeting = LookupTeamById(int.Parse(cols[1]));
                }

                me.Score = double.Parse(cols[2]);

                int parentId = 0;
                if (int.TryParse(cols[3], out parentId))
                {
                    me.ParentMatchup = LookupMatchupById(parentId);
                }
                else
                {
                    me.ParentMatchup = null;
                }

                output.Add(me);
            }

            return output;
        }
        public static List<MatchupModel> ConvertToMatchupModels(this List<string> lines)
        {
            //id=0,entries=1(pipe delimited by entry id), winner=2, matchupRound=3
            List<MatchupModel> output = new List<MatchupModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                MatchupModel p = new MatchupModel();
                p.Id = int.Parse(cols[0]);
                p.Entries = ConvertStringToMatchupEntryModels(cols[1]);
                int winner = 0;
                if(int.TryParse(cols[2], out winner))
                {
                    p.Winner = LookupTeamById(winner);
                }
                else
                {
                    p.Winner = null;
                }
               
                p.MatchupRound = int.Parse(cols[3]);
                output.Add(p);
            }

            return output;
        }

        private static MatchupModel LookupMatchupById(int id)
        {
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();
            return matchups.Where(x => x.Id == id).First();
        }
        private static TeamModel LookupTeamById(int id)
        {
            List<TeamModel> teams = GlobalConfig.TeamsFile.FullFilePath().LoadFile().ConvertToTeamModels(GlobalConfig.PeopleFile);
            return teams.Where(x => x.Id == id).First();
        }


        private static string ConvertRoundListToString(List<List<MatchupModel>> rounds)
        {
            //id, TournamentName, EntryFee, (id|id|id - Entered Teams), (id|id|id - Prizes), (Rounds - id^id^id|id^id^id|id^id^id)
            string output = String.Empty;

            if (rounds.Count == 0)
            {
                return "";
            }

            //2|5|
            foreach (List<MatchupModel> r in rounds)
            {
                output += $"{ ConvertMatchupListToString(r) }|";
            }
            //2|5
            output = output.Substring(0, output.Length - 1);

            return output;
        }
        private static string ConvertMatchupListToString(List<MatchupModel> matchups)
        {
            string output = String.Empty;

            if (matchups.Count == 0)
            {
                return "";
            }

            //2|5|
            foreach (MatchupModel m in matchups)
            {
                output += $"{ m.Id }^";
            }
            //2|5
            output = output.Substring(0, output.Length - 1);

            return output;
        }
        private static string ConvertPrizeListToString(List<PrizeModel> prizes)
        {
            string output = String.Empty;

            if (prizes.Count == 0)
            {
                return "";
            }

            //2|5|
            foreach (PrizeModel p in prizes)
            {
                output += $"{ p.Id }|";
            }
            //2|5
            output = output.Substring(0, output.Length - 1);

            return output;
        }
        private static string ConverMatchupEntryListToString(List<MatchupEntryModel> entries)
        {
            string output = String.Empty;

            if (entries.Count == 0)
            {
                return "";
            }

            //2|5|
            foreach (MatchupEntryModel e in entries)
            {
                output += $"{ e.Id }|";
            }
            //2|5
            output = output.Substring(0, output.Length - 1);

            return output;
        }
        private static string ConvertTeamListToString(List<TeamModel> teams)
        {
            string output = String.Empty;

            if (teams.Count == 0)
            {
                return "";
            }

            //2|5|
            foreach (TeamModel t in teams)
            {
                output += $"{ t.Id }|";
            }
            //2|5
            output = output.Substring(0, output.Length - 1);

            return output;
        }
        private static string ConvertPeopleListToString(List<PersonModel> people)
        {
            string output = String.Empty;

            if (people.Count == 0)
            {
                return "";
            }

            //2|5|
            foreach (PersonModel p in people)
            {
                output += $"{ p.Id }|";
            }
            //2|5
            output = output.Substring(0, output.Length - 1);

            return output;
        }



        public static void SaveToTournamentFile(this List<TournamentModel> models, string fileName)
        {
            // id = 0
            // TournamentName = 1
            // EntryFee = 2
            // EnteredTeams = 3
            // Prizes = 4
            // Rounds = 5
            //id, TournamentName, EntryFee, (id|id|id - Entered Teams), (id|id|id - Prizes), (Rounds - id^id^id|id^id^id|id^id^id)

            List<string> lines = new List<string>();
            foreach (TournamentModel tm in models)
            {
                lines.Add($@"{  tm.Id },
                            { tm.TournamentName },
                            { tm.EntryFee },
                            { ConvertTeamListToString(tm.EnteredTeams) },
                            { ConvertPrizeListToString(tm.Prizes) },
                            { ConvertRoundListToString(tm.Rounds) }");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }
        public static void SaveToTeamFile(this List<TeamModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (TeamModel t in models)
            {
                lines.Add($"{ t.Id },{t.TeamName},{ConvertPeopleListToString(t.TeamMembers)}");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }
        public static void SaveToPersonFile(this List<PersonModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (PersonModel p in models)
            {
                lines.Add($"{p.Id},{p.FirstName},{p.LastName},{p.EmailAddress},{p.CellphoneNumber}");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }
        public static void SaveToPrizeFile(this List<PrizeModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (PrizeModel p in models)
            {
                lines.Add($"{p.Id},{p.PlaceNumber},{p.PlaceName},{p.PrizeAmount},{p.PrizePercentage}");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }
        public static void SaveRoundsToFiles(this TournamentModel model)
        {
            // Loop through each Round
            // Loop through each Matchup
            // Get the id for the new matchup and save the record
            // Loop through each Entry, get the id, and save it

            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    // Load all of the matchups from file
                    // Get the top id and add one
                    // Store the id
                    // Save the matchup record
                    matchup.SaveMatchupToFile();
                }
            }
        }
        public static void SaveMatchupToFile(this MatchupModel matchup)
        {

            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            int currentId = 1;
            if (matchups.Count > 0)
            {
                currentId = matchups.OrderByDescending(x => x.Id).First().Id + 1;
            }

            matchup.Id = currentId;
            matchups.Add(matchup);


            foreach (MatchupEntryModel entry in matchup.Entries)
            {
                entry.SaveEntryToFile();
            }

            // save to file
            List<string> lines = new List<string>();
            //id=0,entries=1(pipe delimited by id), winner=2, matchupRound=3

            foreach (MatchupModel m in matchups)
            {
                string winner = "";
                if (m.Winner != null)
                {
                    winner = m.Winner.Id.ToString();
                }
                lines.Add($"{ m.Id },{ ConverMatchupEntryListToString(m.Entries) },{ winner },{ m.MatchupRound }");
            }

            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
        }
        public static void SaveEntryToFile(this MatchupEntryModel entry)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            int currentId = 1;

            if (entries.Count > 0)
            {
                currentId = entries.OrderByDescending(x => x.Id).First().Id + 1;
            }
            entry.Id = currentId;
            entries.Add(entry);

            List<string> lines = new List<string>();
            // id = 0, TeamCompeting = 1, Score = 2, ParentMatchup = 3
            foreach (MatchupEntryModel e in entries)
            {
                string parent = "";
                if (e.ParentMatchup != null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }
                string teamCompeting = "";
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{ e.Id },{ teamCompeting },{ e.Score },{ parent }");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
        }
    }
}
