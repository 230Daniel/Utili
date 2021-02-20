//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using Discord;
//using Discord.WebSocket;

//namespace Utili
//{
//    public class MessageCache : IMessageCache
//    {
//        private readonly ConcurrentDictionary<ulong, object> _messages;
//        private readonly ConcurrentQueue<ulong> _orderedMessages;
//        private readonly int _size;

//        public IReadOnlyCollection<SocketMessage> Messages => _messages.Select(x => x.Value as SocketMessage).ToList();

//        public IMessageCache CreateMessageCache(int size) => new MessageCache(size);

//        public MessageCache(int size)
//        {
//            int numProcs = Environment.ProcessorCount;
//            int concurrencyLevel = numProcs * 2;

//            _size = size;
//            _messages = new ConcurrentDictionary<ulong, object>(concurrencyLevel, (int)(_size * 1.05));
//            _orderedMessages = new ConcurrentQueue<ulong>();
//        }

//        public MessageCache() { }

//        public void Add(SocketMessage message) { }

//        public void AddManual(IMessage message)
//        {
//            if (_messages.TryAdd(message.Id, message))
//            {
//                _orderedMessages.Enqueue(message.Id);

//                while (_orderedMessages.Count > _size && _orderedMessages.TryDequeue(out ulong msgId))
//                    _messages.TryRemove(msgId, out _);
//            }
//        }

//        public SocketMessage Remove(ulong id)
//        {
//            if (_messages.TryRemove(id, out object result) && result is SocketMessage message)
//                return message;
//            return null;
//        }

//        public SocketMessage Get(ulong id)
//        {
//            if (_messages.TryGetValue(id, out object result) && result is SocketMessage message)
//                return message;
//            return null;
//        }

//        public IMessage GetManual(ulong id)
//        {
//            if (_messages.TryGetValue(id, out object result) && result is SocketMessage message)
//                return message;
//            if (result is IMessage imessage)
//                return imessage;
//            return null;
//        }

//        /// <exception cref="ArgumentOutOfRangeException"><paramref name="limit"/> is less than 0.</exception>
//        public IReadOnlyCollection<SocketMessage> GetMany(ulong? fromMessageId, Direction dir,
//            int limit = DiscordConfig.MaxMessagesPerBatch)
//        {
//            if (limit < 0) throw new ArgumentOutOfRangeException(nameof(limit));
//            if (limit == 0) return ImmutableArray<SocketMessage>.Empty;

//            IEnumerable<ulong> cachedMessageIds;
//            if (fromMessageId == null)
//                cachedMessageIds = _orderedMessages;
//            else if (dir == Direction.Before)
//                cachedMessageIds = _orderedMessages.Where(x => x < fromMessageId.Value);
//            else if (dir == Direction.After)
//                cachedMessageIds = _orderedMessages.Where(x => x > fromMessageId.Value);
//            else //Direction.Around
//            {

//                if (!_messages.TryGetValue(fromMessageId.Value, out object result) || result is not SocketMessage)
//                    return ImmutableArray<SocketMessage>.Empty;
//                SocketMessage msg = (SocketMessage)result;
//                int around = limit / 2;
//                IReadOnlyCollection<SocketMessage> before = GetMany(fromMessageId, Direction.Before, around);
//                IEnumerable<SocketMessage> after = GetMany(fromMessageId, Direction.After, around).Reverse();

//                return after.Concat(new SocketMessage[] { msg }).Concat(before).ToImmutableArray();
//            }

//            if (dir == Direction.Before)
//                cachedMessageIds = cachedMessageIds.Reverse();
//            if (dir == Direction.Around
//            ) //Only happens if fromMessageId is null, should only get "around" and itself (+1)
//                limit = limit / 2 + 1;

//            return cachedMessageIds
//                .Select(x =>
//                {
//                    if (_messages.TryGetValue(x, out object result) && result is SocketMessage msg)
//                        return msg;
//                    return null;
//                })
//                .Where(x => x != null)
//                .Take(limit)
//                .ToImmutableArray();
//        }
//    }
//}
