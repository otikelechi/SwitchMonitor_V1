using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using SwitchMonitor_v1.Model;
using SwitchMonitor_v1.Data;
using Newtonsoft.Json;

namespace SwitchMonitor_v1.Web
{
    public class SwitchMonitorHub : Hub
    {        
        public void GetAllNodes()
        {
            Tuple<List<Node>, List<Node>, string> returnedObject = NodeData.getAllNodes();
            string statusMesssage = returnedObject.Item3;
            List<Node> allUsedNodes = returnedObject.Item1;
            string notUsed = JsonConvert.SerializeObject(returnedObject.Item2);
            string nodes = JsonConvert.SerializeObject(allUsedNodes);
            object recentUpdates = NodeData.GetRecentUpdates();
            string updates = JsonConvert.SerializeObject(recentUpdates);
            Clients.All.getAllNodes(nodes,updates,notUsed,statusMesssage);

        }

        
    }
}