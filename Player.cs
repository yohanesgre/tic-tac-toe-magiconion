using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

[MessagePackObject]
public class Player
{
    [Key(0)]
    public int IdPlayer { get; set; }
    [Key(1)]
    public int IdFill { get; set; }
    [Key(2)]
    public int ButtonId { get; set; }
}
