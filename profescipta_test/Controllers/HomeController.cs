using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using profescipta_test.Controllers;
using profescipta_test.Models;
using profescipta_test.Repository;
using System;
using System.Threading.Tasks;

namespace profescipta_test.Controllers;

public class HomeController : Controller
{

    private SalesOrderRepo _databaseHelper;

    public HomeController(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        _databaseHelper = new SalesOrderRepo(connectionString);
    }

    // GET: SalesOrders
    public async Task<IActionResult> Index(int page = 1,int pageSize = 5,string keyword = "",string orderDateFilter = "")
    {
        var salesOrders = await _databaseHelper.GetPagedSalesOrdersAsync(page, pageSize,keyword,orderDateFilter);
        var totalRecords = await _databaseHelper.GetTotalSalesOrderAsync(pageSize,keyword,orderDateFilter);
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
            index = (page -1)* pageSize;
            index = index + 1;
        }
        ViewData["Index"] = index;

        return View(salesOrders);
    }
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }



}

