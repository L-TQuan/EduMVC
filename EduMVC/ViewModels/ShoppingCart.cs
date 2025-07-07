namespace EduMVC.ViewModels
{
    public class ShoppingCart
    {
        public String UserId { get; set; }
        public List<Guid> CourseIds { get; set; } = new List<Guid>();
        public decimal TotalPrice { get; set; }
        //{
        //    get
        //    {
        //        return CartItems?.Sum(ci => ci.Price) ?? 0;
        //    }
        //}

        //public bool AddProduct(CourseSummaryVM product)
        //{
        //    var existingItem = CartItems.SingleOrDefault(ci => ci.Id.Equals(product.Id));
        //    if (existingItem != null)
        //    {
        //        return false; // Course is already in the cart
        //    }

        //    var cartItem = new CartItem
        //    {
        //        Id = product.Id,
        //        Price = product.Price,
        //        Product = product,
        //    };
        //    CartItems.Add(cartItem);
        //    return true;
        //}
    }
}
