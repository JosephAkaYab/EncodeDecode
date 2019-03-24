using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace EncodeDecode
{
    public partial class MainWindow : Window
    {
        private const string charList = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ "; //List of chars the user can enter into the input, they can enter others but they will be ignored
        private const string keychars = "abcdefghijklmnopqrstuvwxyz"; //List of chars that the key can use
        bool encode = true; //What mode the program is on
        string key; //
        string output; // 
        List<string> substrings; //The input split into 4 char chunks
        List<long> subints; //The previous but converted from base 63
        List<string> substringsoutput; //The output split into chunks of 4
        StringBuilder sb = new StringBuilder();
        RNGCryptoServiceProvider securerandom = new RNGCryptoServiceProvider();

        public MainWindow()
        {
            InitializeComponent();
            GenerateKey(); //Generates a ket on startup
        }

        private void Encode()
        {
            if (txtInput.Text != null) //Only if theres text to encode
            {
                IntifyInputEncode(); //Splits the string into 4 char chunks and generates the int list
                ScrambleInts(); //Does math on the ints 
                ReadyOutput(); //Converts the ints to a string list
                AddstringSplits(); //Adds & at the end of every "chunk" so the decode knows where to split
                OutputInts(); //Assembles and prints output string list
            }
        }

        private void Decode()
        {
            if (txtInput.Text != null)
            {
                IntifyInputDecode(); //Same as encode but splits at every & not every 4 chars
                UnScrambleInts(); //Reverses math
                ReadyOutput();
                OutputInts();
            }
        }

        private void IntifyInputEncode()
        {
            substrings = new List<string>(Regex.Split(txtInput.Text, @"(?<=\G.{4})", RegexOptions.Singleline)); //Splits input every 4 chars, this is done so that the ints dont become too big and overflow
            subints = new List<long>(new long[substrings.Count]); //Makes a int list the same length 

            for (int i = 0; i < substrings.Count; i++)
            {
                subints[i] = FromCustomBase(substrings[i], charList); //Populates the int list by converting from base 63 to base 10
            }
        }

        private void IntifyInputDecode()
        {
            substrings = new List<string>(txtInput.Text.Split('&')); //Splits the text every &
            subints = new List<long>(new long[substrings.Count]);

            for (int i = 0; i < substrings.Count; i++)
            {
                substrings[i].Replace("&", ""); //Deletes the &s as they arent needed
            }

            for (int i = 0; i < substrings.Count; i++)
            {
                subints[i] = FromCustomBase(substrings[i], charList); 
            }
        }

        private void ScrambleInts() //Does randomish reversable maths based on the key
        {
            for (int i = 0; i < subints.Count; i++)
            {
                if (i % 2 == 0)
                {
                    subints[i] *= FromCustomBase(key, keychars) + 8870781348;
                }

                else if (i % 3 == 0)
                {
                    subints[i] *= FromCustomBase(key, keychars) + 3057374526;
                }

                else
                {
                    subints[i] *= FromCustomBase(key, keychars) + 4004675815;
                }
            }
        }

        private void UnScrambleInts() //Reverses the maths
        {
            for (int i = 0; i < subints.Count; i++)
            {
                if (i % 2 == 0)
                {
                    subints[i] /= FromCustomBase(key, keychars) + 8870781348;
                }

                else if (i % 3 == 0)
                {
                    subints[i] /= FromCustomBase(key, keychars) + 3057374526;
                }

                else
                {
                    subints[i] /= FromCustomBase(key, keychars) + 4004675815;
                }
            }
        }

        private void ReadyOutput()
        {
            substringsoutput = new List<string>(new string[substrings.Count]);

            for (int i = 0; i < substrings.Count; i++)
            {
                substringsoutput[i] = ToCustomBase(subints[i], charList);
            }
        }

        private void AddstringSplits() //Adds a & at the end of every chunk, this is beacuse they change size when going through the encyptor
        {
            for (int i = 0; i < substrings.Count; i++)
            {
                substringsoutput[i] = substringsoutput[i] + "&";
            }
        }

        private void OutputInts() //Assembles the output strings list and prints it
        {
            output = "";

            for (int i = 0; i < substringsoutput.Count; i++)
            {
                output = output + substringsoutput[i];
            }

            txtOutput.Text = output;
        }

        private void GenerateKey() //Generates a random key in base 26 
        {
            char[] c = new char[6];
            byte[] randomNumber = new byte[1];

            for (int i = 0; i < c.Length; i++)
            {
                securerandom.GetBytes(randomNumber);
                c[i] = keychars[randomNumber[0] % 26];
            }

            key = new String(c);
            txtKey.Text = key;
        }

        private static String ToCustomBase(long input, string chars) //Converts from base 10 to any custom base, I used base 63(numbers, uppercase, lowercase and spaces) 
        {                                                                //for the text and base 26 (lowercase) for the key
            if (input < 0) return "";

            char[] clistarr = chars.ToCharArray();
            var result = new Stack<char>();
            while (input != 0)
            {
                result.Push(clistarr[input % chars.Length]);
                input /= chars.Length;
            }
            return new string(result.ToArray());
        }

        private static Int64 FromCustomBase(string input, string chars) //Converts from a custom base to base 10
        {
            var reversed = input.Reverse();
            long result = 0;
            int pos = 0;
            foreach (char c in reversed)
            {
                result += chars.IndexOf(c) * (long)Math.Pow(chars.Length, pos);
                pos++;
            }
            return result;
        }

        private void EncodeDecode() //Either encodes or decodes depending on the mode, called when anything changes
        {
            switch (encode)
            {
                case true:
                    Encode();
                    break;

                case false:
                    Decode();
                    break;
            }
        }

        private void BtnSetting_Click(object sender, RoutedEventArgs e) //Toggles encode/decode
        {
            encode = !encode;

            if (encode)
            {
                btnSetting.Content = "Encode";
            }

            if (!encode)
            {
                btnSetting.Content = "Decode";
            }

            EncodeDecode();
        }

        private void BtnKey_Click(object sender, RoutedEventArgs e) //Genetates a new key
        {
            GenerateKey();
            EncodeDecode();
        }

        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e) //When user inputs stuff
        {
            EncodeDecode();
        }

        private void TxtKey_TextChanged(object sender, TextChangedEventArgs e) //User can type in their own key
        {
            key = txtKey.Text.ToString();
            EncodeDecode();
        }
    }
}
