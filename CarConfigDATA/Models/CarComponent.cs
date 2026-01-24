using System;
using System.Collections.Generic;

namespace CarConfigDATA.Models;

public partial class CarComponent
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public int ComponentTypeId { get; set; }

    public virtual ComponentType ComponentType { get; set; } = null!;
        
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
