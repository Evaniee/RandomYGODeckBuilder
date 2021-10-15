using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace RandomYGODeck
{
    class CardDB
    {
        private Random m_random;
        private JArray m_cards;

        private readonly string[] m_mainDeckTypes = new string[] { "Effect Monster", "Flip Effect Monster", "Flip Tuner Effect Monster", "Gemini Monster", "Normal Monster", "Normal Tuner Monster", "Pendulum Effect Monster", "Pendulum Flip Effect Monster", "Pendulum Normal Monster", "Pendulum Tuner Effect Monster", "Ritual Effect Monster", "Ritual Monster", "Skill Card", "Spell Card", "Spirit Monster", "Toon Monster", "Trap Card", "Tuner Monster", "Union Effect Monster", "Normal Spell", "Quick-Play Spell", "Continuous Spell", "Ritual Spell", "Equip Spell", "Field Spell", "Normal Trap", "Continuous Trap", "Counter Trap"};
        private readonly string[] m_extraDeckTypes = new string[] { "Fusion Monster", "Link Monster", "Pendulum Effect Fusion Monster", "Synchro Monster", "Synchro Pendulum Effect Monster", "Synchro Tuner Monster", "XYZ Monster", "XYZ Pendulum Effect Monster" };

        public CardDB()
        {
            m_random = new Random();
            m_cards = Startup();
            SaveDeck(MakeDeck());
            Console.WriteLine("Deck Created!");
            Console.ReadKey();
        }

        #region Startup
        private JArray Startup()
        {
            bool l_valid = false;
            string l_json = null;
            Console.Write("Do you have a local copy of the cardlist in JSON format (\"cardinfo.php.json\") (Y/N): ");
            while (!l_valid)
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.Y:
                        Console.WriteLine("Y");
                        l_valid = true;
                        l_json = ReadJSON("cardinfo.php.json");
                        break;
                    case ConsoleKey.N:
                        Console.WriteLine("N");
                        l_valid = true;
                        l_json = PullJSON();
                        MakeLocal(l_json);
                        break;
                }
            return MakeArray(l_json);
        }

        private string ReadJSON(string a_filepath) 
        {
            try
            {
                return File.ReadAllText(a_filepath);
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured trying to load cards from local \"cardinfo.php.json\".\nCheck if present or escalate privileges.");
                Environment.Exit(-1);
                return null;
            }
        }

        private string PullJSON()
        {
            
            try
            {
                string l_string;
                using (WebClient l_client = new WebClient())
                {
                    l_string = l_client.DownloadString("https://db.ygoprodeck.com/api/v7/cardinfo.php");
                }
                return l_string;
            }
            catch(Exception)
            {
                Console.WriteLine("An error ocurred attempting to download the card JSON from ygoprodeck.\nTry manual download or escalate privileges.");
                Environment.Exit(-1);
                return null;
            }
        }

        private void MakeLocal(string a_json)
        {
            try
            {
                File.WriteAllText("cardinfo.php.json", a_json);
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured trying create a local backup at \"cardinfo.php.json\".\nTry manual download or escalate privileges.");
                Environment.Exit(-1);
            }
        }

        private JArray MakeArray(string a_json)
        {
            return (JArray)JObject.Parse(a_json)["data"];
        }
        #endregion

        #region Deck Creation
        private Tuple<List<string>, List<string>, List<string>> MakeDeck()
        {
            Tuple<List<string>, List<string>, List<string>> m_deck = new Tuple<List<string>, List<string>, List<string>>(new List<string>(), new List<string>(), new List<string>());

            while (m_deck.Item1.Count < 40 || m_deck.Item2.Count < 15 || m_deck.Item3.Count < 15)
            {
                JObject l_card = GetCard(m_cards);
                if (IsMain(l_card))
                {
                    if (m_deck.Item1.Count < 40)
                    {
                        m_deck.Item1.Add((string)l_card.SelectToken("id"));
                        Console.WriteLine("Main " + m_deck.Item1.Count + ": " + (string)l_card.SelectToken("name"));
                    }

                    else if (m_deck.Item3.Count < 15)
                    {
                        m_deck.Item3.Add((string)l_card.SelectToken("id"));
                        Console.WriteLine("Side " + m_deck.Item3.Count + ": " + (string)l_card.SelectToken("name"));
                    }
                }
                else if (IsExtra(l_card))
                {
                    if (m_deck.Item2.Count < 15)
                    {
                        m_deck.Item2.Add((string)l_card.SelectToken("id"));
                        Console.WriteLine("Extra " + m_deck.Item2.Count + ": " + (string)l_card.SelectToken("name"));
                    }
                    else if (m_deck.Item3.Count < 15)
                    {
                        m_deck.Item3.Add((string)l_card.SelectToken("id"));
                        Console.WriteLine("Side " + m_deck.Item3.Count + ": " + (string)l_card.SelectToken("name"));
                    }
                }
            }
            return m_deck;
        }

        private void SaveDeck(Tuple<List<string>, List<string>, List<string>> a_deck)
        {
            StringBuilder l_deckstring = new StringBuilder();
            l_deckstring.Append("#created by RNG\n#main\n");
            foreach (string i_card in a_deck.Item1)
                l_deckstring.Append(i_card + "\n");
            l_deckstring.Append("#extra\n");
            foreach (string i_card in a_deck.Item2)
                l_deckstring.Append(i_card + "\n");
            l_deckstring.Append("!side\n");
            foreach (string i_card in a_deck.Item3)
                l_deckstring.Append(i_card + "\n");
            l_deckstring.Remove(l_deckstring.Length - 1, 1);
            File.WriteAllText("RandomDeck.ydk",l_deckstring.ToString());
        }

        private JObject GetCard(JArray a_array)
        {
            return (JObject)a_array[m_random.Next(0, a_array.Count)];
        }

        private bool IsMain(JObject a_object)
        {
            foreach (string i_type in m_mainDeckTypes)
                if ((string)a_object.SelectToken("type") == i_type)
                    return true;
            return false;
        }

        public bool IsExtra(JObject a_object)
        {
            foreach (string i_type in m_extraDeckTypes)
                if ((string)a_object.SelectToken("type") == i_type)
                    return true;
            return false;
        }
        #endregion
    }



    class Program
    {
        static void Main(string[] args)
        {
            CardDB l_cardDB = new CardDB();
            
        }
    }
}
