﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.Entities
{
    public class UserDetails
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Function { get; set; }
        public int Action { get; set; }
    }
}
