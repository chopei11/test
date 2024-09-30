using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Console01
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("作業01：檔案搬移的程式");

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

            // 建立DI容器
            var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole()) // 使用 Console logger
                .BuildServiceProvider();
            // Default: 將最低日誌級別設為 Information，這樣程式會記錄 Information 及以上的所有日誌 (Information、Warning、Error、Critical)。
            // Microsoft: 將 Microsoft 內建的日誌級別設為 Warning，這樣可以避免記錄太多 Microsoft 相關的詳細訊息。
            
            // 取得logger
            var logger = serviceProvider.GetService<ILogger<Program>>();


            try
            {
                logger.LogInformation("config 來源目錄 {sourceDirectory}", sourceDirectory);
                logger.LogInformation("config 目的地目錄 {destinationDirectory}", destinationDirectory);

                // 確認來源目錄是否存在
                if (!Directory.Exists(sourceDirectory))
                {
                    logger.LogError($"來源目錄不存在：{sourceDirectory}");   
                    return;
                }

                // 檢查或創建目的地目錄
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // 取得來源目錄中的所有檔案
                string[] files = Directory.GetFiles(sourceDirectory);

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(destinationDirectory, fileName);

                    // 複製檔案到目的地目錄
                    //File.Copy(file, destFile);
                    // 複製檔案到目的地目錄。檢查目標檔案是否存在，若存在則覆寫
                    File.Copy(file, destFile, true);
                    logger.LogInformation($"{fileName} 檔案已複製");

                    // 刪除來源目錄中的檔案
                    File.Delete(file);
                    logger.LogInformation($"{fileName} 檔案已刪除");                    
                }
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, "發生例外錯誤：{Message}", ex.Message);

            }
            finally
            {
                // 確保資源釋放
                if (serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }             
            }
        }
    }
}
