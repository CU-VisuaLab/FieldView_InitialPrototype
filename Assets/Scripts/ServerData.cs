using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;

// Need a class to define the data we're expecting from the backend
public class Mountain : MonoBehaviour {
    [JsonProperty ("uid")]
    public string uid;
    [JsonProperty("title")]
    public string title;
    [JsonProperty("peak")]
    public string peak;
    [JsonProperty("description")]
    public string description;
    [JsonProperty("longitude")]
    public double longitude;
    [JsonProperty("latitude")]
    public double latitude;
    [JsonProperty("elevation")]
    public int elevation;
    [JsonProperty("altitude")]
    public double altitude;
    [JsonProperty("created")]
    public string created;
}

// Need a class to define a collection of mountains

// Note: Json.NET required .NET 4.6 in Unity.
public class ServerData : MonoBehaviour {
    public string serverGetAllUrl;

	// Use this for initialization
	void Start () {
        //Debug.Log(getNewData());
        //List<Mountain> mountains = JsonUtility.FromJson<List<Mountain>>(getNewData());
        //JsonArray arr = JsonParser.Deserialize(getNewData());
        
        /*List<Mountain> raw_mountains = JsonConvert.DeserializeObject<List<Mountain>>(getNewData());

        List<Mountain> mountains = new List<Mountain>();
        for (var i = 0; i < raw_mountains.Count; i++)
        {
            
            if (raw_mountains[i].altitude >= 14000 && raw_mountains[i].altitude <= 15000) mountains.Add(raw_mountains[i]);
        }
        GraphRender twoDimensionRenderer = GameObject.Find("MapScatterRender").GetComponent<GraphRender>();
        if (twoDimensionRenderer != null)
        {
            Debug.Log(mountains.Count);
            twoDimensionRenderer.loadRemoteData(mountains);
        }*/
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // Function to update only with new data
    private string getNewData(string ip_addr)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ip_addr);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        } catch
        {
            return "[]";
        }
    }
    public List<Mountain> getRemoteMountains(string ip_addr)
    {
        List<Mountain> mountains = new List<Mountain>();
        List<Mountain> raw_mountains = JsonConvert.DeserializeObject<List<Mountain>>(getNewData(ip_addr));
        for (var i = 0; i < raw_mountains.Count; i++)
        {
            if (raw_mountains[i].altitude >= 14000 && raw_mountains[i].altitude <= 15000) mountains.Add(raw_mountains[i]);
        }
        return mountains;
    }
    public List<Mountain> getMySQLData(string server_name, string database_name, string table_name, string user_name, string password)
    {
        List<Mountain> mountains = new List<Mountain>();
        
        WebClient client = new WebClient();
        string url = "http://cu-visualab.org/Fieldview/fieldview_mysql.php?password=" + password;
        byte[] html = client.DownloadData(url);
        UTF8Encoding utf = new UTF8Encoding();
        string dataString = utf.GetString(html);
       
        List<Mountain> raw_mountains = JsonConvert.DeserializeObject<List<Mountain>>(dataString);
        for (var i = 0; i < raw_mountains.Count; i++)
        {
            if (raw_mountains[i].elevation >= 14000 && raw_mountains[i].elevation <= 15000) mountains.Add(raw_mountains[i]);
        }
        return mountains;
    }
}
