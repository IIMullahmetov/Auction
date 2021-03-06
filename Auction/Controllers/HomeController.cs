﻿using Auction.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Mvc;

namespace Auction.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			
			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}

		[Authorize(Roles = "USER")]
		public ActionResult Auction()
		{
			ViewBag.Message = "Auction";
			return View();
		}

		[Authorize(Roles = "ADMIN")]
		public ActionResult Auctions()
		{
			AuctionManager manager = new AuctionManager();
			return View(manager.GetAuctions());
		}
	}
}