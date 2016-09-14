using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using SwitchMonitor_v1.Data;

namespace SwitchMonitor_v1.Web
{
    public class SwitchMonitorHub : Hub
    {
        public void GetAllNodes()
        {
            var returnedObject = NodeData.getAllNodes();
            var statusMesssage = returnedObject.Item3;
            var allUsedNodes = returnedObject.Item1;
            var notUsed = JsonConvert.SerializeObject(returnedObject.Item2);
            var nodes = JsonConvert.SerializeObject(allUsedNodes);
            var recentUpdates = NodeData.GetRecentUpdates();
            var updates = JsonConvert.SerializeObject(recentUpdates);
            Clients.All.getAllNodes(nodes, updates, notUsed, statusMesssage);
        }
    }
}