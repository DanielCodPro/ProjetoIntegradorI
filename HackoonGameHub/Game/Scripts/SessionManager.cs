using System.Collections.Generic;
using Godot;
using Models;

namespace Game.Scripts;

public partial class SessionManager : Node
{
    public Usuario usuario { get; set; }
    public List<Usuario> usuariosNaRede { get; set; } = new List<Usuario>();
    public Sala sala { get; set; }

    public override void _Ready()
    {
        GD.Print("Session Manager Ready");
    }
}