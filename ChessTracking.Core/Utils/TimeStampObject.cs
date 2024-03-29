﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Utils;

/// <summary>
/// Holder for object T containing timestamp
/// </summary>
class TimestampObject<T>
{
    public DateTime Timestamp { get; set; }
    public T StoredObject { get; set; }

    public TimestampObject(T obj, DateTime timestamp)
    {
        StoredObject = obj;
        Timestamp = timestamp;
    }

    public TimestampObject(T obj)
    {
        StoredObject = obj;
        Timestamp = DateTime.Now;
    }
}
