namespace Portfolio.Models
{
	public class Photo
	{
		public int Id { get; set; }
		public byte[] Bytes { get; set; }
		public string Description { get; set; } = string.Empty;
		public string FileExtension { get; set; } = string.Empty;
		public decimal Size { get; set; }
		public int WorkId { get; set; }
	}
}
