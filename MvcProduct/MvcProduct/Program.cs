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

            // �]�w��Ʈw�s�u�r�� (appsettings.json)
            builder.Services.AddDbContext<AdventureWorksLT2022Context>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //ConnectionStrings:DefaultConnection


            // �إߤ@�� ConfigurationBuilder ����A�ø��J appsettings.json �ɮסC
            // optional: true ��ܸ��ɮ׬���ܩʸ��J�A�p�G�ɮפ��s�b�]���|�ߥX���~�C
            // reloadOnChange: true ��ܦp�G�ɮפ��e�ܧ�A�]�w�|�۰ʭ��s���J�C
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            string? logPath = configurationBuilder.GetSection("LogPath").Value;

            // �]�w Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // �]�w�̧C���Ŭ� Information�C���]�w�T�O�u�O�� Information ���ŤΥH�W�� log�]Warning�BError�BFatal�^�A�ө������C�����Ŧp Debug �M Verbose�C
                .WriteTo.File($"{logPath}/log.txt", rollingInterval: RollingInterval.Day) // ���w�N��x�g�J {logPath}/log.txt�A�èC�Ѻu�ʲ��ͷs�ɮסC
                .CreateLogger();

            Log.Information("�@�~02�G���@(���~)������");
            Log.Information($"logPath�G{logPath}");


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
