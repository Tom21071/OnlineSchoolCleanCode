
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Presentation.Models.Common;
using System.Diagnostics;
using System.Reflection;

namespace OnlineSchool.Presentation.Hubs
{
    public class Room
    {
        public string RoomName { get; set; }
        public List<RoomUserModel> RoomUsers = new List<RoomUserModel>();
        public int SubjectId { get; set; }
    }

    public class StreamingHub : Hub
    {
        private readonly AppDbContext _context;

        public StreamingHub(AppDbContext context)
        {
            _context = context;
        }
        private static List<Room> rooms = new List<Hubs.Room>();


        public async Task SendMessage(string roomId, string sender, object message, string reciever)
        {
            await Clients.Groups(roomId).SendAsync("ReceiveMessage", message, _context.Users.FirstOrDefault(x => x.Email == sender),reciever);
        }
        public async Task OnConnectedAsync(int subjectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, subjectId.ToString());
            foreach (var room in rooms)
            {
                if (room.RoomUsers.Count > 0)
                {
                    await Clients.Caller.SendAsync("RecieveRoomOnLoad", room.RoomName.ToString());
                }
            }
        }
        public async Task JoinRoom(string userName, string streamingRoomId, int subjectId)
        {
            //add use if room exists
            if (rooms.Where(x => x.RoomName == streamingRoomId.ToString()).Any())
            {
                RoomUserModel roomUser = new RoomUserModel();
                roomUser.User = _context.Users.FirstOrDefault(x => x.Email == userName);
                roomUser.ConnectionId = Context.ConnectionId;

                rooms.FirstOrDefault(x => x.RoomName == streamingRoomId).RoomUsers.Add(roomUser);
                int id = rooms.FirstOrDefault(x => x.RoomName == streamingRoomId).SubjectId;
                await Clients.Caller.SendAsync("RecieveSubjectId", id);
            }
            else //add new room and user
            {
                RoomUserModel roomUser = new RoomUserModel();
                roomUser.User = _context.Users.FirstOrDefault(x => x.Email == userName);
                roomUser.ConnectionId = Context.ConnectionId;

                rooms.Add(new Room
                {
                    RoomName = streamingRoomId.ToString(),
                    RoomUsers = new List<RoomUserModel> { roomUser },
                    SubjectId = subjectId

                });
                await Groups.AddToGroupAsync(Context.ConnectionId, subjectId.ToString());
                await Clients.Groups(subjectId.ToString()).SendAsync("AddRoom", streamingRoomId);

            }

            await Groups.AddToGroupAsync(Context.ConnectionId, streamingRoomId.ToString());
        }

        public async Task<int> RoomUsersCount(string streamingRoomId)
        {
            return rooms.FirstOrDefault(x => x.RoomName == streamingRoomId).RoomUsers.Count();
        }

        public static List<Room> GetGroups()
        {
            if (rooms != null)
            {
                return rooms;
            }
            return new List<Room>();
        }

        public override async Task<Task> OnDisconnectedAsync(Exception? exception)
        {
            foreach (var item in rooms)
            {
                if (item.RoomUsers.Any(x => x.ConnectionId == Context.ConnectionId))
                {
                    item.RoomUsers = item.RoomUsers.Where(x => x.ConnectionId != Context.ConnectionId).ToList();
                    await Clients.Groups(item.RoomName).SendAsync("Disconnected", Context.User.Identity.Name);
                }
                if (item.RoomUsers.Count == 0)
                {
                    await Clients.Groups(item.SubjectId.ToString()).SendAsync("RemoveRoom", item.RoomName.ToString());
                    rooms.Remove(item);
                }
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
