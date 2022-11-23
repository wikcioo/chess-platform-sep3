namespace Domain.Models;

public class StockfishPlayer
{
   public uint Skill { get; private set; }
   public uint Depth { get; private set; }
   public uint Time { get; private set; }

   public StockfishPlayer(uint skill, uint depth, uint time)
   {
      Skill = skill;
      Depth = depth;
      Time = time;
   }
}