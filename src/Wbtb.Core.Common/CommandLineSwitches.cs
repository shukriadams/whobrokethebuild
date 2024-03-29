﻿////////////////////////////////////////////////////////////////
// CommandLineSwitches - Parses command line args to switches //
// Shukri Adams (shukri.adams@gmail.com)                      //
// https://github.com/shukriadams/commandLineSwitches         //
// MIT License (MIT) Copyright (c) 2018 Shukri Adams          //
////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Parses command line arguments into key-value pairs. For example, if an app is started "app.exe -i foo -j bar"
    /// this.Arguments will contain two items, {i,foo} and {j, bar}.
    /// </summary>
    public class CommandLineSwitches
    {
        /// <summary>
        /// Parsed key-values for arguments.
        /// </summary>
        public List<KeyValuePair<string, string>> Arguments { get; set; }

        /// <summary>
        /// If true, all switches will be forced to lower case.
        /// </summary>
        private readonly bool _convertKeysToLower;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">Raw command line args, such as those passed into your command line app.</param>
        /// <param name="switchLead">Lead for switches. All argument switches must be the same. For "-i foo -j bar", "-" is the lead.</param>
        /// <param name="convertKeysToLower">Set to true if you want your command line switches be stored in lower case. Example, -I fOo will yield {i, fOo}. Value is not affected. Default false.</param>
        public CommandLineSwitches(string[] args, string switchLead = "--", bool convertKeysToLower = true)
        {
           
            if (string.IsNullOrEmpty(switchLead))
                throw new ArgumentException("Lead is required.");

            this.Arguments = new List<KeyValuePair<string, string>>();
            _convertKeysToLower = convertKeysToLower;

            if (args == null || !args.Any())
                return;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (!arg.ToLower().StartsWith(switchLead))
                    continue;

                string key = arg.Substring(switchLead.Length);
                string value = null;

                if (args.Length > i + 1 && !args[i + 1].ToLower().StartsWith(switchLead))
                    value = args[i + 1];

                this.Arguments.Add(new KeyValuePair<string, string>(convertKeysToLower ? key.ToLower() : key, value));
            }
        }

        /// <summary>
        /// Returns true if the collection has an item of the given name
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            if (_convertKeysToLower)
                key = key.ToLower();

            return _convertKeysToLower ? this.Arguments.Any(r => r.Key.ToLower() == key) : this.Arguments.Any(r => r.Key == key);
        }

        /// <summary>
        /// Returns the value for a given key ; returns emptry string if key is not set.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            if (_convertKeysToLower)
                key = key.ToLower();

            if (!this.Contains(key))
                return string.Empty;

            return _convertKeysToLower ?
                this.Arguments.Single(r => r.Key.ToLower() == key).Value :
                this.Arguments.Single(r => r.Key == key).Value;
        }
    }
}