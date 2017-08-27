using Auction.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Auction.Hubs
{
	[HubName("AuctionHub")]
	public class AuctionHub : Hub
	{
		private static bool IsRunning = false;
		private static List<Participant> Participants = new List<Participant>();
		private static DateTime end;
		private static Participant participant = new Participant() { Id = "", Email = "" };
		private ApplicationDbContext context = new ApplicationDbContext();
		private static AuctionInfo auctionInfo = new AuctionInfo() { Product = "" };
		private static Models.Auction CurrentAuction = new Models.Auction() { Price = 100 };

		[HubMethodName("Connect")]
		public void Connect(string email)
		{			
			string id = Context.ConnectionId;
			if (!Participants.Any(u => u.Email == email))
			{
				Participants.Add(new Participant() { Id = id, Email = email });
			}
			else
			{
				Participants.Where(p => p.Email == email).First().Id = Context.ConnectionId;
			}
			Clients.Caller.TimeReceiver("00.00.00");
			Clients.All.ReceiveInfo(CurrentAuction.Price, participant.Email, auctionInfo.Product);
		}

		[HubMethodName("AuctionStart")]
		public async Task<string> AuctionStart(string product_name)
		{
			IsRunning = true;
			Clients.Caller.DisableButtons();
			end = Models.Timer.GetTimer();
			auctionInfo = context.AuctionInfos.Where(a => a.Product == product_name).First();
			CurrentAuction = new Models.Auction() { AuctionStart = DateTime.Now, Price = 100, AuctionInfo = auctionInfo };
			Clients.All.ReceiveInfo(CurrentAuction.Price, "", auctionInfo.Product);
			while (TimeLeft > TimeSpan.Zero)
			{
				Clients.All.TimeReceiver(TimeLeft.ToString().Substring(0, 8));
				Thread.Sleep(1000);
			}
			string info;
			if (participant.Email == "")
			{
				info = "Никто не приобрел продукт " + CurrentAuction.AuctionInfo.Product;
				Models.Timer.ResetTimer();
				Clients.AllExcept(Context.ConnectionId).AuctionEnd(info);
				Clients.Caller.EnableButtons();
				return info;
			}
			await End();
			info = new StringBuilder().Append("Пользователь ")
				.Append(CurrentAuction.User.Email)
				.Append(" приобрел ")
				.Append(CurrentAuction.AuctionInfo.Product)
				.Append(" за ")
				.Append(CurrentAuction.Price)
				.ToString();
			Clients.AllExcept(Context.ConnectionId).AuctionEnd(info);
			Clients.Caller.EnableButtons();
			return info;
		}

		//[HubMethodName("AuctionStart")]
		//public async Task<string> AuctionStart(string product_name)
		//{
		//	IsRunning = true;
		//	end = Models.Timer.GetTimer();
		//	CurrentAuction = new Models.Auction() { AuctionStart = DateTime.Now };
		//	auctionInfo = context.AuctionInfos.Where(a => a.Product == product_name).First();
		//	Clients.All.ReceiveInfo(price, "", auctionInfo.Product);
		//	while (TimeLeft > TimeSpan.Zero)
		//	{
		//		Clients.All.TimeReceiver(TimeLeft.ToString().Substring(0, 8));
		//		Thread.Sleep(100);
		//	}
		//	await End();
		//	if (participant == null)
		//	{
		//		Clients.AllExcept(Context.ConnectionId).AuctionEnd(CurrentAuction.Price, CurrentAuction.Winner, CurrentAuction.AuctionInfo.Product);
		//	}

		//	return CurrentAuction.User.Email + "  " + CurrentAuction.Price + "  " + CurrentAuction.AuctionInfo.Product;
		//}

		private async Task End()
		{				
			Models.Timer.ResetTimer();
			CurrentAuction.AuctionEnd = DateTime.Now;
			CurrentAuction.AuctionInfo = auctionInfo;
			context.Auctions.Add(CurrentAuction);
			ApplicationUserManager manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
			ApplicationUser user = await manager.FindByNameAsync(participant.Email);
			CurrentAuction.User = user;
			CurrentAuction.AuctionInfo = auctionInfo;
			context.Auctions.Add(CurrentAuction);
			context.SaveChanges();
			foreach (Participant p in Participants)
			{
				ApplicationUser applicationUser = await manager.FindByNameAsync(p.Email);
				context.AuctionUsers.Add(new AuctionUser() { User = applicationUser, Auction = CurrentAuction });
			}
			IsRunning = false;
			participant = new Participant() { Id = "", Email = "" };
			context.SaveChanges();
		}

		[HubMethodName("ToDouble")]
		public void ToDouble()
		{
			try
			{
				if (IsRunning && Participants.Contains(Participants.Where(p => p.Id == Context.ConnectionId).First()))
				{
					string id = Context.ConnectionId;
					participant = Participants.Where(p => p.Id == id).First();
					CurrentAuction.Price *= 2;
					CurrentAuction.OfferCount++;
					Models.Timer.UpdateTimer();
					Clients.All.ReceiveInfo(CurrentAuction.Price, participant.Email, auctionInfo.Product);
				}
			}
			catch
			{

			}
		}

		[HubMethodName("ToOffer")]
		public void ToOffer(int price)
		{
			try
			{
				if (IsRunning && Participants.Contains(Participants.Where(p => p.Id == Context.ConnectionId).First()))
				{
					if (price - CurrentAuction.Price < 5)
					{
						return;
					}
					participant = Participants.Where(p => p.Id == Context.ConnectionId).First();
					if (price / CurrentAuction.Price >= 2)
					{
						Models.Timer.UpdateTimer();
					}
					CurrentAuction.Price = price;
					CurrentAuction.OfferCount++;
					Clients.All.ReceiveInfo(CurrentAuction.Price, participant.Email, auctionInfo.Product);
				}
			}
			catch
			{

			}
		}
		private TimeSpan TimeLeft => Models.Timer.GetTimer() - DateTime.Now;
	}
}