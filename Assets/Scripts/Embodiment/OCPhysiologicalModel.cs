#region Namespace
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Embodiment;
#endregion

public struct BasicPhysiologicalFactor
{
    public string name;
    public double value;
    public float frequency;
    private float millisecondsPerTick;
    private static long MILLISECONDS_PER_DAY = 24 * 60 * 60 * 1000;

    public BasicPhysiologicalFactor(string name, double value, int frequency, float millisecondsPerTick)
    {
        this.name = name;
        this.value = value;
        this.frequency = frequency;
        this.millisecondsPerTick = millisecondsPerTick;
    }

    /**
     * Update the value of physiological factor.
     */
    public void updateValue()
    {
		// (MILLISECONDS_PER_DAY / frequency) means how long the urgency will increase to 1
        double delta = this.millisecondsPerTick / (MILLISECONDS_PER_DAY / frequency);
        this.value = NumberUtil.zeroOneCut(this.value + delta);
        //Debug.Log("Physiological: " + this.name + " " + this.value);
    }

    public void increase(double delta)
    {
        this.value = NumberUtil.zeroOneCut(this.value + delta);
    }

    public void decrease(double delta)
    {
        this.value = NumberUtil.zeroOneCut(this.value - delta);
    }

    public void reset()
    {
        this.value = 0.0;
    }
}

/**
 * Physiological model for avatar, should be attached to avatar as a component.
 */
public class OCPhysiologicalModel : MonoBehaviour
{
    #region Constants
    /**
     * Constants the model needs.
     */
	private long MILLISECONDS_PER_DAY = 24 * 60 * 60 * 1000;

    /**
     * Actually, the following parameters should be decided by the amount 
     * of eating and drinking.
     */
    public double EAT_ENERGY_INCREASE;
    public double EAT_POO_INCREASE;
    public double DRINK_THIRST_DECREASE;
    public double DRINK_PEE_INCREASE;	
//	private double AT_HOME_DISTANCE; 
//	private double FITNESS_INCREASE_AT_HOME; 
	private double FITNESS_DECREASE_OUTSIDE_HOME; 
	
//	private bool at_home_flag; 
    #endregion
	
    #region Private Variables
    // Private variables:
    private double IDLE_ENERGY_DECREASE_RATE;
	private double SLEEP_ENERGY_INCREASE_RATE;
	private double STARVING_ENERGY_DECREASE_RATE;
			
    private OCConnector connector;
    /**
     * Avatar mode
     */
    public enum AvatarMode
    {
        SLEEP,
        IDLE,
        ACTIVE
    }

    /**
     * Current mode of avatar.
     */
    public AvatarMode currentMode;
    private int[] modeCounts;

    /**
	 * Create a system parameters instance.
	 */
    private Config config = Config.getInstance();

    /**
     * Update the physiological model every 0.5 second.
     */
    private readonly float updateInterval = 0.5f;
    /**
     * The timer accumulates during the update, after the value of timer exceeding 
     * the interval, some actions would be triggered.
     */
    private float updateTimer = 0.0f;

    private float millisecondsPerTick;

    /**
     * Compound physiological factors
     */
    public double energy;
    private double _fitness; // Currently equivalent to the "integrity" level.
	
	public double fitness {
		get { return _fitness; }
		set { _fitness = value; }
	}

    /**
     * Map of basic physiological factors: hunger, thirst, pee urgency, poo urgency etc.
     */
    public Dictionary<string, BasicPhysiologicalFactor> basicFactorMap = new Dictionary<string,BasicPhysiologicalFactor>();
    // A quick way to traverse the dictionary "basicFactorMap" by recording all its keys, C# does not allow 
    // modifying the value when traversing a dictionary.
    private List<string> basicFactorList = null;
    /**
     * Summary for the value of all physiological factors, including hunger, thirst, pee urgency, poo urgency,
     * energy, fitness. The summary is used to send to OAC in a tick message.
     */
    private Dictionary<string, double> factorSummaryMap = new Dictionary<string, double>();
    #endregion

    /**
     * Setup the basic physiological factors before the model begins to run.
     * Currently, we have 4 basic factors: hunger, thirst, pee urgency, poo urgency.
     */
    public void setupBasicFactors()
    {
        basicFactorMap["hunger"] = new BasicPhysiologicalFactor("hunger", 0.0,
                                                            config.getInt("EAT_STOPS_PER_DAY"), millisecondsPerTick);
        basicFactorMap["thirst"] = new BasicPhysiologicalFactor("thirst", 0.0,
                                                            config.getInt("DRINK_STOPS_PER_DAY"), millisecondsPerTick);
        basicFactorMap["pee_urgency"] = new BasicPhysiologicalFactor("pee_urgency", 0.0,
                                                            config.getInt("PEE_STOPS_PER_DAY"), millisecondsPerTick);
        basicFactorMap["poo_urgency"] = new BasicPhysiologicalFactor("poo_urgency", 0.0,
                                                            config.getInt("POO_STOPS_PER_DAY"), millisecondsPerTick);

        basicFactorList = new List<string>();
        basicFactorList.AddRange(basicFactorMap.Keys);
    }

    /**
     * Update the value of basic factors such as hunger, thirst...
     */
    private void updateBasicFactors()
    {
        foreach (string key in this.basicFactorList)
        {
            basicFactorMap[key].updateValue();
        }
    }

    /**
     * Update energy according to some rules...
     */
    private void updateEnergy()
    {
        if (this.currentMode == AvatarMode.IDLE)
        {
            this.energy += IDLE_ENERGY_DECREASE_RATE;
        }
        else if (this.currentMode == AvatarMode.SLEEP)
        {
            this.energy += SLEEP_ENERGY_INCREASE_RATE;
        }

        if (basicFactorMap["hunger"].value > 0.9)
        {
            this.energy += STARVING_ENERGY_DECREASE_RATE;
        }

        if (basicFactorMap["thirst"].value > 0.9)
        {
            this.energy += STARVING_ENERGY_DECREASE_RATE;
        }
        this.energy = NumberUtil.zeroOneCut(this.energy);
    }

    /**
     * Fitness is something related to basic physiological factors.
     */
    private void updateFitness()
    {
		// We should update this sometime to reflect the impact of not enough energy on integrity
		/**
        this._fitness = 1.0 - (0.4 * basicFactorMap["hunger"].value +
                            0.3 * basicFactorMap["thirst"].value +
                            0.2 * basicFactorMap["poo_urgency"].value +
                            0.1 * basicFactorMap["pee_urgency"].value);
         */
 		
    }

    /**
     * Compute the physiological parameters in every tick.
     */
    public void timeTick()
    {
        updateBasicFactors();
        updateFitness();
        updateEnergy();
		
        this.modeCounts[(int)this.currentMode]++;

        if (currentMode != AvatarMode.SLEEP)
            currentMode = AvatarMode.IDLE;

        foreach (string factor in basicFactorMap.Keys)
        {
            factorSummaryMap[factor] = basicFactorMap[factor].value;
        }
        factorSummaryMap["energy"] = this.energy;	
		factorSummaryMap["fitness"] = this._fitness; 
		
        if (connector != null)
        {
            // Send updated values to OAC
            connector.SendMessage("sendAvatarSignalsAndTick", factorSummaryMap);

            // Also update values holding by OCConnector, which would be displayed 
            // in psi panel in unity
            connector.SetDemandValue("Energy", (float)this.energy);
            connector.SetDemandValue("Integrity", (float)this._fitness); 
        }	
    }

    /**
     * This method should be invoked to update the physiological model, 
     * once an action has been done.
     * Currently, it would be invoked by ActionManager, by using 
     *      SendMessage("processPhysiologicalEffect", phyEffect)
     */
    public void processPhysiologicalEffect(PhysiologicalEffect effect)
    {
		effect.applyEffect(this);
		// Ensure energy is in acceptable bounds
		this.energy = NumberUtil.zeroOneCut(this.energy);

    }

    #region Unity API
    void Awake()
    {
        // Initialize parameters below.
        this.modeCounts = new int[3];
        this.modeCounts[(int)AvatarMode.IDLE] = 0;
        this.modeCounts[(int)AvatarMode.SLEEP] = 0;
        this.modeCounts[(int)AvatarMode.ACTIVE] = 0;

        this.millisecondsPerTick = config.getLong("MILLISECONDS_PER_TICK");		
		
        this.IDLE_ENERGY_DECREASE_RATE = - this.millisecondsPerTick / 
                                           (MILLISECONDS_PER_DAY / config.getInt("EAT_STOPS_PER_DAY"));
        this.SLEEP_ENERGY_INCREASE_RATE = - IDLE_ENERGY_DECREASE_RATE * 5;
        this.STARVING_ENERGY_DECREASE_RATE = IDLE_ENERGY_DECREASE_RATE * 2;		
		
//		this.AT_HOME_DISTANCE = config.getFloat("AT_HOME_DISTANCE"); 
//		this.FITNESS_INCREASE_AT_HOME = config.getFloat("FITNESS_INCREASE_AT_HOME"); 
		this.FITNESS_DECREASE_OUTSIDE_HOME = config.getFloat("FITNESS_DECREASE_OUTSIDE_HOME"); 
		
        this.EAT_ENERGY_INCREASE = config.getFloat("EAT_ENERGY_INCREASE"); 
        this.EAT_POO_INCREASE = config.getFloat("EAT_POO_INCREASE"); 
        this.DRINK_THIRST_DECREASE = config.getFloat("EAT_THIRST_DECREASE"); 
        this.DRINK_PEE_INCREASE = config.getFloat("DRINK_PEE_INCREASE"); 
		
        this.energy = config.getFloat("INIT_ENERGY"); 		
        this._fitness = config.getFloat("INIT_FITNESS"); 
        this.currentMode = AvatarMode.IDLE;
		
//		this.at_home_flag = false; 

        setupBasicFactors();
        connector = gameObject.GetComponent<OCConnector>() as OCConnector;
    }

    void Update()
    {

    }
	
	/**
	 * Check if the avatar is at home 
	 */
/**
	private void CheckAtHomeStatus()
	{
		GameObject home = GameObject.Find("Home");
		
		if (home != null)
		{
			Vector3 homePos = home.transform.position;
			Vector3 myPos = gameObject.transform.position;
			homePos.y = myPos.y;
			if (Vector3.Distance(myPos, homePos) < 3f)
			{
				this.at_home_flag = true; 
			}
			else
			{
				this.at_home_flag = false; 
			}
		}
	}
*/
	
    void FixedUpdate()
    {
        updateTimer += Time.fixedDeltaTime;
        if (updateTimer >= updateInterval)
        {
//			CheckAtHomeStatus();
			
            // Some actions here
            timeTick();
			
            // Reset the timer.
            updateTimer = 0.0f;
        }
    }
    #endregion
}
