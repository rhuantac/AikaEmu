using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using AikaEmu.GameServer.Managers.Configuration;
using AikaEmu.GameServer.Models;
using AikaEmu.GameServer.Models.Units;
using AikaEmu.GameServer.Models.Units.Character;
using AikaEmu.GameServer.Models.Units.Mob;
using AikaEmu.GameServer.Models.Units.Npc;
using AikaEmu.GameServer.Models.Units.Pran;
using AikaEmu.GameServer.Network.Packets.Game;
using AikaEmu.Shared.Utils;
using NLog;

namespace AikaEmu.GameServer.Managers
{
    public class WorldManager : Singleton<WorldManager>
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<uint, Character> _characters;
        private readonly ConcurrentDictionary<uint, Npc> _npcs;
        private readonly ConcurrentDictionary<uint, Mob> _mobs;
        private readonly ConcurrentDictionary<uint, Pran> _prans;

        protected WorldManager()
        {
            _characters = new ConcurrentDictionary<uint, Character>();
            _npcs = new ConcurrentDictionary<uint, Npc>();
            _mobs = new ConcurrentDictionary<uint, Mob>();
            _prans = new ConcurrentDictionary<uint, Pran>();
        }

        public Npc GetNpc(uint conId)
        {
            return _npcs.ContainsKey(conId) ? _npcs[conId] : null;
        }

        public Npc GetNpc(ushort npcId)
        {
            foreach (var (_, value) in _npcs)
            {
                if (value.NpcId == npcId) return value;
            }

            return null;
        }

        public IEnumerable<Character> GetOnlineCharacters()
        {
            return _characters.Values.ToList();
        }

        public void InitBasicSpawn()
        {
            foreach (var npc in DataManager.Instance.NpcData.GetAllNpc())
                npc.Spawn();

            foreach (var mob in DataManager.Instance.MobData.GetAllMob())
                mob.Spawn();
        }

        public void Spawn(BaseUnit unit)
        {
            if (unit == null) return;

            switch (unit)
            {
                case Character character:
                    _characters.TryAdd(character.Id, character);
                    break;
                case Npc npc:
                    _npcs.TryAdd(npc.Id, npc);
                    break;
                case Mob mob:
                    _mobs.TryAdd(mob.Id, mob);
                    break;
                case Pran pran:
                    _prans.TryAdd(pran.Id, pran);
                    break;
            }

            // TODO - SHOW
        }

        public void Despawn(BaseUnit unit)
        {
            if (unit == null) return;

            switch (unit)
            {
                case Character character:
                    _characters.TryRemove(character.Id, out _);
                    break;
                case Npc npc:
                    _npcs.TryRemove(npc.Id, out _);
                    break;
                case Mob mob:
                    _mobs.TryRemove(mob.Id, out _);
                    break;
                case Pran pran:
                    _prans.TryRemove(pran.Id, out _);
                    break;
            }

            // TODO - HIDE
        }

        public void ShowVisibleUnits(BaseUnit unit)
        {
            if (!(unit is Character character)) return;

            if (character.VisibleUnits.Count <= 0)
            {
                character.VisibleUnits = GetUnitsAround(character.Position, unit.Id);

                foreach (var (_, tempUnit) in character.VisibleUnits)
                {
                    switch (tempUnit)
                    {
                        case Mob mob:
                            character.Connection.SendPacket(new SendMobSpawn(mob));
                            break;
                        case Npc npc:
                            character.Connection.SendPacket(new SendUnitSpawn(tempUnit, 0, character));
                            break;
                        default:
                            character.Connection.SendPacket(new SendUnitSpawn(tempUnit));
                            break;
                    }
                }
            }
            else
            {
                var newUnits = GetUnitsAround(character.Position, unit.Id);

                var oldIds = new List<uint>(character.VisibleUnits.Keys);
                var newIds = new List<uint>(newUnits.Keys);

                var deleteUnits = oldIds.Except(newIds);
                var spawnUnits = newIds.Except(oldIds);

                foreach (var unitDel in deleteUnits)
                    character.Connection.SendPacket(new DespawnUnit(unitDel));

                foreach (var unitSpawn in spawnUnits)
                {
                    var tempUnit = newUnits[unitSpawn];
                    switch (tempUnit)
                    {
                        case Mob mob:
                            character.Connection.SendPacket(new SendMobSpawn(mob));
                            break;
                        case Npc npc:
                            character.Connection.SendPacket(new SendUnitSpawn(tempUnit, 0, character));
                            break;
                        default:
                            character.Connection.SendPacket(new SendUnitSpawn(tempUnit));
                            break;
                    }
                }

                character.VisibleUnits = newUnits;
            }
        }

        public Dictionary<uint, BaseUnit> GetUnitsAround(Position pos, uint myId)
        {
            var list = new Dictionary<uint, BaseUnit>();

            // TODO - Make distance 100 configurable
            foreach (var character in _characters.Values)
            {
                if (character.IsAround(pos, 70) && character.Id != myId) list.Add(character.Id, character);
            }

            foreach (var npc in _npcs.Values)
            {
                if (npc.IsAround(pos, 50) && npc.Id != myId) list.Add(npc.Id, npc);
            }

            foreach (var mob in _mobs.Values)
            {
                if (mob.IsAround(pos, 25) && mob.Id != myId) list.Add(mob.Id, mob);
            }

            foreach (var pran in _prans.Values)
            {
                if (pran.IsAround(pos, 25) && pran.Id != myId) list.Add(pran.Id, pran);
            }

            return list;
        }
    }
}