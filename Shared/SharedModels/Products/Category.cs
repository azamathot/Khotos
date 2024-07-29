using System.ComponentModel.DataAnnotations;

namespace SharedModels.Products
{
	public class Category
	{
		public int Id { get; set; }

		[Display(Name = "Название категории услуг")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "Описание категории услуг")]
		public string? Description { get; set; }
		public List<Product>? Products { get; set; }
	}
}
