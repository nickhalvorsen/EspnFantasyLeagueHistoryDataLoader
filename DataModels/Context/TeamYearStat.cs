using System;
using System.Collections.Generic;

namespace DataModels.Context;

public partial class TeamYearStat
{
    public long Id { get; set; }

    public long TeamId { get; set; }

    public int Year { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Ties { get; set; }

    public decimal PointsFor { get; set; }

    public decimal PointsAgainst { get; set; }

    public int PlayoffSeed { get; set; }

    public int FinalRank { get; set; }

    public virtual Team Team { get; set; } = null!;
}
