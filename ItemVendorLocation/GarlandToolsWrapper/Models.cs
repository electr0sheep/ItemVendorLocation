using System.Collections.Generic;

namespace GarlandToolsWrapper
{
    public class Models
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
            // c can either be an array of floats, an int, or not present
            public dynamic c;
            public object t;
            public int g;
            public int r;
            public int z;
            public int p;
            public dynamic f;

            public bool CIsValid()
            {
                return c?.GetType()?.Name == "JArray";
            }
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

        public class TradeShop
        {
            public string shop;
            public List<ulong> npcs;
            public List<Listing> listings;
        }

        public class Listing
        {
            public List<Currency> currency;
        }

        public class Currency
        {
            public string id;
            public ulong amount;
        }

        public class Item
        {
            public string name;
            public string description;
            public int id;
            public float patch;
            public int patchCategory;
            public ulong price;
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
            public List<TradeShop> tradeShops;
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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
