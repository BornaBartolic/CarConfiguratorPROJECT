using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarConfigDATA.Services
{
    public interface IComponentTypeOrderStore
    {
        Task<List<int>> GetOrderAsync();
        Task SaveOrderAsync(List<int> orderedTypeIds);
    }
}
