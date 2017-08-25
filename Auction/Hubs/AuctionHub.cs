using Auction.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Auction.Hubs
{
	[HubName("AuctionHub")]
	public class AuctionHub : Hub
	{
		private static List<Participant> Participants = new List<Participant>();
		private static DateTime end;
		private static Participant participant = null;
		private static int price = 100;
		public static Models.Auction Auction { get; set; }
		[HubMethodName("BroadcastReceiveTime")]
		public void BroadcastReceiveTime()
		{
			Clients.All.TimeReceiver(DateTime.Now);
		}

		[HubMethodName("Connect")]
		public void Connect(string email)
		{
			string id = Context.ConnectionId;
			if (!Participants.Any(u => u.Id == id))
			{
				Participants.Add(new Participant() { Id = id, Email = email });
				Clients.Caller.TimeReceiver(TimeLeft.ToString().Substring(0, 8));
				if (participant != null)
				{
					Clients.All.ReceiveInfo(price, participant.Email);
				}
				else
				{
					Clients.All.ReceiveInfo(price, "");
				}
			}
		}

		[HubMethodName("AuctionStart")]
		public void AuctionStart()
		{
			end = DateTime.Now.Add(new TimeSpan(0, 1, 0));
			while(TimeLeft > TimeSpan.Zero)
			{
				Clients.All.TimeReceiver(TimeLeft.ToString().Substring(0, 8));
				Clients.All.ReceiveInfo(price, "");
			}
		}

		[HubMethodName("ToDouble")]
		public void ToDouble()
		{
			string id = Context.ConnectionId;
			participant = Participants.Where(p => p.Id == id).First();
			price *= 2;
			Clients.All.ReceiveInfo(price, participant.Email);
		}
		[HubMethodName("ToOffer")]
		public void ToOffer(int price)
		{
			string id = Context.ConnectionId;
			participant = Participants.Where(p => p.Id == id).First();
			AuctionHub.price = price;
			Clients.All.ReceiveInfo(AuctionHub.price, participant.Email);
		}

		private static TimeSpan TimeLeft => end - DateTime.Now;		
	}
}