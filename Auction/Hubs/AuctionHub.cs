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
		private static Participant participant = null;
		private static int price = 100;
		private ApplicationDbContext context = new ApplicationDbContext();
		private static AuctionInfo auctionInfo;
		private static Models.Auction CurrentAuction;

		[HubMethodName("Connect")]
		public void Connect(string email)
		{
			string id = Context.ConnectionId;
			if (!Participants.Any(u => u.Email == email))
			{
				Participants.Add(new Participant() { Id = id, Email = email });
				Clients.Caller.TimeReceiver("00.00.00");
				if (participant != null)
				{
					Clients.All.ReceiveInfo(price, participant.Email);
				}
				else
				{
					Clients.All.ReceiveInfo("", "");
				}
			}
			else
			{
				Participants.Where(p => p.Email == email).First().Id = Context.ConnectionId;
			}
		}

		[HubMethodName("AuctionStart")]
		public async Task<string> AuctionStart(string product_name)
		{
			if (!IsRunning)
			{
				IsRunning = true;
				end = Models.Timer.GetTimer();
				CurrentAuction = new Models.Auction() { AuctionStart = DateTime.Now };
				auctionInfo = context.AuctionInfos.Where(a => a.Product == product_name).First();
				Clients.All.ReceiveProductInfo(auctionInfo.Product);
				Clients.All.ReceiveInfo(price, "", auctionInfo.Product);
				while (TimeLeft > TimeSpan.Zero)
				{
					Clients.All.TimeReceiver(TimeLeft.ToString().Substring(0, 8));
					Thread.Sleep(1000);
				}
				string info;
				if (participant == null)
				{
					info = "Никто не приобрел продукт " + CurrentAuction.AuctionInfo.Product;
					Clients.AllExcept(Context.ConnectionId).AuctionEnd(info);
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
				return info;
			}
			return "Аукцион еще идет";
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
			IsRunning = false;
			CurrentAuction.AuctionEnd = DateTime.Now;
			ApplicationUserManager manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
			ApplicationUser user = await manager.FindByNameAsync(participant.Email);
			CurrentAuction.User = user;
			CurrentAuction.Price = price;
			CurrentAuction.AuctionInfo = auctionInfo;
			context.Auctions.Add(CurrentAuction);
			context.SaveChanges();
			foreach (Participant p in Participants)
			{
				ApplicationUser applicationUser = await manager.FindByNameAsync(p.Email);
				context.AuctionUsers.Add(new AuctionUser() { User = applicationUser, Auction = CurrentAuction });
			}
			context.SaveChanges();
		}

		[HubMethodName("ToDouble")]
		public void ToDouble()
		{
			if (IsRunning && Participants.Contains(Participants.Where(p => p.Id == Context.ConnectionId).First()))
			{
				string id = Context.ConnectionId;
				participant = Participants.Where(p => p.Id == id).First();
				price *= 2;
				Models.Timer.UpdateTimer();
				Clients.All.ReceiveInfo(price, participant.Email);
			}
		}

		[HubMethodName("ToOffer")]
		public void ToOffer(int price)
		{
			if (IsRunning && Participants.Contains(Participants.Where(p => p.Id == Context.ConnectionId).First()))
			{
				string id = Context.ConnectionId;
				participant = Participants.Where(p => p.Id == id).First();
				if (price / AuctionHub.price >= 2)
				{
					Models.Timer.UpdateTimer();
				}
				AuctionHub.price = price;
				Clients.All.ReceiveInfo(AuctionHub.price, participant.Email);
			}
		}

		private TimeSpan TimeLeft => Models.Timer.GetTimer() - DateTime.Now;
	}
}