using System.Collections.Generic;
using System;
using UnityEngine;

public enum BlockType : byte
{
	// old
//	TopSoil=0,
//    Dirt=1,
//    Light = 2,
//    Lava = 3,
//    Leaves=4,
//    Stone = 5,
//    Air = 255
	
	// new
	Air 	
,	Stone 	
,	TopSoil	 //aka grass block
,   Dirt	
,	Cobblestone 
,	WoodenPlanks
,	Saplings
,	Bedrock 
,	Water	
,	StationaryWater
,	Lava	
,	StationaryLava 
,	Sand	
,	Gravel	
,	GoldOre	
,	IronOre	
,	CoalOre	
,	Wood	
,	Leaves	
,	Sponge 	
,	Glass
,	LapisLazuliOre
,	LapisLazuliBlock
,	Dispenser
,	Sandstone
,	NoteBlock
,	Bed
,	PoweredRail
,	DetectorRail
,	StickyPiston
,	Cobweb
,	TallGrass
,	DeadBrush 
,	Piston
,	PistonExtension
,	Wool
,	BlockMovedByPiston
,	Dandelion 
,	Rose
,	BrownMushroom
,	RedMushroom
,	BlockOfGold
,	BlockOfIron
,	DoubleSlabs
,	Slabs
,	Bricks
,	TNT
,	Bookshelf
,	MossStone
,	Obsidian
,	Torch
,	Fire
,	MonsterSpawner
,	WoodenStairs
,	Chest
,	RedstoneWire
,	DiamondOre
,	BlockOfDiamond
,	CraftingTable
,	WheatSeeds
,	Farmland
,	Furnace
,	BurningFurnace
,	SignPost
,	WoodenDoor
,	Ladders
,	Rails
,	CobblestoneStairs
,	WallSign
,	Lever
,	StonePressurePlate
,	IronDoor
,	WoodenPressurePlate
,	RedstoneOre
,	GlowingRedstoneOre
,	RedstoneTorchOff
,	RedstoneTorchOn
,	StoneButton
,	Snow
,	Ice
,	SnowBlock
,	Cactus
,	ClayBlock
,	SugarCane
,	Jukebox
,	Fence
,	Pumpkin
,	Netherrack
,	SoulSand
,	GlowstoneBlock
,	Portal
,	JackOLantern
,	CakeBlock
,	RedstoneRepeaterOff
,	RedstoneRepeaterOn
,	LockedChest
,	Trapdoor
,	HiddenSilverfish
,	StoneBricks
,	HugeBrownMushroom
,	HugeRedMushroom
,	IronBars
,	GlassPane
,	Melon
,	PumpkinStem
,	MelonStem
,	Vines
,	FenceGate
,	BrickStairs
,	StoneBrickStairs
,	Mycelium
,	LilyPad
,	NetherBrick
,	NetherBrickFence
,	NetherBrickStairs
,	NetherWart
,	EnchantmentTable
,	BrewingStand
,	Cauldron
,	EndPortal
,	EndPortalFrame
,	EndStone
,	DragonEgg
,   Light
}

public enum BlockFace : byte
{
    Top = 0,
    Side = 1,
    Bottom = 2
}

/// <summary>
/// The data we store for each block
/// </summary>
public struct Block
{
//	public Block()
//	{
//		Debug.Log("Block");
//	}
	
    public static readonly Dictionary<string, BlockType> StringToTypeMap 
		= new Dictionary<string, BlockType>
	{
		{"TopSoil", BlockType.TopSoil},
		{"Dirt", BlockType.Dirt},
		{"Light", BlockType.Light},
		{"Lava", BlockType.Lava},
		{"Leaves", BlockType.Leaves},
		{"Stone", BlockType.Stone},
		{"Air",BlockType.Air},
		{"WoodenDoor", BlockType.WoodenDoor},
		{"IronDoor", BlockType.IronDoor},
		{"TrapDoor", BlockType.Trapdoor},
		{"Cobblestone", BlockType.Cobblestone},
		{"WoodenPlanks", BlockType.WoodenPlanks},
		{"Saplings", BlockType.Saplings},	
		{"Bedrock", BlockType.Bedrock},
		{"Water", BlockType.Water},
		{"StationaryWater", BlockType.StationaryWater},
		{"StationaryLava", BlockType.StationaryLava},
		{"Sand", BlockType.Sand},
		{"Gravel", BlockType.Gravel},
		{"GoldOre", BlockType.GoldOre},
		{"IronOre", BlockType.IronOre},
		{"CoalOre", BlockType.CoalOre},
		{"Wood", BlockType.Wood},
		{"Glass", BlockType.Glass},
		{"LapisLazuliBlock", BlockType.LapisLazuliBlock},
		{"Sandstone", BlockType.Sandstone},
		{"Bed", BlockType.Bed},
		{"Cobweb", BlockType.Cobweb},
		{"TallGrass", BlockType.TallGrass},
		{"DeadBrush", BlockType.DeadBrush},
		{"Piston", BlockType.Piston},
		{"Dandelion", BlockType.Dandelion},
		{"Rose", BlockType.Rose},
		{"BrownMushroom", BlockType.BrownMushroom},
		{"RedMushroom", BlockType.RedMushroom},
		{"BlockOfGold", BlockType.BlockOfGold},
		{"BlockOfIron", BlockType.BlockOfIron},
		{"Slabs", BlockType.Slabs},
		{"Bricks", BlockType.Bricks},
		{"TNT", BlockType.TNT},
		{"Bookshelf", BlockType.Bookshelf},
		{"MossStone", BlockType.MossStone},
		{"Obsidian", BlockType.Obsidian},
		{"Torch", BlockType.Torch},
		{"Fire", BlockType.Fire},
		{"WoodenStairs", BlockType.WoodenStairs},
		{"Chest", BlockType.Chest},
		{"DiamondOre", BlockType.DiamondOre},
		{"BlockOfDiamond", BlockType.BlockOfDiamond},
		{"CraftingTable", BlockType.CraftingTable},
		{"Farmland", BlockType.Farmland},
		{"Furnace", BlockType.Furnace},
		{"BurningFurnace", BlockType.BurningFurnace},
		{"Ladders", BlockType.Ladders},
		{"Rails", BlockType.Rails},
		{"CobblestoneStairs", BlockType.CobblestoneStairs},
		{"Lever", BlockType.Lever},
		{"Snow", BlockType.Snow},
		{"Ice", BlockType.Ice},
		{"SnowBlock", BlockType.SnowBlock},
		{"ClayBlock", BlockType.ClayBlock},
		{"Fence", BlockType.Fence},
		{"Portal", BlockType.Portal},
		{"LockedChest", BlockType.LockedChest},
		{"StoneBricks", BlockType.StoneBricks},
		{"IronBars", BlockType.IronBars},
		{"GlassPane", BlockType.GlassPane},
		{"FenceGate", BlockType.FenceGate},
		{"BrickStairs", BlockType.BrickStairs},
		{"StoneBrickStairs", BlockType.StoneBrickStairs},
		{"Cauldron", BlockType.Cauldron},
		{"EndPortal", BlockType.EndPortal},
		
		
		
		
		
	};
    public BlockType Type;
    public byte LightAmount;
}

public class BlockUVCoordinates
{
    private readonly Rect[] m_BlockFaceUvCoordinates = new Rect[3];

    public BlockUVCoordinates(Rect topUvCoordinates, Rect sideUvCoordinates, Rect bottomUvCoordinates)
    {
        BlockFaceUvCoordinates[(int)BlockFace.Top] = topUvCoordinates;
        BlockFaceUvCoordinates[(int)BlockFace.Side] = sideUvCoordinates;
        BlockFaceUvCoordinates[(int)BlockFace.Bottom] = bottomUvCoordinates;
    }


    public Rect[] BlockFaceUvCoordinates
    {
        get { return m_BlockFaceUvCoordinates; }
    }
}