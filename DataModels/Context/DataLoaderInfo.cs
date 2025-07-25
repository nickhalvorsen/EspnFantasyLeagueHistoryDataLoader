using System;
using System.Collections.Generic;

namespace DataModels.Context;

public partial class DataLoaderInfo
{
    public int Id { get; set; }

    public DateTime LastSuccessfulLoad { get; set; }
}
