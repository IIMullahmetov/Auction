using System;

namespace Auction.Models
{
	public class Timer
	{
		private static DateTime? end = null;

		public static DateTime GetTimer()
		{
			if(end == null)
			{
				end = DateTime.Now.Add(new TimeSpan(0, 1, 0));
				return end.Value;
			}
			return end.Value;
		}

		public static void UpdateTimer() => end = end.Value.Add(new TimeSpan(0, 1, 0));
	}
}