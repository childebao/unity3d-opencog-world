using UnityEngine;
using System.Collections;

public class AnimationManager : MonoBehaviour {
    // This class is just used to animate the player, not the avatars controlled by OpenCog
    public GameObject PlayerController; // this is the player's avatar's gameobject
    public GameObject PlayerModel;
	private Player player;
	void Start()
	{
		player = PlayerController.GetComponent<Player>() as Player;
	}

    void Update () {
        CharacterMotor ControlScript = PlayerController.GetComponent<CharacterMotor>() as CharacterMotor;

        if(ControlScript.movement.velocity != Vector3.zero) 
		{
            if(PlayerModel.animation["walk"])
                if (PlayerModel.animation.IsPlaying("walk") == false) PlayerModel.animation.Play("walk");
            else if(PlayerModel.animation["move"])
                if (PlayerModel.animation.IsPlaying("move") == false) PlayerModel.animation.Play("move");
        } 
		else 
		{
			if(player.isAutoWalking)
			{
				if(player.isMoving())
					if(PlayerModel.animation.IsPlaying("walk") == false) 
					{
                    	Debug.LogWarning("playing walk");
                    	PlayerModel.animation.Play("walk");
                	}
			}
			else
			{
	            if(PlayerModel.animation.IsPlaying("idle") == false)
	                PlayerModel.animation.Play("idle");
			}
        }
    }

}
