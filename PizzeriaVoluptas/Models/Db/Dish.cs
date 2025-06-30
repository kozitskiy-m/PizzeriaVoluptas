using System;
using System.Collections.Generic;

namespace PizzeriaVoluptas.Models.Db;

public partial class Dish
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? FullDesc { get; set; }

    public decimal? Price { get; set; }

    public decimal? Discount { get; set; }

    public string? ImageName { get; set; }

    public int? Qty { get; set; }

    public string? Ingredients { get; set; }
}
