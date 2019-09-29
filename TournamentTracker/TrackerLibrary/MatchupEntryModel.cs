using System;
namespace TrackerLibrary
{
    public class MatchupEntryModel
    {
        public TeamModel TeamingCompleting { get; set; }
        public double Score { get; set; }
        public MatchupModel ParentMatchup  { get; set; }

    }
}
