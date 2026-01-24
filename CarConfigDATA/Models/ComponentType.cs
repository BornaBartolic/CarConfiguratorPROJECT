using System;
using System.Collections.Generic;

namespace CarConfigDATA.Models;

public partial class ComponentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CarComponent> CarComponents { get; set; } = new List<CarComponent>();
}
