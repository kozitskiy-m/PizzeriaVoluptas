using System;
using System.Collections.Generic;

namespace PizzeriaVoluptas.Models.Db;

public partial class DishGalery
{
    public int Id { get; set; }

    public int DishId { get; set; }

    public string? ImageName { get; set; }
}
