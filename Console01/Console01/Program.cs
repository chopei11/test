using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Console01
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("作業01：檔案搬移的程式");
            /* 作業一回饋#1，20241003
                1.destinationDirectory = null 的時候，應該要正常處理例外，不應該拋出 Exception。
                2.用迴圈搬移檔案時，若有某一檔案發生異常，會造成中斷，後續檔案無法繼續處理。
                3.可以多新增將 Log 寫入到實體檔案的功能。
            */

            try
            {
                // 設定檔讀取
                // 建立一個 ConfigurationBuilder 物件，並載入 appsettings.json 檔案。
                // optional: true 表示該檔案為選擇性載入，如果檔案不存在也不會拋出錯誤。
                // reloadOnChange: true 表示如果檔案內容變更，設定會自動重新載入。
                var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

                // 從 appsettings.json 取得來源和目的地目錄
                string? sourceDirectory = builder.GetSection("FileMoverSettings:SourceDirectory").Value;
                string? destinationDirectory = builder.GetSection("FileMoverSettings:DestinationDirectory").Value;
                string? logPath = builder.GetSection("FileMoverSettings:LogPath").Value;

                // 建立DI容器
                var serviceProvider = new ServiceCollection()
                    .AddLogging(configure => configure.AddConsole()) // 使用 Console logger
                    .BuildServiceProvider();
                // Default: 將最低日誌級別設為 Information，這樣程式會記錄 Information 及以上的所有日誌 (Information、Warning、Error、Critical)。
                // Microsoft: 將 Microsoft 內建的日誌級別設為 Warning，這樣可以避免記錄太多 Microsoft 相關的詳細訊息。
            
                // 取得logger
                // var logger = serviceProvider.GetService<ILogger<Program>>();

                // 20241012 調整：設定 Serilog 日誌配置。安裝套件 (using Serilog;)
                Log.Logger = new LoggerConfiguration()
                   .WriteTo.File($"{logPath}/log.txt", rollingInterval: RollingInterval.Day)
                   .CreateLogger();
     
                //logger.LogInformation("config 來源目錄 {sourceDirectory}", sourceDirectory);
                //logger.LogInformation("config 目的地目錄 {destinationDirectory}", destinationDirectory);
                Log.Information("作業01：檔案搬移的程式");
                Log.Information("config 來源目錄 {sourceDirectory}", sourceDirectory);
                Log.Information("config 目的地目錄 {destinationDirectory}", destinationDirectory);

                // 確認來源目錄、目的地目錄是否存在
                if (string.IsNullOrEmpty(sourceDirectory) || !Directory.Exists(sourceDirectory))
                {
                    Log.Error($"來源目錄不存在：{sourceDirectory}");   
                    return;
                }

                if (string.IsNullOrEmpty(destinationDirectory) || !Directory.Exists(destinationDirectory))
                {
                    Log.Error($"目的地目錄不存在：{destinationDirectory}");
                    return;
                }


                // 取得來源目錄中的所有檔案
                string[] files = Directory.GetFiles(sourceDirectory);

                foreach (var file in files)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        string destFile = Path.Combine(destinationDirectory, fileName);

                        // 複製檔案到目的地目錄
                        //File.Copy(file, destFile);
                        // 複製檔案到目的地目錄。檢查目標檔案是否存在，若存在則覆寫
                        File.Copy(file, destFile, true);
                        Log.Information($"{fileName} 檔案已複製");

                        // 刪除來源目錄中的檔案
                        File.Delete(file);
                        Log.Information($"{fileName} 檔案已刪除");
                    }
                    catch (Exception ex) 
                    {
                        Log.Error(ex, "搬移檔案發生例外錯誤：{Message}", ex.Message);
                    }                   
                }
            }
            catch (Exception ex) 
            {
                Log.Error(ex, "發生例外錯誤：{Message}", ex.Message);

            }
            finally
            {
                // 確保資源釋放
                //if (serviceProvider is IDisposable disposable)
                //{
                //    disposable.Dispose();
                //}
                Log.CloseAndFlush();
            }
        }
    }
}
