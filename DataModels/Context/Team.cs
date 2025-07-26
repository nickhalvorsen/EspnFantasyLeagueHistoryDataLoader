using System;
using System.Collections.Generic;

namespace DataModels.Context;

public partial class Team
{
    public long Id { get; set; }

    public string EspnId { get; set; } = null!;

    public string? PrimaryOwnerId { get; set; }

    public string? ManagerName { get; set; }

    public virtual ICollection<TeamWeekStat> TeamWeekStatAwayTeamEspns { get; set; } = new List<TeamWeekStat>();

    public virtual ICollection<TeamWeekStat> TeamWeekStatHomeTeamEspns { get; set; } = new List<TeamWeekStat>();

    public virtual ICollection<TeamYearStat> TeamYearStats { get; set; } = new List<TeamYearStat>();
}
