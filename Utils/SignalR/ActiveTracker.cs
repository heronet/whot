using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Utils.SignalR
{
    public class ActiveTracker
    {
        private static Dictionary<string, List<string>> ActiveUsers = new Dictionary<string, List<string>>();
        public Task UserConnected(string username, string connectionId)
        {
            lock (ActiveUsers)
            {

                if (ActiveUsers.ContainsKey(username))
                {
                    ActiveUsers[username].Add(connectionId);
                }
                else
                {
                    ActiveUsers.Add(username, new List<string> { connectionId });
                }

            }
            return Task.CompletedTask;
        }
        public Task UserDisconnected(string username, string connectionId)
        {
            lock (ActiveUsers)
            {

                if (!ActiveUsers.ContainsKey(username)) return Task.CompletedTask;

                ActiveUsers[username].Remove(connectionId);
                if (ActiveUsers[username].Count == 0)
                    ActiveUsers.Remove(username);

            }
            return Task.CompletedTask;
        }
        public Task<string[]> GetOnlineUsers()
        {
            string[] activeUsers;
            lock (ActiveUsers)
            {
                activeUsers = ActiveUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }
            return Task.FromResult(activeUsers);
        }
    }
}