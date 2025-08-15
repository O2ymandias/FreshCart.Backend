namespace ECommerce.Core.Models.CartModule;
public class Cart
{
	public Cart(string id)
	{
		Id = id;
		Items = [];
	}
	public string Id { get; set; }
	public List<CartItem> Items { get; set; }
}
