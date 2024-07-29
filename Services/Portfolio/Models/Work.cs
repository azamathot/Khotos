using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models
{
	public class Work
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string? LinkWeb { get; set; }
		public string? LinkSource { get; set; }
		public string Technologies { get; set; } = string.Empty;
		public string? LinkLocalWebHost { get; set; }
		//navigation property: configure one-t0-many relationship with Photo   
		public List<Photo>? Photos { get; set; }

		[FromForm]
		[NotMapped]
		public IFormFileCollection? Files { get; set; }
	}
}
