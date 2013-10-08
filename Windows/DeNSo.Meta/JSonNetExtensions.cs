using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class JSonNetExtensions
{
  public static bool Contains(this JObject jo, string value)
  {
    return jo.Children().Where(v => (string)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, int value)
  {
    return jo.Children().Where(v => (int)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, int? value)
  {
    return jo.Children().Where(v => (int?)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, long value)
  {
    return jo.Children().Where(v => (long)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, long? value)
  {
    return jo.Children().Where(v => (long?)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, double value)
  {
    return jo.Children().Where(v => (double)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, double? value)
  {
    return jo.Children().Where(v => (double?)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, decimal value)
  {
    return jo.Children().Where(v => (decimal)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, decimal? value)
  {
    return jo.Children().Where(v => (decimal?)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, float value)
  {
    return jo.Children().Where(v => (float)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, float? value)
  {
    return jo.Children().Where(v => (float?)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, DateTime value)
  {
    return jo.Children().Where(v => (DateTime)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, byte value)
  {
    return jo.Children().Where(v => (byte)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, byte? value)
  {
    return jo.Children().Where(v => (byte?)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, short value)
  {
    return jo.Children().Where(v => (short)v == value).Count() > 0;
  }
  public static bool Contains(this JObject jo, short? value)
  {
    return jo.Children().Where(v => (short?)v == value).Count() > 0;
  }
}

