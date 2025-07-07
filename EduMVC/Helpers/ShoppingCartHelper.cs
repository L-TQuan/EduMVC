using EduMVC.ViewModels;

namespace EduMVC.Helpers
{
    public class ShoppingCartHelper
    {
        //private static ShoppingCart Cart = null;

        //public static ShoppingCart Get(string userId)
        //{
        //    return Cart;
        //}

        //public static ShoppingCart Set(String userId, CourseSummaryVM product)
        //{
        //    var cartItem = new CartItem
        //    {
        //        Id = product.Id,
        //        Price = product.Price,
        //        Product = product,
        //    };
        //    var listCartItems = new List<CartItem>();
        //    listCartItems.Add(cartItem);
        //    Cart = new ShoppingCart
        //    {
        //        UserId = userId,
        //        CartItems = listCartItems,
        //    };
        //    return Cart;
        //}

        public static ShoppingCart Set(string userId, Guid courseId)
        {
            var courseIds = new List<Guid> { courseId };
            return new ShoppingCart
            {
                UserId = userId,
                CourseIds = courseIds,
            };
        }

        public static void AddCourse(ShoppingCart cart, Guid courseId)
        {
            if (!cart.CourseIds.Contains(courseId))
            {
                cart.CourseIds.Add(courseId);
            }
        }
    }
}
