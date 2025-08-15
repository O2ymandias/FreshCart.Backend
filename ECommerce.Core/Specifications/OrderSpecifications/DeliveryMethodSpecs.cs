using ECommerce.Core.Models.OrderModule;

namespace ECommerce.Core.Specifications.OrderSpecifications;
public class DeliveryMethodSpecs : BaseSpecification<DeliveryMethod>
{
	public DeliveryMethodSpecs(int? deliveryMethodId)
		: base(d => !deliveryMethodId.HasValue || d.Id == deliveryMethodId.Value)
	{
	}
}
