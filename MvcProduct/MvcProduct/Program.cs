using Microsoft.EntityFrameworkCore;
using MvcProduct.Models;
using Serilog;

namespace MvcProduct
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 設定資料庫連線字串 (appsettings.json)
            builder.Services.AddDbContext<AdventureWorksLT2022Context>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //ConnectionStrings:DefaultConnection


            // 建立一個 ConfigurationBuilder 物件，並載入 appsettings.json 檔案。
            // optional: true 表示該檔案為選擇性載入，如果檔案不存在也不會拋出錯誤。
            // reloadOnChange: true 表示如果檔案內容變更，設定會自動重新載入。
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            string? logPath = configurationBuilder.GetSection("LogPath").Value;

            // 設定 Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // 設定最低等級為 Information。此設定確保只記錄 Information 等級及以上的 log（Warning、Error、Fatal），而忽略較低的等級如 Debug 和 Verbose。
                .WriteTo.File($"{logPath}/log.txt", rollingInterval: RollingInterval.Day) // 指定將日誌寫入 {logPath}/log.txt，並每天滾動產生新檔案。
                .CreateLogger();

            Log.Information("作業02：維護(產品)的網站");
            Log.Information($"logPath：{logPath}");


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
