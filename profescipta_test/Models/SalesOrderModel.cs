using System;
using System.ComponentModel.DataAnnotations;

namespace profescipta_test.Models
{
    public class SalesOrderModel
    {
      
        public int Id { get; set; }
        public string SalesOrder { get; set; }
        public DateTime OrderDate { get; set; }
        public string Customer { get; set; }
        public string Address { get; set; }
    }

    public class SalesOrderBulk
    {
        public List<SalesOrderModel> Orders { get; set; } = new List<SalesOrderModel>();
    }

    public class OrderItemModel
    {

        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        public int Total { get; set; }
        public int Price { get; set; }
    }

    public class SalesOrderDetail
    {
        public List<OrderItemModel> Order { get; set; }
        public SalesOrderModel SalesOrder { get; set; }
    }
}

