﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Scada.Data.Client
{
    public class Packet : PacketBase
    {
        // Const keys.
        public const string EntryKey = "entry";

        public const string StationKey = "station";

        public const string TokenKey = "token";

        // Content
        private JObject jobject = new JObject();

        private int result = 0;

        private bool hasResult = false;

        public bool IsFilePacket
        {
            get;
            set;
        }

        public string Options
        {
            get;
            set;
        }

        public Packet()
        {

        }

        public Packet(string token)
        {
            this.Station = Settings.Instance.Station;
            this.Token = token;
        }

        private int Result
        {
            get
            {
                return this.result;
            }
            set
            {
                this.result = value;
                this.hasResult = true;
            }
        }

        private string GetProperty(string propertyName)
        {
            return Packet.GetProperty(propertyName, this.jobject);
        }

        private static string GetProperty(string propertyName, JObject jsonObject)
        {
            JToken s = jsonObject[propertyName];
            if (s != null)
            {
                return s.ToString();
            }
            return string.Empty;
        }

        public string Station
        {
            get
            {
                return this.GetProperty(StationKey);
            }
            set
            {
                this.jobject[StationKey] = value;
            }
        }

        public string Token
        {
            get
            {
                return this.GetProperty(TokenKey);
            }
            set
            {
                this.jobject[TokenKey] = value;
            }
        }

        public override string ToString()
        {
            if (this.hasResult)
            {
                this.jobject["result"] = this.Result;
            }
            return this.jobject.ToString();
        }

        private JArray GetEntries()
        {
            JArray entries = (JArray)this.jobject[EntryKey];
            if (entries == null)
            {
                entries = new JArray();
                this.jobject[EntryKey] = entries;
            }
            return (JArray)entries;
        }

        internal void AddData(string deviceKey, Dictionary<string, object> data)
        {
            this.GetEntries().Add(this.GetObject(deviceKey, data));
        }

        internal JObject GetEntry(int index = 0)
        {
            return (JObject)this.GetEntries()[index];
        }

        internal void AppendEntry(JObject entry)
        {
            this.GetEntries().Add(entry);
        }

        private static string GetDataCenterDeviceId(string deviceKey)
        {
            // TODO:
            if (deviceKey == Devices.Hpic)
                return "hpic";
            else if (deviceKey == Devices.Shelter)
                return "environment";
            else if (deviceKey == Devices.Weather)
                return "weather";
            return "";
        }

        private JObject GetObject(string deviceKey, Dictionary<string, object> data)
        {
            JObject json = new JObject();
            json["device"] = GetDataCenterDeviceId(deviceKey);
            foreach (var kv in data)
            {
                if (kv.Key.ToLower() == "id")
                    continue;
                string value = (string)kv.Value;
                if (kv.Key.ToLower() == "time")
                {
                    if (kv.Value is string)
                    {
                        json["time"] = (string)kv.Value;//this.GetUnixTime((string)kv.Value);
                        continue;
                    }
                }
                if (kv.Value is string)
                {
                    Settings.DeviceCode code = Settings.Instance.GetCode(deviceKey, kv.Key);
                    if (code.DataType == "real")
                    {
                        double v;
                        if (double.TryParse(value, out v))
                        {
                            json[kv.Key] = v;
                        }
                    }
                    else if (code.DataType == "bit")
                    {
                        json[kv.Key] = 1;
                    }
                }
            }
            return json;
        }

        /* Move to ...
        private static DateTime StartTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));

        private long GetUnixTime(string time)
        {
            DateTime nowTime = DateTime.Now;
            DateTime dateTime = DateTime.Parse(time);
            long unixTime = (long)Math.Round((dateTime - StartTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }
        */

        public string Path
        {
            get;
            set;
        }
    }
}
