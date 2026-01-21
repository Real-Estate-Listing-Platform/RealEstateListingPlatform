using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Favorite
{
    public Guid UserId { get; set; }

    public Guid ListingId { get; set; }

    public DateTime? SavedAt { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
