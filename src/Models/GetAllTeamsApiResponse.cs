// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

namespace EspnFantasyLeagueHistoryDataLoader.Models.GetAllTeamsApiResponse;

public class Away
{
    public double gamesBack { get; set; }
    public int losses { get; set; }
    public double percentage { get; set; }
    public double pointsAgainst { get; set; }
    public double pointsFor { get; set; }
    public int streakLength { get; set; }
    public string streakType { get; set; }
    public int ties { get; set; }
    public int wins { get; set; }
}

public class Division
{
    public double gamesBack { get; set; }
    public int losses { get; set; }
    public double percentage { get; set; }
    public double pointsAgainst { get; set; }
    public double pointsFor { get; set; }
    public int streakLength { get; set; }
    public string streakType { get; set; }
    public int ties { get; set; }
    public int wins { get; set; }
}

public class DraftDetail
{
    public bool drafted { get; set; }
    public bool inProgress { get; set; }
}

public class DraftStrategy
{
    public List<object> futureKeeperPlayerIds { get; set; }
    public List<object> keeperPlayerIds { get; set; }
}

public class Home
{
    public double gamesBack { get; set; }
    public int losses { get; set; }
    public double percentage { get; set; }
    public double pointsAgainst { get; set; }
    public double pointsFor { get; set; }
    public int streakLength { get; set; }
    public string streakType { get; set; }
    public int ties { get; set; }
    public int wins { get; set; }
}

public class MatchupAcquisitionTotals
{
}

public class Member
{
    public string displayName { get; set; }
    public string firstName { get; set; }
    public string id { get; set; }
    public string lastName { get; set; }
    public List<NotificationSetting> notificationSettings { get; set; }
}

public class NotificationSetting
{
    public bool enabled { get; set; }
    public string id { get; set; }
    public string type { get; set; }
}

public class Overall
{
    public double gamesBack { get; set; }
    public int losses { get; set; }
    public double percentage { get; set; }
    public double pointsAgainst { get; set; }
    public double pointsFor { get; set; }
    public int streakLength { get; set; }
    public string streakType { get; set; }
    public int ties { get; set; }
    public int wins { get; set; }
}

public class Record
{
    public Away away { get; set; }
    public Division division { get; set; }
    public Home home { get; set; }
    public Overall overall { get; set; }
}

public class GetAllTeamsApiResponse
{
    public DraftDetail draftDetail { get; set; }
    public int gameId { get; set; }
    public int id { get; set; }
    public List<Member> members { get; set; }
    public int scoringPeriodId { get; set; }
    public int seasonId { get; set; }
    public int segmentId { get; set; }
    public Status status { get; set; }
    public List<Team> teams { get; set; }
}

public class Status
{
    public long activatedDate { get; set; }
    public int createdAsLeagueType { get; set; }
    public int currentLeagueType { get; set; }
    public int currentMatchupPeriod { get; set; }
    public int finalScoringPeriod { get; set; }
    public int firstScoringPeriod { get; set; }
    public bool isActive { get; set; }
    public bool isExpired { get; set; }
    public bool isFull { get; set; }
    public bool isPlayoffMatchupEdited { get; set; }
    public bool isToBeDeleted { get; set; }
    public bool isViewable { get; set; }
    public bool isWaiverOrderEdited { get; set; }
    public int latestScoringPeriod { get; set; }
    public List<int> previousSeasons { get; set; }
    public int teamsJoined { get; set; }
    public int transactionScoringPeriod { get; set; }
    public long waiverLastExecutionDate { get; set; }
    public WaiverProcessStatus waiverProcessStatus { get; set; }
}

public class Team
{
    public string abbrev { get; set; }
    public int currentProjectedRank { get; set; }
    public int divisionId { get; set; }
    public int draftDayProjectedRank { get; set; }
    public int id { get; set; }
    public bool isActive { get; set; }
    public string logo { get; set; }
    public string logoType { get; set; }
    public string name { get; set; }
    public List<string> owners { get; set; }
    public int playoffSeed { get; set; }
    public double points { get; set; }
    public double pointsAdjusted { get; set; }
    public double pointsDelta { get; set; }
    public string primaryOwner { get; set; }
    public int rankCalculatedFinal { get; set; }
    public int rankFinal { get; set; }
    public Record record { get; set; }
    public TradeBlock tradeBlock { get; set; }
    public TransactionCounter transactionCounter { get; set; }
    public int waiverRank { get; set; }
    public DraftStrategy draftStrategy { get; set; }
}

public class TradeBlock
{
}

public class TransactionCounter
{
    public int acquisitionBudgetSpent { get; set; }
    public int acquisitions { get; set; }
    public int drops { get; set; }
    public MatchupAcquisitionTotals matchupAcquisitionTotals { get; set; }
    public int misc { get; set; }
    public int moveToActive { get; set; }
    public int moveToIR { get; set; }
    public double paid { get; set; }
    public double teamCharges { get; set; }
    public int trades { get; set; }
}

public class WaiverProcessStatus
{
}

