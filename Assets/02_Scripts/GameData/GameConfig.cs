using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game
{
    public static class GameConfig
    {
        public static readonly int ItemSelectedLayerOrder = 10;

        public static int ItemLayerMask = LayerMask.GetMask("Item");
        public static int GroundLayerMask = LayerMask.GetMask("Ground");
        public static int UILayerMask = LayerMask.GetMask("UI");

        public static readonly string BoxIcon01 = "Images/Round/hide_box_img_1";

        public static Dictionary<float, GameType.Direction> AngleToDirectionMap = new Dictionary<float, GameType.Direction>() {
        {1, GameType.Direction.BOTTOM_RIGHT },
        {51, GameType.Direction.BOTTOM },
        {110, GameType.Direction.BOTTOM_LEFT },
        {153, GameType.Direction.LEFT },
        {190, GameType.Direction.TOP_LEFT },
        {220, GameType.Direction.TOP },
        {290, GameType.Direction.TOP_RIGHT },
        {357, GameType.Direction.RIGHT }
    };
    }

    public class GamePath
    {
        public static readonly string PopupPath = "Popup";

        public static string GetImagePath(string path)
        {
            return Path.Combine("Images", path);
        }

    }


    public enum ObjStatus
    {
        Idle,
        Walk,
        Attack,
    }


    public enum RenderingLayer
    {
        GROUND = 0,
        SHADOW = 1,
        SPRITE = 2
    }
}
