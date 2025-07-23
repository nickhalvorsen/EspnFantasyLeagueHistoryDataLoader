using System;
using System.Collections.Generic;

namespace EspnFantasyLeagueHistoryDataLoader.src.Context;

public partial class Team
{
    public int Id { get; set; }

    public string EspnId { get; set; } = null!;

    public string? Name { get; set; }

    public string? OwnerName { get; set; }

    public virtual ICollection<TeamYearStat> TeamYearStats { get; set; } = new List<TeamYearStat>();
}
