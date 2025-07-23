using System;
using System.Collections.Generic;

namespace EspnFantasyLeagueHistoryDataLoader.src.Context;

public partial class TeamYearStat
{
    public int Id { get; set; }

    public int TeamId { get; set; }

    public int Year { get; set; }

    public int? Wins { get; set; }

    public int? Losses { get; set; }

    public int? Ties { get; set; }

    public decimal? PointsFor { get; set; }

    public decimal? PointsAgainst { get; set; }

    public virtual Team Team { get; set; } = null!;
}
