using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GarlandToolsWrapper
{
    public class Models
    {

        public class Data
        {
            public Dictionary<string, LocationData> locationIndex;
        }
        public class LocationData
        {
            public int id;
            public string name;
            public int parentId;
            public float size;
        }

        public class F
        {
            public int id;
            public int job;
            public int lvl;
        }
        public class Obj
        {
            public ulong i;
            public string n;
            // l can either be an int, or a string
            public dynamic l;
            // c can either be an array of floats, or an int
            public dynamic c;
            public object t;
            public int g;
            public int r;
            public int z;
            public int p;
            public dynamic f;
        }
        public class ItemSearchResult
        {
            public string type;
            public string id;
            public Obj obj;
        }
        public class ItemDetails
        {
            public Item item;
            //public List<Ingredient> ingredients;
            public List<Partial> partials;
        }

        public class Partial
        {
            public string type;
            public string id;
            public Obj obj;
        }

        public class Item
        {
            public string name;
            public string description;
            public int id;
            public float patch;
            public int patchCategory;
            public int price;
            public int ilvl;
            public int category;
            public int tradeable;
            public int sell_price;
            public int rarity;
            public int stackSize;
            public AttrNq attr;
            public AttrHq attr_hq;
            public int icon;
            public List<ulong> vendors;
            public List<Craft> craft;
            //public object ingredient_of;
            public List<int> quests;
            public List<int> leves;
            public Supply supply;
        }
        public class Supply
        {
            public int count;
            public int xp;
            public int seals;
        }

        public class Craft
        {
            public int id;
            public int job;
            public int rlvl;
            public int durability;
            public int quality;
            public int progress;
            public int lvl;
            public int suggestedCraftsmanship;
            public int suggestedControl;
            public int materialQualityFactor;
            public int yield;
            public int hq;
            public int quickSynth;
            public List<CraftingIngredient> ingredients;
            public Complexity complexity;
        }
        public class Complexity
        {
            public int nq;
            public int hq;
        }
        public class CraftingIngredient
        {
            public int id;
            public int amount;
            public float quality;
        }
        public class Attr
        {
            public Action action;
        }
        public class AttrNq : Attr { }
        public class AttrHq : Attr { }

        public class Action
        {
            public Tenacity tenacity;
            public int vitality;
            public int directHitRate;
        }

        public class Tenacity
        {
            public int rate;
            public int limit;
        }
    }
}
