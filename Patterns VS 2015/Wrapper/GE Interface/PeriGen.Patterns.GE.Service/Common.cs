namespace PeriGen.Patterns.GE.Service
{
    internal static class Common
    {
		/// <summary>
		/// In demo mode or registered mode?
		/// </summary>
		public static bool DemoMode { get; set; }

        /// <summary>
        /// Chalkboard
        /// </summary>
        public static PatternsChalkboard Chalkboard { get; set; }
    }
}
