using System.Collections;
using System.IO;
using System;
using System.Net;
using UnityEngine;

public class Config
{
    protected static Hashtable table = new Hashtable();
    
	private static Config instance = null;

    public Config()
    {
        table["GENERATE_TICK_MESSAGE"] = "true";

        // Proxy config
        table["MY_ID"] = "PROXY";
        table["MY_IP"] = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
        table["MY_PORT"] = "16315";

        // Router config
        table["ROUTER_ID"] = "ROUTER";
        table["ROUTER_IP"] = "192.168.1.48";
        table["ROUTER_PORT"] = "16312";

        // Spawner config
        table["SPAWNER_ID"] = "SPAWNER";

        // Unread messages management
        table["UNREAD_MESSAGES_CHECK_INTERVAL"] = "10";
        table["UNREAD_MESSAGES_RETRIEVAL_LIMIT"] = "1";
        table["NO_ACK_MESSAGES"] = "true";

        // Time for sleeping in server loop
        table["SERVER_LOOP_SLEEP_TIME"] = "100";

        // Time interval for sending perception (map-info, physiological) messages in milliseconds
        table["MESSAGE_SENDING_INTERVAL"] = "100";
        // Interval between time ticks (in milliseconds)
        table["TICK_INTERVAL"] = "500";

        // Number of simulated milliseconds per tick 
        // (Default value == TICK_INTERVAL) => (simulated time == real time)
        // For accelerating the simulated time (useful for automated tests), 
        // this value must/may be increased. 
        table["MILLISECONDS_PER_TICK"] = "500";

        // Parameters for controlling Physiological feelings:
		// 480 = 60/3 * 24 means the eat action is expected to happen once per 3 minute 
		// when the virtual pet does nothing else
        table["EAT_STOPS_PER_DAY"] = "480";
        table["DRINK_STOPS_PER_DAY"] = "480";
        table["PEE_STOPS_PER_DAY"] = "480";
        table["POO_STOPS_PER_DAY"] = "480";
		table["MAX_ACTION_NUM"] = "50";  // Maximum number of actions the avatar can do without eating
        table["EAT_ENERGY_INCREASE"] = "0.55";
//		table["AT_HOME_DISTANCE"] = "3.8";  // Avatar is at home, if the distance between avatar and home is smalled than this value
//		table["FITNESS_INCREASE_AT_HOME"] = "0.008333"; // Equals 1/(60/0.5), need 60 seconds at most to increase to 1
		table["FITNESS_DECREASE_OUTSIDE_HOME"] = "0.005556";  // Equals 1/(60*1.5/0.5), need 1.5 minutes at most to decrease to 0
        table["POO_INCREASE"] = "0.05";
        table["DRINK_THIRST_DECREASE"] = "0.15";
        table["DRINK_PEE_INCREASE"] = "0.05";	
		table["INIT_ENERGY"] = "1.0"; 
		table["INIT_FITNESS"] = "0.80"; 

        // Interval between messages sending (in milliseconds)
        table["MESSAGE_SENDING_INTERVAL"] = "100";

        // Map min/max position
        table["GLOBAL_POSITION_X"] = "500";	//"-165000";
        table["GLOBAL_POSITION_Y"] = "500";	//"-270000";
        table["AVATAR_VISION_RADIUS"] = "200";	//"64000";

        // Golden standard generation
        table["GENERATE_GOLD_STANDARD"] = "true";
        // filename where golden standard message will be recorded 
        table["GOLD_STANDARD_FILENAME"] = "GoldStandards.txt";

        table["AVATAR_STORAGE_URL"] = ""; // Default is Jack's appearance
		
		// There are six levels: NONE, ERROR, WARN, INFO, DEBUG, FINE.
		table["LOG_LEVEL"] = "DEBUG";
		
		// OpenCog properties persistence data file
		table["OCPROPERTY_DATA_FILE"] = ".\\oc_properties.dat";
  
    }
    
	public static Config getInstance()
	{
		if (instance == null)
			instance = new Config();
		return instance;
	}

    /**
     * Load passed file and redefines values for parameters.
     *
     * Parameters which are not mentioned in the file will keep
     * their default value. 
     * 
     * Parameters which do not have default values are
     * discarded.
     */
    public void loadFromFile(string fileName) 
    {
	    StreamReader reader = new StreamReader(fileName);
	    char[] separator = {'=',' '};
        int linenumber = 0;
	    
		while (!reader.EndOfStream){
	        string line = reader.ReadLine();
            linenumber++;
	        
	        // not a commentary or an empty line
	        if(line.Length > 0 && line[0] != '#'){
	            string[] tokens = line.Split(separator,StringSplitOptions.RemoveEmptyEntries);
	            if (tokens.Length < 2)
	            {
	                if (Debug.isDebugBuild)
                        Debug.LogError("Invalid format at line " + linenumber +": '" + line + "'");
	            }
	            if (table.ContainsKey(tokens[0])) 
	            {
	                //if (Debug.isDebugBuild) Debug.Log(tokens[0] + "=" + tokens[1]);
	                table[tokens[0]] = tokens[1];
	            }
	            else
	            {
                    Debug.LogWarning("Ignoring unknown parameter name '" + tokens[0] + "' at line "
                            + linenumber + ".");
	        	}           
	        }
	    }

	    reader.Close();    
	}

    /**
     * Return current value of a given parameter.
     *
     * @return Current value of a given parameter.
     */
    public string get(string paramName, string DEFAULT="")
    {
        if (table.ContainsKey(paramName)) {
            return (string) table[paramName];
        } else {
            return DEFAULT;
        }
    }

    public long getLong(string paramName, long DEFAULT=0)
    {
        if (table.ContainsKey(paramName)) {
            return long.Parse((string)table[paramName]);
        } else {
            return DEFAULT;
        }
    }
	
	public int getInt(string paramName, int DEFAULT=0)
    {
        if (table.ContainsKey(paramName)) {
            return int.Parse((string)table[paramName]);
        } else {
            return DEFAULT;
        }
    }

    public float getFloat(string paramName, long DEFAULT=0)
    {
        if (table.ContainsKey(paramName)) {
            return float.Parse((string)table[paramName]);
        } else {
            return DEFAULT;
        }
    }
}

