using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Microsoft.ML;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.ML.Data;

namespace breyn
{

    public class ItemPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Tag;
    }


    public class ItemMetaData
    {               
        public enum RepeatType { Never, EverySecond, EveryMinute, Hourly, Daily, Weekly, Monthly, Yearly };
        public DateTime Added;
        public DateTime Done;        
        public RepeatType Repeat;
    }

    public class Item
    {
       
        [LoadColumn(0)]
        public string ID { get; set; }
        
        [LoadColumn(1)]
        public string Tag;

        [LoadColumn(2)]
        public string Value;

        private ItemMetaData MetaData = null;

        public void index(string Value, string Tag = null)
        {
            this.Value = Value;
            this.Tag = Tag;
            this.MetaData = new ItemMetaData() { Added = DateTime.Now, Done = DateTime.MinValue };
        }

        public bool IsDone()
        {
            return this.MetaData.Done > DateTime.MinValue;
        }

    }

    public class IndexItems : List<Item>
    {

        public void AddDemoItems()
        {
            this.Add(new Item() { Value = "Ueberstunden auszahlen lassen", Tag ="Arbeit" } );
            this.Add(new Item() { Value = "Urlaub planen Freitag/Montag", Tag = "Arbeit" } );
            this.Add(new Item() { Value = "Patchday", Tag = "Arbeit, Windows, Server" } );
            this.Add(new Item() { Value = "Tomaten, Brot kaufen", Tag = "Privat, Einkaufen" } );
            this.Add(new Item() { Value = "Mail wg. Serverabschaltung", Tag = "" } );
            this.Add(new Item() { Value = "Jenny anrufen", Tag = "" } );
            this.Add(new Item() { Value = "Morgen TK Anlage neustarten", Tag = "" } );
        }

    }

    public class Index
    {

        const string IndexFileName = "index.json";
        public IndexItems Items { get; set; }

        public Index()
        {
            this.Items = new IndexItems();
        }

        public void ClearIndex()
        {
            this.Items.Clear();
        }

        public void Load()
        {
            if (System.IO.File.Exists(IndexFileName))
            {
                this.LoadIndexFrom(IndexFileName);
            }
        }

        public void LoadIndexFrom(string filename)
        {            
            try
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(filename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    this.Items = (IndexItems)serializer.Deserialize(file, typeof(IndexItems));
                }           
            }
            catch (System.Exception)
            {
                
            }
        }

        public void Save()
        {
            this.SaveIndexAs(IndexFileName);
        }

        public void SaveIndexAs(string filename)
        {
            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, this.Items);
            }
        }

    }
}