using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcProduct.Models;
using Serilog;

namespace MvcProduct.Controllers
{    
    // 使用樣板工具來產生CRUD頁面 (Create、Read、Update、Delete)
    // Controller資料夾右鍵 --> 新增Scaffold --> 選取使用 【Entity Framework 執行檢視的 MVC 控制器】

    public class ProductsController : Controller
    {
        private readonly AdventureWorksLT2022Context _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(AdventureWorksLT2022Context context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }


        /* 教學課程：開始在 ASP.NET MVC Web 應用程式中使用 EF Core
         * https://learn.microsoft.com/zh-tw/aspnet/core/data/ef-mvc/intro?view=aspnetcore-8.0
         * 非同步程式設計是預設的 ASP.NET Core 和 EF Core 模式。
         * 在下列程式碼中，async、Task<T>、await 和 ToListAsync 都會讓程式碼以非同步方式執行。
         */


        // GET: Products
        // 取得產品列表。 for test 顯示前 5 筆資料
        public async Task<IActionResult> Index()
        {         
            Log.Information("Products/Index");

            try
            {
                var products = _context.Product
                    .Include(p => p.ProductCategory)
                    .Include(p => p.ProductModel)
                    .OrderBy(p => p.ProductID)
                    .Take(5) // for test 取前 5 筆
                    .ToListAsync();

                return View(await products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "取得產品清單時發生錯誤。");
                return StatusCode(500, "伺服器內部錯誤，請稍後再試。");
               
            }
        }

        // GET: Products/Details/5
        // 根據傳入的產品 ID 取得產品詳細資訊
        public async Task<IActionResult> Details(int? id)
        {
            Log.Information($"Products/Details/{id}");

            if (id == null)
            {
                Log.Error($"Product with ID {id} not found.");               
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductModel)
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        // 顯示新增產品的頁面
        public IActionResult Create()
        {
            Log.Information("Products/Create");

            // 使用 SelectList 來為 ProductCategory 和 ProductModel 提供下拉選單選項
            ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategory, "ProductCategoryID", "Name");
            ViewData["ProductModelID"] = new SelectList(_context.ProductModel, "ProductModelID", "Name");            
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // 用於處理提交的產品創建請求，並防止過度提交攻擊
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductID,Name,ProductNumber,Color,StandardCost,ListPrice,Size,Weight,ProductCategoryID,ProductModelID,SellStartDate,SellEndDate,DiscontinuedDate,ThumbNailPhoto,ThumbnailPhotoFileName,rowguid,ModifiedDate")] Product product)
        {
            Log.Information("Products/Create");
            
            // 檢查模型狀態是否有效
            if (ModelState.IsValid)
            {
                try
                {
                    // 如果資料有效，將新產品添加到資料庫並保存
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index)); // 保存成功後返回產品列表
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "建立產品時發生錯誤。");
                }
            }

            // 如果提交資料有誤，重新加載下拉選單選項並返回創建頁面
            ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategory, "ProductCategoryID", "Name", product.ProductCategoryID);
            ViewData["ProductModelID"] = new SelectList(_context.ProductModel, "ProductModelID", "Name", product.ProductModelID);            
            return View(product);
        }

        // GET: Products/Edit/5
        // 根據產品 ID 顯示編輯產品頁面
        public async Task<IActionResult> Edit(int? id)
        {
            Log.Information($"Products/Edit/{id}");

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // 使用 SelectList 為編輯頁面提供下拉選單選項
            ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategory, "ProductCategoryID", "Name", product.ProductCategoryID);
            ViewData["ProductModelID"] = new SelectList(_context.ProductModel, "ProductModelID", "Name", product.ProductModelID);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // 用於處理提交的產品編輯請求
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,Name,ProductNumber,Color,StandardCost,ListPrice,Size,Weight,ProductCategoryID,ProductModelID,SellStartDate,SellEndDate,DiscontinuedDate,ThumbNailPhoto,ThumbnailPhotoFileName,rowguid,ModifiedDate")] Product product)
        {
            Log.Information($"Products/Edit/{id}");

            // 如果 ID 不匹配，返回 404
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 更新產品並保存變更
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Log.Error(ex, "資料庫更新時發生錯誤");

                    // 如果產品不存在，捕獲例外並返回 404
                    if (!ProductExists(product.ProductID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); // 保存成功後返回產品列表
            }

            //  如果提交資料有誤，重新加載下拉選單選項並返回編輯頁面
            ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategory, "ProductCategoryID", "Name", product.ProductCategoryID);
            ViewData["ProductModelID"] = new SelectList(_context.ProductModel, "ProductModelID", "Name", product.ProductModelID);
            return View(product);
        }

        // GET: Products/Delete/5
        // 顯示刪除確認頁面，根據產品 ID 取得產品
        public async Task<IActionResult> Delete(int? id)
        {
            Log.Information($"Products/Delete/{id}");

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductModel)
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        // 確認刪除產品，根據產品 ID 刪除
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Log.Information($"Products/DeleteConfirmed/{id}");

            try
            {
                var product = await _context.Product.FindAsync(id);
                if (product != null)
                {
                    _context.Product.Remove(product);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {   
                Log.Error(ex, $"刪除 ID 為 {id} 的產品時發生錯誤。");
                return StatusCode(500, "伺服器內部錯誤，請稍後再試。");
            }
        }

        // 檢查產品是否存在
        private bool ProductExists(int id)
        {
            Log.Information($"ProductExists Product {id}");
            return _context.Product.Any(e => e.ProductID == id);
        }
    }
}
