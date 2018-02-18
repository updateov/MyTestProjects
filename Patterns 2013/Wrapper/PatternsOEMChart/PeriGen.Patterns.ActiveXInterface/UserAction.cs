using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

namespace PeriGen.Patterns.ActiveXInterface
{
	/// <summary>
	/// The different type of action
	/// </summary>
	public enum ActionTypes : byte
	{
		None = 0,
		StrikeoutContraction = 1,
		StrikeoutEvent = 2,
		ConfirmEvent = 3,
		UndoStrikeoutEvent = 4,
		UndoStrikeoutContraction = 5,
	}

	/// <summary>
	/// A stub for User action for testing
	/// </summary>
	public class XUserAction
	{
		/// <summary>
		/// Unique Id
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Type
		/// </summary>
		public ActionTypes ActionType { get; set; }

		/// <summary>
		/// Related artifact ID
		/// </summary>
		public int ArtifactId { get; set; }

		/// <summary>
		/// Related patient ID
		/// </summary>
		public int PatientId { get; set; }

		/// <summary>
		/// User ID
		/// </summary>
		public string UserId { get; set; }

		/// <summary>
		/// User name (label)
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// Performed time
		/// </summary>
		public DateTime PerformedTime { get; set; }

		/// <summary>
		/// Encode a UserAction into an XElement that can be sent to the patterns activex
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public virtual XElement EncodeForActiveX()
		{
			return new XElement("action",
				new XAttribute("type", ((int)this.ActionType).ToString(CultureInfo.InvariantCulture)),
				new XAttribute("artifact", this.ArtifactId.ToString(CultureInfo.InvariantCulture)),
				new XAttribute("userid", this.UserId),
				new XAttribute("username", this.UserName),
				new XAttribute("performed", this.PerformedTime.ToEpoch().ToString(CultureInfo.InvariantCulture)));
		}

		/// <summary>
		/// Serialize the given user actions
		/// </summary>
		public static XElement EncodeForActiveX(IEnumerable<XUserAction> actions)
		{
			var element = new XElement("actions");
			if (actions != null)
			{
				foreach (var action in actions)
				{
					element.Add(action.EncodeForActiveX());
				}
			}

			return element;
		}

		/// <summary>
		/// From an XML element to an action...
		/// </summary>
		/// <param name="element"></param>
		public static XUserAction FromActiveX(XElement element)
		{
			Debug.Assert(element.Name.ToString().ToUpperInvariant() == "ACTION");

			return new XUserAction
			{
				ActionType = (ActionTypes)Enum.ToObject(typeof(ActionTypes), Convert.ToByte(element.Attribute("type").Value, CultureInfo.InvariantCulture)),
				ArtifactId = Convert.ToInt32(element.Attribute("artifact").Value, CultureInfo.InvariantCulture),
				PatientId = Convert.ToInt32(element.Attribute("patient").Value, CultureInfo.InvariantCulture),
				UserId = element.Attribute("userid").Value,
				UserName = element.Attribute("username").Value,
				PerformedTime = DateTime.UtcNow
			};
		}
	}

    public class XExtendedUserAction : XUserAction
    {
        /// <summary>
        /// Earliest requested artifact
        /// </summary>
        public DateTime StartPeriod { get; set; }

        /// <summary>
        /// Latest requested artifact
        /// </summary>
        public DateTime EndPeriod { get; set; }

        public override XElement EncodeForActiveX()
        {
            XElement toRet = base.EncodeForActiveX();
            toRet.SetAttributeValue("startTime", StartPeriod.ToEpoch());
            toRet.SetAttributeValue("endTime", EndPeriod.ToEpoch());
            return toRet;
        }

        /// <summary>
        /// Serialize the given user actions
        /// </summary>
        public static XElement EncodeForActiveX(IEnumerable<XExtendedUserAction> actions)
        {
            var element = new XElement("actions");
            if (actions != null)
            {
                foreach (var action in actions)
                {
                    element.Add(action.EncodeForActiveX());
                }
            }

            return element;
        }

        /// <summary>
        /// From an XML element to an action...
        /// </summary>
        /// <param name="element"></param>
        public static new XExtendedUserAction FromActiveX(XElement element)
        {
            Debug.Assert(element.Name.ToString().ToUpperInvariant() == "ACTION");

            return new XExtendedUserAction
            {
                ActionType = (ActionTypes)Enum.ToObject(typeof(ActionTypes), Convert.ToByte(element.Attribute("type").Value, CultureInfo.InvariantCulture)),
                ArtifactId = Convert.ToInt32(element.Attribute("artifact").Value, CultureInfo.InvariantCulture),
                PatientId = Convert.ToInt32(element.Attribute("patient").Value, CultureInfo.InvariantCulture),
                UserId = element.Attribute("userid").Value,
                UserName = element.Attribute("username").Value,
                StartPeriod = Convert.ToInt64(element.Attribute("startTime").Value, CultureInfo.InvariantCulture).ToDateTime(),
                EndPeriod = Convert.ToInt64(element.Attribute("endTime").Value, CultureInfo.InvariantCulture).ToDateTime(),
                PerformedTime = DateTime.UtcNow
            };
        }
    }
}
