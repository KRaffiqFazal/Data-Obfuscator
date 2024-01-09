using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Redgate_Assignment
{
  internal class Program
  {
    static void Main(string[] args)
    {
      if (args.Length != 2)
      {
        Console.WriteLine("Usage: .\\DataMasker.exe data_file_path rules_file_path");
        Environment.Exit(0);
      }

      string _dataFilePath = args[0];
      string _rulesFilePath = args[1];

      JArray _dataFile = FileContents(_dataFilePath);
      JArray _rulesFile = FileContents(_rulesFilePath);

      Dictionary<string, Regex> _rules = ConvertRules(_rulesFile);
      List<Dictionary<string, string>> _data = JsonData(_dataFile);

      List<Dictionary<string, string>> _obfuscatedData = DataObfuscation(_data, _rules);

      JArray _writeToFile = JArray.FromObject(_obfuscatedData); // Ref: C

      File.WriteAllText("output.json", _writeToFile.ToString());
      Console.WriteLine(_writeToFile.ToString());
    }
    /// <summary>
    /// Takes in a file path and reads the file.
    /// </summary>
    /// <param name="filePath">Path to JSON file</param>
    /// <returns>key value pairs from JSON file as a JArray</returns>
    static JArray FileContents(string filePath)
    {
      if (!File.Exists(filePath)) // Ref: A.
      {
        Console.WriteLine($"File not found at: {filePath}");
        Environment.Exit(0);
      }
      string _fileContents = File.ReadAllText(filePath);
      JArray _objFile = JArray.Parse(_fileContents); // Ref: B
      
      return _objFile;
    }
    /// <summary>
    /// Converts text into rules.
    /// </summary>
    /// <param name="ruleFileContents">JSON converted into an array of key value rules</param>
    /// <returns>Dictionary with rules for keys and rules for values in no order</returns>
    static Dictionary<string, Regex> ConvertRules(JArray ruleFileContents)
    { 
      Dictionary<string, Regex> _rules = new Dictionary<string, Regex>(); // Key/value : regex pattern.
      Regex _correspondingPattern;
      String _key;
      String _value;
      foreach (JToken _entry in ruleFileContents)
      {
        _key = _entry.ToString().Split(':')[0];
        _value = _entry.ToString().Split(":")[1];
        if (_key.Equals("k"))
        {
          _correspondingPattern = new Regex(_value);
          _rules.Add("key", _correspondingPattern);
        }
        else if (_key.Equals("v"))
        {
          _correspondingPattern = new Regex(_value);
          _rules.Add("value", _correspondingPattern);
        }
      }
      return _rules;
    }
    /// <summary>
    /// Processes data file's contents as a list of dictionaries with each dictionary being an entry.
    /// </summary>
    /// <param name="dataFileContents">file written into JArray data type</param>
    /// <returns>data in a string format</returns>
    static List<Dictionary<string, string>> JsonData(JArray dataFileContents)
    {
      List<Dictionary<string, string>> _dataList = new List<Dictionary<string, string>>(); // Data entries separated in a list with a dictionary for each key value pair entry.
      Dictionary<string, string> _entry;
      foreach (JObject _item in dataFileContents)
      {
        _entry = new Dictionary<string, string>();
        foreach (var property in _item.Properties())
        {
          _entry.Add(property.Name, property.Value.ToString());
        }
        _dataList.Add(_entry);
      }
      return _dataList;
    }
    /// <summary>
    /// Obfuscates data based on rules.
    /// </summary>
    /// <param name="toObfuscate">data to be obfuscated</param>
    /// <param name="rules">rules to be obfuscated with</param>
    /// <returns>obfuscated data as a list of dictionaries where each dictionary is an entry</returns>
    static List<Dictionary<string, string>> DataObfuscation(List<Dictionary<string, string>> toObfuscate, Dictionary<string, Regex> rules)
    {
      List<Dictionary<string, string>> _obfuscatedInformation = new List<Dictionary<string, string>>();
      Dictionary<string, string> _newEntry;
      foreach (Dictionary<string, string> _entry in toObfuscate)
      {
        _newEntry = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> _pair in _entry)
        {
          _newEntry.Add(_pair.Key, ObfuscateValue(_pair, rules));
        }
        _obfuscatedInformation.Add(_newEntry);
      }
      return _obfuscatedInformation;
    }
    /// <summary>
    /// Scans rules to see if key and value need to be obfuscated.
    /// </summary>
    /// <param name="toObfuscate">key value combination to check with the rules</param>
    /// <param name="rules">all rules corresponding to key or value</param>
    /// <returns>the obfuscated value with the regex rules</returns>
    static string ObfuscateValue(KeyValuePair<string, string> toObfuscate, Dictionary<string, Regex> rules)
    {
      string _value = toObfuscate.Value;
      foreach (KeyValuePair<string, Regex> _pair in rules)
      {
        if (_pair.Key.Equals("key"))
        {
          if (_pair.Value.IsMatch(toObfuscate.Key))
          {
            _value = new string('*', toObfuscate.Value.Length);
            break;
          }
        }
        if (_pair.Key.Equals("value"))
        {
          if (_pair.Value.IsMatch(toObfuscate.Value)) // Assumes that there are no 2 regex that obfuscate parts of each other, e.g. in "ABC" if patterns of "AB" and "BC" exist, "BC" will not be obfuscated as the value becomes "**C".
          {
            _value = Regex.Replace(_value, _pair.Value.ToString(), match => new string('*', match.Length)); // Ref: C
          }
        }
      }
      return _value;
    }
  }
}