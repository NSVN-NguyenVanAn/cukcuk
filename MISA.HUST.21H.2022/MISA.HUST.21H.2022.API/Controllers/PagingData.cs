using MISA.HUST._21H._2022.API.Entities;

namespace MISA.HUST._21H._2022.API.Controllers
{
    internal class PagingData<T>
    {
        public PagingData()
        {
        }

        public List<Employee> Data { get; set; }
        public long TotalCount { get; set; }
    }
}