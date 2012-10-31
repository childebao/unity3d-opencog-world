using System;
using System.Collections;
using System.Collections.Generic;

/**
 * Each action summary instance needs to contain physiological effect.
 */
public class PhysiologicalEffect
{
    #region statics
    public enum CostLevel { NONE = 0, LOW = 1, MEDIUM = 2, HIGH = 3, STRONG = 4 };
    #endregion
    
    #region Effect impacts
    public CostLevel level;
    public float energyIncrease = 0.0f;
    public float fitnessChange = 0.0f;
    public OCPhysiologicalModel.AvatarMode newMode = OCPhysiologicalModel.AvatarMode.ACTIVE;
    
    public Dictionary<string, float> changeFactors = new Dictionary<string,float>();
    public List<string> resetFactors = new List<string>();
    
    #endregion
    
    /**
     * The actual energy cost is calculated in getActionCost function. 
     * It is derived from BASE_ENERGY_COST, fitness and level. 
     */
    private float BASE_ENERGY_COST;
    
    /**
     * Create a system parameters instance.
     */
    private Config config = Config.getInstance();
    
    public PhysiologicalEffect(CostLevel level)
    {
        this.level = level;
        // MAX_ACTION_NUM is the number of normal actions possible on a full battery charge.
        this.BASE_ENERGY_COST = 1.0f / config.getInt("MAX_ACTION_NUM");
    }
    
    public void applyEffect(OCPhysiologicalModel model)
    {
        // Update energy
        model.energy -= getActionCost((float)model.fitness);
        model.energy += energyIncrease;
        
        model.fitness += fitnessChange;
        
        // Set new mode
        model.currentMode = newMode;
        // Change factors...
        foreach(String factorName in changeFactors.Keys) {
            float changeValue = changeFactors[factorName];
            if (changeValue < 0.0f) 
                model.basicFactorMap[factorName].decrease(-changeValue);
            else 
                model.basicFactorMap[factorName].increase(changeValue);
        }
        // Reset factors
        foreach(String factorName in resetFactors) {
            model.basicFactorMap[factorName].reset();
        }

        // Deal with the action which has effects on the physiological factors.
        
        // For reference, these are the old physiological effect of actions.
        /*switch (effect.actionName)
        {
            case "sleep":
                this.currentMode = AvatarMode.SLEEP;
                break;
            case "eat":
                // increase the energy
                this.energy += EAT_ENERGY_INCREASE;
                // decrease the hunger
                basicFactorMap["hunger"].decrease(this.energy);
                // increase the poo urgency
                basicFactorMap["poo_urgency"].increase(EAT_POO_INCREASE);
                break;
            case "drink":
                // decrease the thirst
                basicFactorMap["thirst"].decrease(DRINK_THIRST_DECREASE);
                // increase the pee urgency
                basicFactorMap["pee_urgency"].increase(DRINK_PEE_INCREASE);
                break;
            case "pee":
                // reset the pee urgency
                basicFactorMap["pee_urgency"].reset();
                break;
            case "poo":
                // reset the poo urgency
                basicFactorMap["poo_urgency"].reset();
                break;
            default:
                break;
        }*/
        
        
    }
    
    /**
     * Calculate the actual energy cost according to BASE_ENERGY_COST, fitness and level.  
     * Higher energy cost is produced with lower fitness, higher BASE_ENERGY_COST and level. 
     */
    public float getActionCost(float fitness)
    {
        return (float) (1.5 - fitness) * ((int)level * BASE_ENERGY_COST);
    }

}

