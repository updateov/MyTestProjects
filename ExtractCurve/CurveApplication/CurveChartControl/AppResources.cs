﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CurveChartControl
{
    /// <summary>
    /// Provides access to Global Language Manager
    /// </summary>
	public class AppResources
    {
        public AppResources()
        {
        }
        static Languages languageManager = null;

        public static Languages LanguageManager 
        {
            get 
            {
                if (languageManager == null)
                {
                    languageManager = new Languages();
                }
                return languageManager;
            }
        }
    }
}
