﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CorazonLibrary.Obfuscation
{
    /// <summary>
    /// Obfuscate (mask) and deobfuscate (unmask)
    ///  : Integer value <-> Masked string value
    /// </summary>
    public class CObfuscation
    {
        /// <summary>
        /// The characters which can be obfuscated.
        /// By default, the values contain numerics since we are trying to mask/unmask an integer.
        /// </summary>
        private string AllowedInput = "0123456789";

        /// <summary>
        /// The characters which will be used to represent the obfuscated string.
        /// This set cannot overlap with DummyKeys set.
        /// </summary>
        private string PossibleKeys = "cnbetvqizlu8mnj3eoap1fjprvqfgw4b9my0och7wlxszu";
        
        /// <summary>
        /// The dummy characters to be added in case the obfuscated string is less than MinimumLength.
        /// This set cannot overlap with PossibleKeys set.
        /// </summary>
        private string DummyKeys = "dkixrtkh5yg6s2da";
        
        /// <summary>
        /// The minimum length which the obfuscated key needs to have.
        /// </summary>
        private int MinimumKeyLength = 8;

        /// <summary>
        /// The list of keys.
        /// </summary>
        private List<List<char>> Keys { get; set; }

        /// <summary>
        /// Initialize object.
        /// Objective: Initialize the Keys which will be used for obfuscation/de-obfuscation process.
        /// </summary>
        public CObfuscation()
        {
            // get the count of the possible input characters
            int allowedInputCount = AllowedInput.Length;
            
            // initialize the Keys list
            Keys = new List<List<char>>();
            for (int i = 0; i < allowedInputCount; i++)
                Keys.Add(new List<char>());

            // assign values to the Keys list by each character from now 'scrambled' PossibleKeys
            int index = 0;
            foreach (char c in PossibleKeys.ToCharArray())
            {
                Keys[index].Add(c);

                // increment index in a circular fashion
                index = (index + 1) % allowedInputCount;
            }
        }

        /// <summary>
        /// Obfuscate an integer.
        /// </summary>
        /// <param name="value">The value to be obfuscated.</param>
        /// <returns>Returns the obfuscated string if successful, null otherwise.</returns>
        public string Obfuscate(int value)
        {
            // (i.e. duplicate = Keys.clone()) - make a copy to the Keys first, so that any changes made won't affect the original key
            List<List<char>> duplicateKeyList = CreateDuplicateKeyList(Keys);

            string result = "", valueString = value.ToString();
            foreach (char c in valueString.ToCharArray())
            {
                // get the index location of char c in the string: AllowedInput
                int indexInAllowedInputCollection = AllowedInput.IndexOf(c);
                if (indexInAllowedInputCollection == -1) return null;       // doesn't exist in the list of allowed chars

                // set the matched character representation of char c at position 0 in inner List<char>
                char matched = duplicateKeyList[indexInAllowedInputCollection][0];

                // move character from position 0 in inner List<char> to the last
                duplicateKeyList[indexInAllowedInputCollection].RemoveAt(0);
                duplicateKeyList[indexInAllowedInputCollection].Add(matched);

                // append matched char representation to string isSuccess
                result += matched;
            }

            // return the obfuscated string, adding a set of dummy keys if required.
            return AddDummyKey(result, value);
        }

        /// <summary>
        /// De-obfuscate a previously obfuscated string.
        /// </summary>
        /// <param name="text">The text which represents an obfuscated integer.</param>
        /// <returns>Returns the original integer if successful, null otherwise.</returns>
        public int? DeObfuscate(string text)
        {
            // null string cannot be de-obfuscated
            if (String.IsNullOrEmpty(text))
                return null;

            // variables
            string deobfuscatedString = "";
            int? deobfuscatedValue = null;
            
            // (i.e. duplicate = Keys.clone()) - make a copy to the Keys first, so that any changes made won't affect the original key
            List<List<char>> duplicateKeyList = CreateDuplicateKeyList(Keys);

            // first, remove the dummy characters from text
            text = RemoveDummyKey(text);

            // begin de-obfuscating process
            foreach (char character in text.ToCharArray())
            {
                // get the index location of char c in the char collection: PossibleKeys
                int indexInList = GetIndexOfList(ref duplicateKeyList, character);
                if (indexInList == -1) return null;       // doesn't exist in the list of possible keys

                // append matched int representation to string result
                deobfuscatedString += AllowedInput[indexInList];
            }

            // safe type-casting
            int temp;
            if (Int32.TryParse(deobfuscatedString, out temp))
                deobfuscatedValue = temp;
            
            return deobfuscatedValue;
        }

        public string Scramble(string input)
        {
            // break the input string to array of characters
            char[] chars = input.ToArray();

            // init Random with the fixed seed, so it will always return the same scrambled string for the key
            Random random = new Random(1994);

            // loop through the input text character by character, and scramble its position
            for (int i = 0; i < chars.Length; i++)
            {
                int randomIndex = random.Next(0, chars.Length);
                char temp = chars[randomIndex];
                chars[randomIndex] = chars[i];
                chars[i] = temp;
            }

            // return the scrambled string
            return new string(chars);
        }

        /// <summary>
        /// Prepend set of dummy characters in case the key doesn't meet the minimum lenght.
        /// </summary>
        /// <param name="key">The key to add dummy characters.</param>
        /// <param name="originalInteger">The original integer of the obfuscated string.</param>
        /// <returns>Returns the key with dummy characters.</returns>
        public string AddDummyKey(string key, int originalInteger)
        {
            // get how many dummy characters are there in the possible dummy key set
            int dummyKeyCount = DummyKeys.Count();

            // init starting index of DummyKeys based on the original integer
            int index = (originalInteger + 1) % dummyKeyCount;

            // start looping to add dummy keys in case the obfuscated key is lesser than MinimumLenght
            while (key.Length < MinimumKeyLength)
            {
                key = DummyKeys[index] + key;
                index = (index + 1) % dummyKeyCount;
            }
            
            return key;
        }

        /// <summary>
        /// Remove set of dummy characters present in the text.
        /// </summary>
        /// <param name="text">The text to remove the dummy characters from.</param>
        /// <returns>Returns the text without any dummy character.</returns>
        public string RemoveDummyKey(string text)
        {
            // loop through all characters listed as dummy keys
            foreach (char c in DummyKeys)
            {
                // search for dummy character c in text. if exist, remove it
                text = text.Replace(c.ToString(), "");
            }
            
            return text;
        }

        /// <summary>
        /// Get the index location of the specified character in the list.
        /// </summary>
        /// <param name="list">The 2D list of character to find the character from.</param>
        /// <param name="toFind">The character to find.</param>
        /// <returns>Returns the index location of the character if successful, -1 otherwise.</returns>
        private int GetIndexOfList(ref List<List<char>> list, char toFind)
        {
            // loop through the list and try to find the character we want
            int index = 0;
            for (index = 0; index < list.Count; index++)
            {
                // find at position 0 always
                if (list[index][0] == toFind)
                {
                    // move character from position 0 in inner List<char> to the last
                    char matched = list[index][0];
                    list[index].RemoveAt(0);
                    list[index].Add(matched);

                    // found at list[index]
                    return index;
                }
                // else not found, continue loop until index reaches list.Count
            }

            // not found
            return -1;
        }

        /// <summary>
        /// Create a duplicate key.
        /// </summary>
        /// <param name="keys">The original key to duplicate.</param>
        /// <returns>Returns the duplicate (a new object) of the key passed.</returns>
        private List<List<char>> CreateDuplicateKeyList(List<List<char>> keys)
        {
            // initialize new object
            List<List<char>> result = new List<List<char>>();

            // use two loops to copy each char value from the original keys to the duplicate one
            foreach (List<char> lc in keys)
            {
                List<char> newLC = new List<char>();
                foreach (char character in lc)
                {
                    newLC.Add(character);
                }
                result.Add(newLC);
            }
            
            return result;
        }
    }
}