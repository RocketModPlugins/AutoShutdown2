using System;
using System.Collections.Generic;

using SDG.Unturned;

using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Core.Plugins;
using Rocket.Core.Logging;

using UnityEngine;

using System.Reflection;

namespace falsechicken.AutoShutdown2
{
	public class AutoShutdown2 : RocketPlugin<AutoShutdown2Configuration>
	{

		#region STORAGE VARS

		private byte currentHour, currentMinutes, currentSeconds; //The current hour, minutes, and seconds for fast lookup later.
		
		private Dictionary<byte, List<ShutdownWarning>> warningHourTable; //Cache the warnings for each hour for faster lookups.
		private Dictionary<byte, List<ShutdownTime>> shutdownHourTable; //Cache the shut down times for each hour for faster lookups.
		
		private DateTime lastCalled; //Used to store when the last checks where performed. We only want to update once per second.
		private DateTime now; //Used to store the time right now.

        private DateTime shutdown;
        private bool timer = false;
		#endregion

		#region ROCKET FUNCTIONS

		protected override void Load()
		{
            timer = Configuration.Instance.EnableTimer && Configuration.Instance.Timer > 0;
            ShowLoadedMessage();
            if (timer)
            {
                shutdown = DateTime.Now.AddSeconds(Configuration.Instance.Timer);
                Rocket.Core.Logging.Logger.Log("Will shutdown at "+shutdown.ToString());
            }
            else
            {
			    PopulateCacheTables();
            }
            UpdateTimeVariables();
            UpdateLastCalledTime();
        }

		void FixedUpdate()
        {
            now = DateTime.Now;
            if ((now - lastCalled).TotalSeconds > 1) //Check once per second.
            {
                if (timer)
                {
                    if(now > shutdown)
                    {
                        ShowShutdownMessageToChat();
                        ShutdownServer();
                    }else
                    {
                        double v = (shutdown - now).TotalSeconds;
                        foreach(ShutdownWarning w in Configuration.Instance.ShutdownWarnings)
                        {
                            if(!w.shown && w.timer > v)
                            {
                                UnturnedChat.Say(w.message, UnturnedChat.GetColorFromName(w.color, Color.green));
                                w.shown = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    UpdateTimeVariables();
                    CheckWarnings();
                    CheckShutdowns();
                }
                UpdateLastCalledTime();
            }
		}

		#endregion

		#region CORE PLUGIN FUNCTIONS

		/**
		 * Check the shutdown cache table to see if we have any shutdowns set for this hour. Then check the minute is correct before shutting down.
		 */
		private void CheckShutdowns()
		{
			if (shutdownHourTable[currentHour].Count < 1) return; //If there are no shutdowns for this hour return.

			foreach (ShutdownTime sT in shutdownHourTable[currentHour])
			{  
				if (sT.minutes == currentMinutes && currentSeconds == 0)
				{
					ShowShutdownMessageToChat();
				
					ShutdownServer();
				}
			}
		}
		
		/**
		 * Check the warning cache table to see if we have any warnings set for this hour. Then check if the minute is correct before printing.
		 */
		private void CheckWarnings()
		{
			if (warningHourTable [currentHour].Count < 1) return; //If there are no warnings for this hour return.

			foreach (ShutdownWarning sW in warningHourTable[currentHour])
			{
				if (sW.minute == currentMinutes && currentSeconds == 0)
				{
					UnturnedChat.Say(sW.message, UnturnedChat.GetColorFromName(sW.color, Color.green));
				}
			}
		}

		/**
		 * Initialize and populate the cache tables for use.
		 */
		private void PopulateCacheTables()
		{
			warningHourTable = new Dictionary<byte, List<ShutdownWarning>>();
			shutdownHourTable = new Dictionary<byte, List<ShutdownTime>>();
			
			for (byte hour = 0; hour < 24; hour++) //Populate the shutdown and warning cache tables with keys 0 - 23 to represent the hours of the day.
			{
				warningHourTable.Add(hour, new List<ShutdownWarning>());
				shutdownHourTable.Add(hour, new List<ShutdownTime>());
			}

			foreach (ShutdownWarning sW in this.Configuration.Instance.ShutdownWarnings)
			{
				warningHourTable[sW.hour].Add(sW);
			}

			foreach (ShutdownTime sT in this.Configuration.Instance.ShutdownTimes)
			{
				shutdownHourTable[sT.hour].Add(sT);
			}
		}
	
		/**
		 * Update the last called variable. Used to make sure we only check the time once per second.
		 */
		private void UpdateLastCalledTime()
		{
			lastCalled = DateTime.Now;
		}

		/**
		 * Update the variables for the hour, minutes, and seconds for faster lookup.
		 */
		private void UpdateTimeVariables()
		{
			currentHour = (byte) now.Hour;
			currentMinutes = (byte) now.Minute;
			currentSeconds = (byte) now.Second;
		}

		/**
		 * Shut the server down.
		 */
		private void ShutdownServer()
		{
			SDG.Unturned.SaveManager.save();
			Provider.shutdown();
		}

		/**
		 * Print a message to the console informing the user that the plugin has loaded.
		 */
		private void ShowLoadedMessage()
		{
            Rocket.Core.Logging.Logger.Log(" Version " + Assembly.GetExecutingAssembly().GetName().Version + " loaded.");
		}

		/**
		 * Show the shut down message to chat.
		 */
		private void ShowShutdownMessageToChat()
		{
			UnturnedChat.Say("Automatic server shut down in progress...", UnturnedChat.GetColorFromName(this.Configuration.Instance.ShutdownMessageColor, Color.green));
		}

		#endregion

	}
}