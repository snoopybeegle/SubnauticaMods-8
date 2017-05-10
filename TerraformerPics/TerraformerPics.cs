using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UM4SN;
using UnityEngine;

namespace TerraformerPics
{
    public class TerraformerPics : UnityPlugin
    {
        public override string Title
        {
            get
            {
                return "Terraformer Pixelart";
            }
        }

        public override string Desc
        {
            get
            {
                return "Make pixel art with text files!";
            }
        }

        public override void OnGameStart()
        {
            GameObject terraFormerPic = new GameObject();
            CreateTerraformerPic CTP = terraFormerPic.AddComponent<CreateTerraformerPic>();
        }
    }

    public class CreateTerraformerPic : MonoBehaviour
    {
        //public string image = "8888888888888888888888888888888888888888888888888888888888888888~8AAAAAAAAAAAAAA8888888888888888888888888888888888888888888888888~AAAAAAAAAAAAA884444444444444444444444444444448888888888888888888~AAAAAAAAAA884444447777777777777777777777744444488888888888888888~EEEEEEEEEE884444777777777755777755777777777444488888888888888888~EEEEEEEEEE884477775577777777777777444455777774488888844448888888~EEEEEEEEEE884477777777777777777784999944777774488884499994888888~9999999999884477777777775577777784999999477774488849999994888888~9999999999884477777777777777777784999999944444444499999994888888~9999999999884477777777777777777784999999999999999999999994888888~5555555555884477777777777755778499999966449999999966449999948888~5555558888884477557777777777778499999944449999999944449999948888~5588888888884477777777777777778499999999999994499999999999948888~8888888888884444777777557777778499559994499994499994499995548888~8888888888884444447777777777777849999994444444444444499994488888~8888888888888844444444444444444444499999999999999999999448888888~8888888888999988888888888888888888844444444444444444488888888888~8888888888999988888899888888888888888899888888999988888888888888~8888888888888888888888888888888888888888888888888888888888888888~";
        public Dictionary<string, int> charInt = new Dictionary<string, int>()
        {
            { "1", 1 }, //smooth sand
            { "2", 2 }, //warped sand
            { "3", 3 }, //really warped sand
            { "4", 4 }, //black rock
            { "5", 5 }, //purple rock with orange spots
            { "6", 6 }, //pink orangish coral
            { "7", 7 }, //purple rock white spots and orange
            { "8", 8 }, //really smooth sand (stone?)
            { "9", 9 }, //yellow with green inside spots
            { "A", 10 },//orange rock with grey rocks
            { "B", 11 },//brown rock with orange spots
            { "C", 12 },//gold and blue spots
            { "D", 13 },//red yellow purple spots
            { "E", 14 },//default terraformer
            { "F", 15 } //brown rock
        };
        public void Awake()
        {
            DevConsole.RegisterConsoleCommand(this, "createpixel");
            DevConsole.RegisterConsoleCommand(this, "createpixeltest");
        }
        private void OnConsoleCommand_createpixel(NotificationCenter.Notification n)
        {
            string pixelArtText = File.ReadAllText(@".\SNUnityMod\PixelArt\" + (string)n.data[0] + ".txt");
            CreateImage(pixelArtText);
        }
        private void OnConsoleCommand_createpixeltest(NotificationCenter.Notification n)
        {
            CreateImage("123456789ABCDEF");
        }
        public void CreateImage(string imageData)
        {
            int x = -100;
            int y = 50;
            Player.main.SetPosition(new Vector3(x, 20, y));
            foreach (char character in imageData)
            {
                if (character == '~' || character == '\n')
                {
                    //Debug.Log("Making a new line");
                    x = -100;
                    y -= 3;
                } else
                {
                    if (x % 50 == 0)
                    {
                        //If you get teleported into the water and
                        //can't get up, try using a fire extinguisher
                        //and propelling yourself up like WALL-E
                        Player.main.SetPosition(new Vector3(x, 20, y));
                    }
                    try {
                        if (character != '!')
                        {
                            //Debug.Log("Building at " + new Vector3(x, 0, y).ToString() + "(" + charInt[character.ToString()] + ")");
                            LargeWorldStreamer.main.PerformBoxEdit(new Bounds(new Vector3(x, 0, y), new Vector3(3, 2, 3)), Quaternion.identity, true, Convert.ToByte(charInt[character.ToString()]));
                        }
                    } catch (Exception ex)
                    {
                        Debug.Log("Failed to build " + new Vector3(x, 0, y).ToString());
                        Debug.LogError(ex);
                    }
                    x += 3;
                }
            }
        }
    }
}
