﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Model
{
    public enum PingProtocol
    {
        ICMP,
        TCP
    }

    public enum PingerStatus
    {
        Started,
        Stopped
    }
}
