using System;
namespace TrackerLibrary.Models
{
    public class MatchupEntryModel
    {
        public TeamModel TeamingCompleting { get; set; }
        public double Score { get; set; }
        public MatchupModel ParentMatchup  { get; set; }

    }
}
