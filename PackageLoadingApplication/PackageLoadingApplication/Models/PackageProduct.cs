using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageLoadingApplication.Models
{
    public class PackageProduct
    {
        public int IncNum { get; set; }
        public int ProductId { get; set; }
        public string Descr { get; set; }
        public int EntityId { get; set; }
        public int HasParent { get; set; }
        public int IsParent { get; set; }
        public string EntityName { get; set; }

        public List<PackageProduct> ContainedPackagesList { get; set; }
    }
}
