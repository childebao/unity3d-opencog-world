
using UnityEditor;


/// <summary>
/// Automation manager for player building and unit testing.
/// Example commandline usage:
/// "C:\Program Files (x86)\Unity\Editor\Unity.exe" -batchMode -quit -nographics -projectPath C:\project -executeMethod AutomationManager.BuildAll
/// </summary>
public class AutomationManager
{
	[MenuItem ("Build/BuildAll")]
	static void BuildAll()
	{
		BuildStandaloneLinuxPlayer();
		BuildStandaloneLinux64Player();
	}
	
	[MenuItem ("Build/BuildStandaloneLinux64Player")]
	static void BuildStandaloneLinux64Player()
	{
		string[] scenes = { "Assets/Scenes/GameScenes/MainGameScene.unity" };
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneLinux64);
		BuildPipeline.BuildPlayer(scenes
								  , "Players/Unity3DGameWorldPlayer_Linux64"
								  , BuildTarget.StandaloneLinux64, BuildOptions.None );
	}
	
	[MenuItem ("Build/BuildStandaloneLinuxPlayer")]
	static void BuildStandaloneLinuxPlayer()
	{
		string[] scenes = { "Assets/Scenes/GameScenes/MainGameScene.unity" };
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneLinux);
		BuildPipeline.BuildPlayer(scenes
								  , "Players/Unity3DGameWorldPlayer_Linux"
								  , BuildTarget.StandaloneLinux, BuildOptions.None );
	}
}
