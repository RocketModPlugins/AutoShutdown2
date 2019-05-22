using System;
using System.Xml.Serialization;

using Rocket.API;

namespace falsechicken.AutoShutdown2
{
	public sealed class ShutdownTime
	{
		[XmlAttribute("Hour")]
		public byte hour;
        	
		[XmlAttribute("Minute")]
		public byte minutes;

		public ShutdownTime(byte _hour, byte _minute)
		{
			hour = _hour;
			minutes = _minute;
		}
		
		public ShutdownTime()
		{
			hour = 0;
			minutes = 0;
		}
	}
		
	public sealed class ShutdownWarning
	{
		[XmlAttribute("Hour")]
		public byte hour;
				
		[XmlAttribute("Minute")]
		public byte minute;

        [XmlAttribute("Message")]
        public string message;
        
        [XmlAttribute("Timer")]
        public long timer;

        [XmlAttribute("Color")]
		public string color;

        [NonSerialized]
        internal bool shown = false;

        public ShutdownWarning(byte _hour, byte _minute, long _timer, string _message, string _color)
		{
			hour = _hour;
			minute = _minute;
            message = _message;
            timer = _timer;
            color = _color;
		}
		
		public ShutdownWarning()
		{
			hour = 0;
			minute = 0;
			message = "";
			color = "";
		}
	}
		
	public class AutoShutdown2Configuration : IRocketPluginConfiguration
	{
		public string ShutdownMessageColor;

		[XmlArrayItem("Shutdown_Time")]
        [XmlArray(ElementName = "Shutdown_Times")]
        public ShutdownTime[] ShutdownTimes;
        
        [XmlArrayItem("Shutdown_Warning")]
        [XmlArray(ElementName = "Shutdown_Warnings")]
        public ShutdownWarning[] ShutdownWarnings;

        public bool EnableTimer = false;
        public long Timer = 86400;

        public void LoadDefaults()
		{
			ShutdownMessageColor = "Red";

			ShutdownTimes = new ShutdownTime[]
			{
				new ShutdownTime(12, 0),
			};

            EnableTimer = false;
            Timer = 86400;

            ShutdownWarnings = new ShutdownWarning[]
			{
				new ShutdownWarning(11, 55, 300,"Automatic shutdown in 5 minutes.", "Green"),
				new ShutdownWarning(11, 57, 180,"Automatic shutdown in 3 minutes.", "Green"),
				new ShutdownWarning(11, 59, 60,"Automatic shutdown in 1 minute.", "Green"),
			};			
		}
	}
}
