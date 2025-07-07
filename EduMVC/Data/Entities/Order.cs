using System.Collections.ObjectModel;

namespace EduMVC.Data.Entities
{
    public class Order
    {
        public Order()
        {
            Id = Guid.NewGuid();
            OrderDetails = new Collection<OrderDetail>();
        }
        public Guid Id { get; set; }
        public String UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }

        //List of Products
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
