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
        private const string charList = "0123456789 abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"; //List of chars the user can enter into the input, they can enter others but they will be ignored
        private const string keychars = "abcdefghijklmnopqrstuvwxyz-"; //List of chars that the key can use
        private const string splitchars = "¬`¦£$%^&*()-_=+[]{};:'@#~,./<>?|";
        bool encode = true; //What mode the program is on
        string[] keys = new string[7];
        string output;
        List<string> substrings; //The input split into 4 char chunks
        List<long> subints; //The previous but converted from base 63
        List<string> substringsoutput; //The output split into chunks of 4
        StringBuilder sb = new StringBuilder();
        Random random = new Random();
        RNGCryptoServiceProvider securerandom = new RNGCryptoServiceProvider();

        public MainWindow()
        {
            InitializeComponent();

            NewKey();
        }

        private void Encode()
        {
            if (txtInput.Text != null && txtInput.Text != "") //Only if theres text to encode
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
            if (txtInput.Text != null && txtInput.Text != "")
            {
                IntifyInputDecode(); //Same as encode but splits at every & not every 4 chars
                UnScrambleInts(); //Reverses math
                ReadyOutput();
                OutputInts();
            }
        }

        private void IntifyInputEncode()
        {
            substrings = new List<string>(Regex.Split(txtInput.Text, @"(?<=\G.{1})", RegexOptions.Singleline)); //Splits input every char, this is done so that the ints dont become too big and overflow and some other problem with spaces
            subints = new List<long>(new long[substrings.Count]); //Makes a int list the same length 

            for (int i = 0; i < substrings.Count; i++)
            {
                Regex pattern = new Regex(splitchars + "|[\n]{2}");
                pattern.Replace(substrings[i], "");
            }

            for (int i = 0; i < substrings.Count; i++)
            {
                if (substrings[i] == "" || substrings[i] == null) continue;
                subints[i] = FromCustomBase(substrings[i], charList); //Populates the int list by converting from base 63 to base 10
            }
        }

        private void IntifyInputDecode()
        {
            substrings = new List<string>(txtInput.Text.Split(splitchars.ToCharArray())); //Splits the text every string splitter
            subints = new List<long>(new long[substrings.Count]);

            for (int i = 0; i < substrings.Count; i++)
            {
                Regex pattern = new Regex(splitchars + "|[\n]{2}");
                pattern.Replace(substrings[i], ""); //Deletes the splitters as they arent needed
            }

            for (int i = 0; i < substrings.Count; i++)
            {
                if (substrings[i] == "" || substrings[i] == null) continue;
                subints[i] = FromCustomBase(substrings[i], charList); 
            }
        }

        private void ScrambleInts() //Does randomish reversable maths based on the key
        {
            for (int i = 0; i < subints.Count; i++)
            {
                long num = FromCustomBase(keys[0], keychars) - FromCustomBase(keys[1], keychars) + FromCustomBase(keys[2], keychars) + FromCustomBase(keys[3], keychars) + FromCustomBase(keys[4], keychars) + FromCustomBase(keys[5], keychars) + FromCustomBase(keys[6], keychars);
                long num2 = FromCustomBase(keys[0], keychars) + FromCustomBase(keys[1], keychars) - FromCustomBase(keys[2], keychars) + FromCustomBase(keys[3], keychars) + FromCustomBase(keys[4], keychars) + FromCustomBase(keys[5], keychars) + FromCustomBase(keys[6], keychars);
                long num3 = FromCustomBase(keys[0], keychars) + FromCustomBase(keys[1], keychars) + FromCustomBase(keys[2], keychars) - FromCustomBase(keys[3], keychars) + FromCustomBase(keys[4], keychars) + FromCustomBase(keys[5], keychars) + FromCustomBase(keys[6], keychars);
                long num4 = FromCustomBase(keys[0], keychars) + FromCustomBase(keys[1], keychars) + FromCustomBase(keys[2], keychars) + FromCustomBase(keys[3], keychars) - FromCustomBase(keys[4], keychars) + FromCustomBase(keys[5], keychars) + FromCustomBase(keys[6], keychars);
                long num5 = FromCustomBase(keys[0], keychars) + FromCustomBase(keys[1], keychars) + FromCustomBase(keys[2], keychars) + FromCustomBase(keys[3], keychars) + FromCustomBase(keys[4], keychars) - FromCustomBase(keys[5], keychars) + FromCustomBase(keys[6], keychars);
                long num6 = FromCustomBase(keys[0], keychars) + FromCustomBase(keys[1], keychars) + FromCustomBase(keys[2], keychars) + FromCustomBase(keys[3], keychars) + FromCustomBase(keys[4], keychars) + FromCustomBase(keys[5], keychars) - FromCustomBase(keys[6], keychars);

                long num7 = num.ToString().Sum(c => Convert.ToInt32(c)) + num2.ToString().Sum(c => Convert.ToInt32(c)) + num3.ToString().Sum(c => Convert.ToInt32(c)) + num4.ToString().Sum(c => Convert.ToInt32(c)) + num5.ToString().Sum(c => Convert.ToInt32(c)) + num6.ToString().Sum(c => Convert.ToInt32(c));

                if (i % 2 == 0)
                {
                    subints[i] *= (FromCustomBase(keys[0], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else if (i % 3 == 0)
                {
                    subints[i] *= (FromCustomBase(keys[1], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else if (i % 5 == 0)
                {
                    subints[i] *= (FromCustomBase(keys[2], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else if (i % 7 == 0)
                {
                    subints[i] *= (FromCustomBase(keys[3], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else if (i % 11 == 0)
                {
                    subints[i] *= (FromCustomBase(keys[4], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else if (i % 13 == 0)
                {
                    subints[i] *= (FromCustomBase(keys[5], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else
                {
                    subints[i] *= (FromCustomBase(keys[6], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }
            }
        }

        private void UnScrambleInts() //Reverses the maths
        {
            for (int i = 0; i < subints.Count; i++)
            {
                long num = FromCustomBase(keys[0], keychars) - FromCustomBase(keys[1], keychars) + FromCustomBase(keys[2], keychars) + FromCustomBase(keys[3], keychars) + FromCustomBase(keys[4], keychars) + FromCustomBase(keys[5], keychars) + FromCustomBase(keys[6], keychars);
                long num2 = FromCustomBase(keys[0], keychars) + FromCustomBase(keys[1], keychars) - FromCustomBase(keys[2], keychars) + FromCustomBase(keys[3], keychars) + FromCustomBase(keys[4], keychars) + FromCustomBase(keys[5], keychars) + FromCustomBase(keys[6], keychars);
                long num3 = FromCustomBase(keys[0], keychars) + FromCustomBase(keys[1], keychars) + FromCustomBase(keys[2], keychars) - FromCustomBase(keys[3], keychars) + FromCustomBase(keys[4], keychars) + FromCustomBase(keys[5], keychars) + FromCustomBase(keys[6], keychars);
                long num4 = FromCustomBase(keys[0], keychars) + FromCustomBase(keys[1], keychars) + FromCustomBase(keys[2], keychars) + FromCustomBase(keys[3], keychars) - FromCustomBase(keys[4], keychars) + FromCustomBase(keys[5], keychars) + FromCustomBase(keys[6], keychars);
                long num5 = FromCustomBase(keys[0], keychars) + FromCustomBase(keys[1], keychars) + FromCustomBase(keys[2], keychars) + FromCustomBase(keys[3], keychars) + FromCustomBase(keys[4], keychars) - FromCustomBase(keys[5], keychars) + FromCustomBase(keys[6], keychars);
                long num6 = FromCustomBase(keys[0], keychars) + FromCustomBase(keys[1], keychars) + FromCustomBase(keys[2], keychars) + FromCustomBase(keys[3], keychars) + FromCustomBase(keys[4], keychars) + FromCustomBase(keys[5], keychars) - FromCustomBase(keys[6], keychars);

                long num7 = num.ToString().Sum(c => Convert.ToInt32(c)) + num2.ToString().Sum(c => Convert.ToInt32(c)) + num3.ToString().Sum(c => Convert.ToInt32(c)) + num4.ToString().Sum(c => Convert.ToInt32(c)) + num5.ToString().Sum(c => Convert.ToInt32(c)) + num6.ToString().Sum(c => Convert.ToInt32(c));


                if (i % 2 == 0)
                {
                    subints[i] /= (FromCustomBase(keys[0], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else if (i % 3 == 0)
                {
                    subints[i] /= (FromCustomBase(keys[1], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else if (i % 5 == 0)
                {
                    subints[i] /= (FromCustomBase(keys[2], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else if (i % 7 == 0)
                {
                    subints[i] /= (FromCustomBase(keys[3], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else if (i % 11 == 0)
                {
                    subints[i] /= (FromCustomBase(keys[4], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else if (i % 13 == 0)
                {
                    subints[i] /= (FromCustomBase(keys[5], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
                }

                else
                {
                    subints[i] /= (FromCustomBase(keys[6], keychars) * num) + (2 * num2) + (3 * num3) + (4 * num4) - (5 * num5) - (6 * num6) * (128 * num7) + 256;
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

        private void AddstringSplits() //Adds a string splitter at the end of every chunk, this is beacuse they change size when going through the encyptor
        {
            for (int i = 0; i < substrings.Count; i++)
            {
                substringsoutput[i] = substringsoutput[i] + splitchars[random.Next(splitchars.Length)];
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

        private void NewKey()
        {
            for (int i = 0; i < keys.Length; i++)
            {
                GenerateKey(i);
            }

            txtKey.Text = keys[0];
            txtKey1.Text = keys[1];
            txtKey2.Text = keys[2];
            txtKey3.Text = keys[3];
            txtKey4.Text = keys[4];
            txtKey5.Text = keys[5];
            txtKey6.Text = keys[6];
        }

        private void GenerateKey(int i) //Generates a random key in base 26 
        {
            char[] c = new char[5];
            byte[] randomNumber = new byte[1];

            for (int ii = 0; ii < c.Length; ii++)
            {
                securerandom.GetBytes(randomNumber);
                c[ii] = keychars[randomNumber[0] % keychars.Length];
            }

            keys[i] = new String(c);
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
            NewKey();
            EncodeDecode();
        }

        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e) //When user inputs stuff
        {
            if (txtInput.Text == "") txtOutput.Text = "";
            EncodeDecode();
        }

        private void TxtKey_TextChanged(object sender, TextChangedEventArgs e) //User can type in their own key
        {
            keys[0] = txtKey.Text;
            EncodeDecode();
        }

        private void TxtKey1_TextChanged(object sender, TextChangedEventArgs e) //User can type in their own key
        {
            keys[1] = txtKey1.Text;
            EncodeDecode();
        }

        private void TxtKey2_TextChanged(object sender, TextChangedEventArgs e) //User can type in their own key
        {
            keys[2] = txtKey2.Text;
            EncodeDecode();
        }

        private void TxtKey3_TextChanged(object sender, TextChangedEventArgs e) //User can type in their own key
        {
            keys[3] = txtKey3.Text;
            EncodeDecode();
        }

        private void TxtKey4_TextChanged(object sender, TextChangedEventArgs e) //User can type in their own key
        {
            keys[4] = txtKey4.Text;
            EncodeDecode();
        }

        private void TxtKey5_TextChanged(object sender, TextChangedEventArgs e) //User can type in their own key
        {
            keys[5] = txtKey5.Text;
            EncodeDecode();
        }

        private void TxtKey6_TextChanged(object sender, TextChangedEventArgs e) //User can type in their own key
        {
            keys[6] = txtKey6.Text;
            EncodeDecode();
        }
    }
}
