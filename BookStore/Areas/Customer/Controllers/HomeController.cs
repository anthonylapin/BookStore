using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BookStore.Models;
using BookStore.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookStore.Utility;
using Microsoft.AspNetCore.Http;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if(claim != null)
            {
                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == claim.Value)
                    .ToList().Count();

                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
            }

            return View(productList);
        }

        public IActionResult Details(int id)
        {
            var productFromDb = _unitOfWork.Product
                .GetFirstOrDefault(p => p.Id == id, includeProperties:"Category,CoverType");

            if(productFromDb == null)
            {
                return NotFound();
            }

            var cartObj = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id
            };

            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart cartObject)
        {
            cartObject.Id = 0;

            if(ModelState.IsValid)
            {
                // then add to cart
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cartObject.ApplicationUserId = claim.Value;

                var cartFromDb = _unitOfWork.ShoppingCart
                    .GetFirstOrDefault(
                        u => u.ApplicationUserId == cartObject.ApplicationUserId && u.ProductId == cartObject.ProductId,
                        includeProperties: "Product"
                    );

                if(cartFromDb == null)
                {
                    // no reсords exists in db for product for that user
                    _unitOfWork.ShoppingCart.Add(cartObject);
                }
                else
                {
                    cartFromDb.Count += cartObject.Count;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                }

                _unitOfWork.Save();

                int count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == cartObject.ApplicationUserId)
                    .ToList().Count();

                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);

                return RedirectToAction(nameof(Index));
            }

            var productFromDb = _unitOfWork.Product
               .GetFirstOrDefault(p => p.Id == cartObject.ProductId, includeProperties: "Category,CoverType");

            if (productFromDb == null)
            {
                return NotFound();
            }

            var cartObj = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id
            };

            return View(cartObj);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
