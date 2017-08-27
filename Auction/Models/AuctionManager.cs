using System;
using System.Collections.Generic;

namespace Auction.Models
{
	public class AuctionManager
	{
		private ApplicationDbContext context;

		public AuctionManager() => context = new ApplicationDbContext();

		public List<Tuple<DateTime, DateTime, int, int, string>> GetAuctions()
		{
			List<Tuple<DateTime, DateTime, int, int, string>> auctions = new List<Tuple<DateTime, DateTime, int, int, string>>();
			foreach(Auction auction in context.Auctions)
			{
				auctions.Add(new Tuple<DateTime, DateTime, int, int, string>(auction.AuctionStart, auction.AuctionEnd, auction.Price, auction.OfferCount, auction.User.Email));
			}
			return auctions;
		}
	}
}