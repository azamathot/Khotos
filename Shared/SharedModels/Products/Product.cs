using System.ComponentModel.DataAnnotations;

namespace SharedModels.Products
{
	public class Product
	{
		public int Id { get; set; }

		[Display(Name = "Название услуи")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "Описание услуги")]
		public string? Description { get; set; }

		[Display(Name = "Цена")]
		public decimal? Price { get; set; }
		public int CategoryId { get; set; }
	}
}
