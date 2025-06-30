namespace PizzeriaVoluptas.Models.ViewModels
{
    public class DishCartViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; } 
        public string? ImageName { get; set; }
        public int Count { get; set; }
        public decimal? Price { get; set; }
        public decimal? RowSumPrice { get; set; }
    }
}
