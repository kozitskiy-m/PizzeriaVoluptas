using System;
using System.Collections.Generic;

namespace PizzeriaVoluptas.Models.Db;

public partial class OrderDetail
{
    public int Id { get; set; }

    public string DishTitle { get; set; } = null!;

    public decimal DishPrice { get; set; }

    public int Count { get; set; }

    public int OrderId { get; set; }

    public int DishId { get; set; }
}
