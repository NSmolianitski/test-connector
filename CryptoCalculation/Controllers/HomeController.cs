using CryptoCalculation.Models;
using CryptoCalculation.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoCalculation.Controllers;

[Route("/")]
public class HomeController(CryptoService cryptoService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var currentBalance = new Dictionary<string, decimal>
        {
            {"BTC", 1},
            {"XRP", 15000},
            {"XMR", 50},
            {"DSH", 30}
        };

        var totalBalance = await cryptoService.CalculateTotalBalance(currentBalance);
        return View(new TotalPortfolioBalance(totalBalance));
    }
}