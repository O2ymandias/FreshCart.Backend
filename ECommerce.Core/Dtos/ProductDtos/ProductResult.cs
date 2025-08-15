using System.Text.Json.Serialization;

namespace ECommerce.Core.Dtos.ProductDtos;
public class ProductResult
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public string PictureUrl { get; set; }
	public decimal Price { get; set; }
	public string Brand { get; set; }
	public string Category { get; set; }

	[JsonIgnore]
	public int UnitsInStock { get; set; }
	public bool InStock => UnitsInStock > 0;

}
