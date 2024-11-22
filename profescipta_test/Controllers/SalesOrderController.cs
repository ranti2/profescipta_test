using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using profescipta_test.Controllers;
using profescipta_test.Models;
using profescipta_test.Repository;
using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace profescipta_test.Controllers;

public class SalesOrderController : Controller
{

    private SalesOrderRepo salesOrderRepo;

    public SalesOrderController(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        salesOrderRepo = new SalesOrderRepo(connectionString);
    }

    // GET: SalesOrders
    public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string keyword = "", string orderDateFilter = "")
    {
        var salesOrders = await salesOrderRepo.GetPagedSalesOrdersAsync(page, pageSize, keyword, orderDateFilter);
        var totalRecords = await salesOrderRepo.GetTotalSalesOrderAsync(pageSize, keyword, orderDateFilter);
        var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalRecords"] = totalRecords;
        ViewData["StartRecord"] = (page - 1) * pageSize + 1;
        ViewData["EndRecord"] = Math.Min(page * pageSize, totalRecords);
        ViewData["Keyword"] = keyword;
        ViewData["OrderDateFilter"] = orderDateFilter;


        int index = 1;
        if (page > 1)
        {
            index = (page - 1) * pageSize;
            index = index + 1;
        }
        ViewData["Index"] = index;

        return View(salesOrders);
    }
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // GET: /SalesOrder/Create
    public IActionResult Create()
    {
        var sales = new SalesOrderModel();
        var order = new List<OrderItemModel>();  
        var salesOrder= new SalesOrderDetail{
            Order = order,
            SalesOrder = sales
        };
        salesOrder.SalesOrder.OrderDate = DateTime.Today;
        return View(salesOrder);
    }

    // POST: /SalesOrder/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SalesOrderDetail model)
    {
        if (ModelState.IsValid)
        {
   
            var salesOrder = await salesOrderRepo.GetSalesOrderByOrdersAsync(model.SalesOrder.SalesOrder);
            if (salesOrder == null) {

               await salesOrderRepo.AddSalesOrderAsync(model.SalesOrder);
               salesOrderRepo.BulkInsertOrderItem(model.Order);

            }
            else
            {
                TempData["SuccessMessage"] = "Sales Order Alredy Exist!";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Sales Order created successfully!";
            return RedirectToAction("Index");
        }

        return View(model);
    }

    // GET: /SalesOrder/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        SalesOrderModel salesOrder = null;

        salesOrder = await salesOrderRepo.GetSalesOrderByIdsAsync(id);

        if (salesOrder == null)
        {
            return NotFound();
        }

        return View(salesOrder);
    }

    // POST: /SalesOrder/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SalesOrderModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await salesOrderRepo.UpdateSalesOrderAsync(id, model);

            TempData["SuccessMessage"] = "Sales Order updated successfully!";
            return RedirectToAction("Index");
        }

        return View(model);
    }

    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await salesOrderRepo.DeleteSalesOrderAsync(id);

        TempData["SuccessMessage"] = "Sales Order deleted successfully!";
        return RedirectToAction("Index");
    }

    
    public async Task<IActionResult> BulkCreate(List<SalesOrderModel> data)
    {
        if (ModelState.IsValid)
        {
            foreach(var item in data)
            {
                var salesOrder = await salesOrderRepo.GetSalesOrderByOrdersAsync(item.SalesOrder);
                if (salesOrder == null)
                {

                    await salesOrderRepo.AddSalesOrderAsync(item);
                }
                else
                {
                    TempData["SuccessMessage"] = "Sales Order Alredy Exist!";
                    return RedirectToAction("Index");
                }

            }
            

            TempData["SuccessMessage"] = "Sales Order created successfully!";
            return RedirectToAction("Index");
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Export(string keyword = "", string orderDateFilter = "")
    {
        var data = await salesOrderRepo.GetSalesOrdersAsync(keyword, orderDateFilter);
        var csv = new StringBuilder();
        csv.AppendLine("Sales Order,Order Date,Customer"); 

        foreach (var item in data)
        {
            csv.AppendLine($"{item.SalesOrder},{item.OrderDate},{item.Customer}"); 
        }

        var fileName = "ExportedData.csv";
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);

    }


}


