using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    

    public enum PotionTypes
    {
        Colourless,
        Yellow,
        Red,
        Blue,
        Purple,
        Green,
        Kong,
        Key
    }

    public class DK64_Item
    {
        public string name;
        public string itemType;
        public PotionTypes potionType;
        public int item_id;
        public string image;

        public override string ToString()
        {
            return $"{name}, {itemType}, {potionType}, {item_id}, {image}";
        }
    }

    public static class DK64_Items
    {
        public static Dictionary<int, int> BossRooms { get; } = new Dictionary<int, int>()
        {
            {203, 1},
            {204, 2},
            {205, 3},
            {206, 4},
            {207, 5},
            {8, 6},
            {197, 7},
            {154, 8},
            {111, 9},
            {83, 10},
            {196, 11},
            {199, 12},
        };
        public static Dictionary<int, DK64_Item> GenerateDK64Items()
        {
            // bigass list of every spoilerhintable object, their ID, their potion enum
            Dictionary<int, DK64_Item> theDict = new Dictionary<int, DK64_Item>();

            //fuck modularity
            string pathto = Application.StartupPath + "/Autotrackers/DK64_SpoilerItems.csv";
            string[] lines = System.IO.File.ReadAllLines(pathto);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');

                // ignore the header row lol
                if (parts[0] == "name") { continue; }
                DK64_Item temp = new DK64_Item()
                {
                    name = parts[0],
                    itemType = parts[1],
                    potionType = (PotionTypes)int.Parse(parts[2]),
                    item_id = int.Parse(parts[3]),
                    image = parts[4]
                };
                theDict.Add(temp.item_id, temp);
            }
            return theDict;
        }



        public static Dictionary<int, int> GenerateDK64Maps()
        {
            Dictionary<int, int> theDict = new Dictionary<int, int>();

            //fuck modularity
            string pathto = Application.StartupPath + "/Autotrackers/DK64_SpoilerRegions.csv";
            string[] lines = System.IO.File.ReadAllLines(pathto);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');

                // ignore the header row lol
                if (parts[0] == "id") { continue; }
                theDict.Add(Convert.ToInt32(parts[0], 16), int.Parse(parts[1]));
            }


            return theDict;
        }
    }

}
