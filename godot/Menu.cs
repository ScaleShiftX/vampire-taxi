using Godot;
using System;

public partial class Menu : Control
{
    public override void _Ready()
    {
        //Set default mouse mode
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("menu_toggle"))
        {
            //Toggle menu
            SetMenuActive(!Visible);
        }

        if (Input.IsActionJustPressed("mouse_select"))
        {
            //Toggle menu
            SetMenuActive(true);
        }
    }

    private void SetMenuActive(bool isActive)
    {
        //Set visibility
        Visible = isActive;

        //Mouse mode
        if (isActive)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }
}