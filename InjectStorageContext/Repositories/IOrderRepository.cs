using System.Collections.Generic;
using System.Threading.Tasks;

namespace InjectStorageContext.Repositories
{
    public interface IOrderRepository
    {
        Task Add(Order order);
        Task Add(IEnumerable<Order> orders);
    }
}