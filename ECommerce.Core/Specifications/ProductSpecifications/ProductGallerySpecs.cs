using ECommerce.Core.Models.ProductModule;

namespace ECommerce.Core.Specifications.ProductSpecifications;
public class ProductGallerySpecs : BaseSpecification<ProductGallery>
{
	public ProductGallerySpecs(int productId)
		: base(g => g.ProductId == productId)
	{
	}
}
