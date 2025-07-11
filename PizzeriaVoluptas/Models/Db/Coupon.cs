﻿using System;
using System.Collections.Generic;

namespace PizzeriaVoluptas.Models.Db;

public partial class Coupon
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal? Discount { get; set; }
}
