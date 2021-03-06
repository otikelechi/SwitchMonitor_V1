﻿using System;

namespace SwitchMonitor_v1.Model
{
    public class Node
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }
        public string ConnectionType { get; set; }
        public string IPAddress { get; set; }
        public bool IsConnected { get; set; }
        public bool OnIPNX { get; set; }
        public DateTime TimeRecorded { get; set; }

        public static explicit operator Node(NodeResult v)
        {
            throw new NotImplementedException();
        }
    }
}