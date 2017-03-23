using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using SwitchMonitor_v1.Data.NodeInfoService;
using SwitchMonitor_v1.Model;
using Node = SwitchMonitor_v1.Model.Node;

namespace SwitchMonitor_v1.Data
{
    public class NodeData
    {
        private static string[] GetNodeNotInUse()
        {
            try
            {
                var nodesNotInUse =
                    ConfigurationManager.AppSettings["notInUse"].Split(',').Select(s => s.Trim()).ToArray();
                return nodesNotInUse;
            }
            catch
            {
                return null;
            }
        }

        private static List<NodeResult> GetNodesConnectedToCentral()
        {
            using (var info = new InfoServiceClient())
            {
                List<NodeResult> result = new List<NodeResult>(200);
                NodeInfoService.Node[] activeNodes = null;
                try
                {
                    //var uptimeReport = info.GetServiceUptimeReport(DateTime.Now);
                    //uptimeReport = info.GetServiceUptimeReport(new DateTime(2016, 09, 01));
                    
                    
                    activeNodes = info.GetAllNodes();
                    
                    activeNodes = activeNodes.Where(x => x.IsActive).ToArray();
                    for (var i = 0; i <= activeNodes.Length - 1; i++)
                    {
                        if (activeNodes[i].ChildNodes!= null && !activeNodes[i].Name.Contains("IPNX"))
                        {
                            var nodeChildren = activeNodes[i].ChildNodes;
                            result.AddRange(nodeChildren.Select(node => new NodeResult
                            {
                                Name  = activeNodes[i].Name + " " + node.Name, IsConnected = node.IsConnected,Port = node.Port, ConnectionType = node.ConnectionType.ToString(), IPAddress = node.HostName, OnIPNX = false
                            }));
                        }
                        NodeResult add = new NodeResult
                        {
                            Name = activeNodes[i].Name,
                            IsConnected = activeNodes[i].IsConnected,
                            Port = activeNodes[i].Port,
                            ConnectionType = activeNodes[i].ConnectionType.ToString(),
                            IPAddress = activeNodes[i].HostName,
                            OnIPNX = false
                        };
                        result.Add(add);
                    }
                    return result;
                }
                catch (TimeoutException ex)
                {
                    return GetNodesConnectedToCentral();
                }
                catch (FaultException ex)
                {
                    Trace.WriteLine("an Error While trying to get GoGrid Nodes Occured at " + DateTime.Now + " due to at " + ex.Message + "with an inner exception :" +
                                   ex.InnerException+"\n");
                    return null;
                }
                catch (CommunicationException ex)
                {
                    Trace.WriteLine("an Error While trying to get GoGrid Nodes Occured at " + DateTime.Now + "due to " + ex.Message + "with an inner exception :" +
                                   ex.InnerException + "\n");
                    return null;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("an Error While trying to get GoGrid Nodes Occured at " + DateTime.Now + "due to " + ex.Message + "with an inner exception :" +
                                  ex.InnerException + "\n");
                    return GetNodesConnectedToCentral();
                }
            }
        }

        private static List<NodeResult> GetNodesConnectedToIPNX()
        {
            using (var info = new IPNXinfoserviceRef.InfoServiceClient())
            {
                var result = new List<NodeResult>();
                IPNXinfoserviceRef.Node[] activeNodes = null;
                try
                {
                    activeNodes = info.GetAllNodes();
                    activeNodes = activeNodes.Where(x => x.IsActive).ToArray();
                    for (var i = 0; i <= activeNodes.Length - 1; i++)
                    {
                        if (activeNodes[i].ChildNodes != null)
                        {
                            var nodeChildren = activeNodes[i].ChildNodes;
                            result.AddRange(nodeChildren.Select(node => new NodeResult
                            {
                                Name = activeNodes[i].Name + " "+node.Name,
                                IsConnected = node.IsConnected,
                                Port = node.Port,
                                ConnectionType = node.ConnectionType.ToString(),
                                IPAddress = node.HostName,
                                OnIPNX = false
                            }));
                        }
                        var add = new NodeResult
                        {
                            Name = activeNodes[i].Name,
                            IsConnected = activeNodes[i].IsConnected,
                            Port = activeNodes[i].Port,
                            ConnectionType = activeNodes[i].ConnectionType.ToString(),
                            IPAddress = activeNodes[i].HostName,
                            OnIPNX = true
                        };
                        result.Add(add);
                    }
                    return result;
                }
                catch (TimeoutException ex)
                {
                    return GetNodesConnectedToIPNX();
                }
                catch (FaultException ex)
                {
                    Trace.WriteLine("an Error While trying to get GoGrid Nodes Occured at " + DateTime.Now + " due to at " + ex.Message + "with an inner exception :" +
                                  ex.InnerException + "\n");
                    return GetNodesConnectedToIPNX();
                }
                catch (CommunicationException ex)
                {
                    Trace.WriteLine("an Error While trying to get GoGrid Nodes Occured at " + DateTime.Now + " due to at " + ex.Message + "with an inner exception :" +
                                  ex.InnerException + "\n");
                    return null;
                }

                catch (Exception ex)
                {
                    Trace.WriteLine("an Error While trying to get GoGrid Nodes Occured at " + DateTime.Now + " due to at " + ex.Message + "with an inner exception :" +
                                  ex.InnerException + "\n");
                    //Console.Write(ex.Message + " on IPNX");
                    //Console.ReadKey();
                    return GetNodesConnectedToIPNX();
                }
            }
        }

        public static Tuple<List<Node>, List<Node>, string> getAllNodes()
        {
            var context = new ConfigContext();
            var nodes = new List<Node>();
            var notUsedNodesList = new List<Node>();
            var fromCentral = GetNodesConnectedToCentral();
            var fromIPNX = GetNodesConnectedToIPNX();
            if (fromCentral != null)
            {
                foreach (var nr in fromCentral)
                {
                    AddOrUpdate(nr);
                }
            }
            if (fromIPNX != null)
            {
                foreach (var nr in fromIPNX)
                {
                    AddOrUpdate(nr);
                }
            }

            var allNodes = context.Nodes.ToList();
            foreach (var n in allNodes)
            {
                try
                {
                    if (GetNodeNotInUse() != null && GetNodeNotInUse().Contains(n.Name))
                        notUsedNodesList.Add(n);
                    else nodes.Add(n);
                }
                catch
                {
                    nodes.Add(n);
                }
            }
            if (fromCentral == null && fromIPNX != null)
            {
                return new Tuple<List<Node>, List<Node>, string>(nodes.Where(x => x.OnIPNX).ToList(), notUsedNodesList,
                    "Connection To Central is Down");
            }
            if (fromCentral != null && fromIPNX == null)
            {
                return new Tuple<List<Node>, List<Node>, string>(nodes.Where(x => !x.OnIPNX).ToList(), notUsedNodesList,
                    "Connection to IPNX is Down");
            }
            if (fromCentral == null && fromIPNX == null)
            {
                return new Tuple<List<Node>, List<Node>, string>(null, notUsedNodesList,
                    "Connection to IPNX and Central is Down");
            }
            return new Tuple<List<Node>, List<Node>, string>(nodes, notUsedNodesList, null);
        }

        private static void AddOrUpdate(NodeResult nr)
        {
            using (var dbNodes = new ConfigContext())
            {
                var db = dbNodes.Nodes;
                var n = new Node();
                n.ConnectionType = nr.ConnectionType;
                n.IPAddress = nr.IPAddress;
                n.IsConnected = nr.IsConnected;
                n.Name = nr.Name;
                n.OnIPNX = nr.OnIPNX;
                n.Port = nr.Port;


                var exsit = dbNodes.Nodes.Any(x => x.Name == n.Name && x.OnIPNX == n.OnIPNX);
                if (!exsit)
                {
                    n.TimeRecorded = DateTime.Now;
                    dbNodes.Nodes.Add(n);
                    dbNodes.SaveChanges();
                }
                if (exsit)
                {
                    var node = dbNodes.Nodes.First(x => x.Name == n.Name);
                    if (node.IsConnected != n.IsConnected)
                    {
                        node.IsConnected = n.IsConnected;
                        node.TimeRecorded = DateTime.Now;
                        dbNodes.SaveChanges();
                    }
                }
                // nodeResult.Add(n);
                UpdateNodeResult(n, !n.IsConnected);
            }
        }

        private static void UpdateNodeResult(Node n, bool isDown)
        {
            using (var context = new ConfigContext())
            {
                List<NodeResult> resultList = null;
                var nameExists = context.NodeUpdates.Any(x => x.Name == n.Name);
                if (nameExists)
                {
                    resultList =
                        context.NodeUpdates.Where(x => x.Name.ToUpper() == n.Name.ToUpper())
                            .OrderByDescending(x => x.TimeRecorded)
                            .ToList();
                }

                if (isDown)
                {
                    if (!nameExists)
                    {
                        var downResult = new NodeResult();
                        downResult.Name = n.Name;
                        downResult.IsConnected = !isDown;
                        downResult.TimeRecorded = DateTime.Now;
                        context.NodeUpdates.Add(downResult);
                        context.SaveChanges();
                        context.Dispose();
                    }
                    else
                    {
                        var wasDown = resultList.First().IsConnected;
                        if (wasDown)
                        {
                            var existingDown = resultList.Last();
                            // existingDown = compList.First(x => x.IsDown);
                            existingDown.IsConnected = !isDown;
                            existingDown.TimeRecorded = DateTime.Now;
                            context.Entry(existingDown).State = EntityState.Modified;
                            context.SaveChanges();
                            context.Dispose();
                        }
                    }
                }
                else if (resultList != null)
                {
                    if (!resultList.First().IsConnected)
                    {
                        var wasUp = resultList.Any(x => x.IsConnected);

                        if (wasUp)
                        {
                            var alreadyUp = resultList.Last(x => x.IsConnected);
                            alreadyUp.TimeRecorded = DateTime.Now;
                            alreadyUp.Name = n.Name;
                            context.Entry(alreadyUp).State = EntityState.Modified;
                            context.SaveChanges();
                            context.Dispose();
                            return;
                        }
                        //  alreadyUp.IsDown = isDown;
                        if (!wasUp)
                        {
                            var downResult = new NodeResult();
                            downResult.IsConnected = !isDown;
                            downResult.TimeRecorded = DateTime.Now;
                            downResult.Name = n.Name;
                            context.Entry(downResult).State = EntityState.Added;
                            context.SaveChanges();
                            context.Dispose();
                        }
                    }
                }
            }
        }

        public static object GetRecentUpdates()
        {
            var context = new ConfigContext();

            var downResult = context.NodeUpdates.OrderByDescending(x => x.TimeRecorded);
            var result = downResult.Take(10);
            // var downList = downResult.OrderBy(i => i.ArrivalTime).Take(5);
            return result;
        }

        //        if (response.StatusCode == HttpStatusCode.OK)

        //        var response = (HttpWebResponse)myRequest.GetResponseAsync();
        //        var myRequest = (HttpWebRequest)WebRequest.Create(url);
        //    {
        //    try
        //{

        //public static bool IsConnectionDown()
        //        {
        //            Trace.TraceInformation(string.Format("{0} Available", url));
        //        }
        //        else
        //        {
        //            Trace.TraceInformation(string.Format("{0} Returned, but with status: {1}", url, response.StatusDescription));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceInformation(string.Format("{0} unavailable: {1}", url, ex.Message));
        //    }

        //}
    }
}