using SwitchMonitor_v1.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;

namespace SwitchMonitor_v1.Data
{
    public class NodeData
    {
        private static string[] GetNodeNotInUse()
        {
            try
            {
                string[] nodesNotInUse = ConfigurationManager.AppSettings["notInUse"].Split(',').Select(s => s.Trim()).ToArray();
                return nodesNotInUse;
            }
            catch { return null; }
            
        }
        
        private static List<NodeResult> GetNodesConnectedToCentral()
        {
            using (NodeInfoService.InfoServiceClient info = new NodeInfoService.InfoServiceClient())
            {
               List<NodeResult> result = new List<NodeResult>(100);
                NodeInfoService.Node[] activeNodes = null;
                try
                {
                    var uptimeReport = info.GetServiceUptimeReport(DateTime.Now);
                    activeNodes = info.GetAllNodes();
                    activeNodes = activeNodes.Where(x => x.IsActive).ToArray();
                    for (int i = 0; i <= activeNodes.Length - 1; i++)
                    {
                        NodeResult add = new NodeResult()
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
                    return null;
                }
                catch (CommunicationException ex)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    return GetNodesConnectedToCentral();
                }

            }
        }

        private static List<NodeResult> GetNodesConnectedToIPNX()
        {
            using (IPNXinfoserviceRef.InfoServiceClient info = new IPNXinfoserviceRef.InfoServiceClient())
            {
                
                List<NodeResult> result = new List<NodeResult>();
                IPNXinfoserviceRef.Node[] activeNodes = null;
                try
                {
                    activeNodes = info.GetAllNodes();
                    activeNodes = activeNodes.Where(x => x.IsActive).ToArray();
                    for (int i = 0; i <= activeNodes.Length - 1; i++)
                    {
                        NodeResult add = new NodeResult()
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
                    return GetNodesConnectedToIPNX();
                }
                catch (CommunicationException ex)
                {
                    return null;
                }
                
                catch (Exception ex)
                {
                    //Console.Write(ex.Message + " on IPNX");
                    //Console.ReadKey();
                    return GetNodesConnectedToIPNX();
                }
               
            }
        }

        public static Tuple<List<Node>, List<Node>, string> getAllNodes()
        {
            ConfigContext context = new ConfigContext();
            List<Node> nodes = new List<Node>();
            List<Node> notUsedNodesList = new List<Node>();
            List<NodeResult> fromCentral = GetNodesConnectedToCentral();
            List<NodeResult> fromIPNX = GetNodesConnectedToIPNX();
            if (fromCentral != null)
            {
                foreach (NodeResult nr in fromCentral)
                { AddOrUpdate(nr);}
            }
            if (fromIPNX != null)
            {
                foreach (NodeResult nr in fromIPNX)
                { AddOrUpdate(nr); }
            }
           
            List<Node> allNodes= context.Nodes.ToList();
            foreach (Node n in allNodes)
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
            if (fromCentral == null && fromIPNX!=null)
            {
                return new Tuple<List<Node>, List<Node>,string>(nodes.Where(x=>x.OnIPNX).ToList(), notUsedNodesList,"Connection To Central is Down");
            }
            if (fromCentral != null && fromIPNX == null)
            {
                return new Tuple<List<Node>, List<Node>, string>(nodes.Where(x => !x.OnIPNX).ToList(), notUsedNodesList, "Connection to IPNX is Down");
            }
            if (fromCentral == null && fromIPNX==null)
            {
                return new Tuple<List<Node>, List<Node>, string>(null, notUsedNodesList, "Connection to IPNX and Central is Down");
            }
            return new Tuple<List<Node>, List<Node>,string>(nodes,notUsedNodesList,null);

        }

       private static void AddOrUpdate(NodeResult nr)
        {
            using(ConfigContext dbNodes = new ConfigContext())
            {
            var db = dbNodes.Nodes;
            Node n = new Node();
            n.ConnectionType = nr.ConnectionType;
            n.IPAddress = nr.IPAddress;
            n.IsConnected = nr.IsConnected;
            n.Name = nr.Name;
            n.OnIPNX = nr.OnIPNX;
            n.Port= nr.Port;


            bool exsit = dbNodes.Nodes.Any(x => x.Name == n.Name && x.OnIPNX == n.OnIPNX);
            if(!exsit)
            {
                n.TimeRecorded = DateTime.Now;
                    dbNodes.Nodes.Add(n);
                    dbNodes.SaveChanges();
            }
            if(exsit)
            {
                var node = dbNodes.Nodes.Single(x => x.Name == n.Name);
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
            using (ConfigContext context = new ConfigContext())
            {
                List<NodeResult> resultList = null;
                bool nameExists = context.NodeUpdates.Any(x => x.Name == n.Name);
                if (nameExists)
                {
                    resultList = context.NodeUpdates.Where(x => x.Name.ToUpper() == n.Name.ToUpper() ).OrderByDescending(x => x.TimeRecorded).ToList();

                }

                if (isDown)
                {
                    if (!nameExists)
                    {
                        NodeResult downResult = new NodeResult();
                        downResult.Name = n.Name;
                        downResult.IsConnected = !isDown;
                        downResult.TimeRecorded = DateTime.Now;
                        context.NodeUpdates.Add(downResult);
                        context.SaveChanges();
                        context.Dispose();
                        return;
                    }
                    else
                    {
                        bool wasDown = resultList.First().IsConnected;
                        if (wasDown)
                        {
                            NodeResult existingDown = resultList.Last();
                            // existingDown = compList.First(x => x.IsDown);
                            existingDown.IsConnected = !isDown;
                            existingDown.TimeRecorded = DateTime.Now;
                            context.Entry(existingDown).State = EntityState.Modified;
                            context.SaveChanges();
                            context.Dispose();
                            return;
                        }
                    }
                    return;
                }
                else if (resultList != null)
                {
                    if (!resultList.First().IsConnected)
                    {
                        bool wasUp = resultList.Any(x => x.IsConnected);

                        if (wasUp)
                        {
                            NodeResult alreadyUp = resultList.Last(x => x.IsConnected);
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
                            NodeResult downResult = new NodeResult();
                            downResult.IsConnected = !isDown;
                            downResult.TimeRecorded = DateTime.Now;
                            downResult.Name = n.Name;
                            context.Entry(downResult).State = EntityState.Added;
                            context.SaveChanges();
                            context.Dispose();
                            return;
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

        //public static bool IsConnectionDown()
        //{
        //    try
        //    {
        //        var myRequest = (HttpWebRequest)WebRequest.Create(url);

        //        var response = (HttpWebResponse)myRequest.GetResponseAsync();

        //        if (response.StatusCode == HttpStatusCode.OK)
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
    


