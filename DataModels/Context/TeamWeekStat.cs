using System;
using System.Collections.Generic;

namespace DataModels.Context;

public partial class TeamWeekStat
{
    public long Id { get; set; }

    public int Year { get; set; }

    public int WeekNumber { get; set; }

    public string AwayTeamEspnId { get; set; } = null!;

    public string HomeTeamEspnId { get; set; } = null!;

    public decimal AwayTeamScore { get; set; }

    public decimal HomeTeamScore { get; set; }

    public string Winner { get; set; } = null!;

    public virtual Team AwayTeamEspn { get; set; } = null!;

    public virtual Team HomeTeamEspn { get; set; } = null!;
}
