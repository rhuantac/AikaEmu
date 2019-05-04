using AikaEmu.GameServer.Models;
using AikaEmu.GameServer.Models.Units.Character;
using AikaEmu.GameServer.Network.GameServer;
using AikaEmu.Shared.Network;

namespace AikaEmu.GameServer.Network.Packets.Game
{
    public class Unk303D : GamePacket
    {
        private readonly int _type;

        public Unk303D(Character character, int type)
        {
            _type = type;
            Opcode = (ushort) GameOpcode.Unk303D;
            SenderId = character.ConnectionId;
        }

        public override PacketStream Write(PacketStream stream)
        {
            switch (_type)
            {
                case 0:
                    stream.Write(new byte[]
                    {
                        0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x10, 0x00,
                        0x12, 0x00, 0x17, 0x00, 0x8D, 0x00, 0x8E, 0x00, 0x9E, 0x00, 0xC2, 0x00,
                        0x08, 0x01, 0x15, 0x01, 0x03, 0x00, 0x02, 0x09, 0x09, 0x02, 0x09, 0x02,
                        0x09, 0x0B, 0x0B, 0x0B
                    });
                    break;
                case 1:
                    stream.Write(new byte[]
                    {
                        0x01, 0x00, 0x00, 0x00, 0x4F, 0x01, 0x70, 0x01, 0x9D, 0x01, 0xF1, 0x03,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x02, 0x09, 0x0B, 0x02, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00
                    });
                    break;
            }

            return stream;
        }
    }
}