using Entities.Enums;
using WebApiQandA.Validators;

namespace WebApiQandA.DTO
{
    public class Sort<T>
    {
        [ValidEnum]
        public T SortBy { get; set; }

        [ValidEnum]
        public SortDirection SortDirection { get; set; }
    }
}
