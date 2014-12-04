using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractionHandler : MonoBehaviour {

#region Variables

	//Publics
	public GameObject arrowTowerPool;
	public GameObject poisonTowerPool;
	public GameObject bombTowerPool;
	public GameObject frostTowerPool;
	public GameObject placementTiles;
	public GameObject backGround;
	public Sprite selectedSpirit;
	public LayerMask layersToHit; //For the unit selection interaction
	public float startGold = 250;
	public bool randomMode = false;
	public GameObject infosheet;

	public GameObject arrowTower;
	public GameObject poisonTower;
	public GameObject bombTower;
	public GameObject frostTower;

	public GUIStyle goldStyle;
	public GUIStyle normalStyle;

	public static int playerID;
	public static string mode;
	public static string towerinfo = "";
	public static int score = 0;

	public static float curGold = 100;
	public static int lifesRemaining = 50;
	public static bool gameOver = false;
	public static List<Tower> currentArrowTowers = new List<Tower>();
	public static List<Tower> currentPoisonTowers = new List<Tower>();
	public static List<Tower> currentBombTowers = new List<Tower>();
	public static List<Tower> currentFrostTowers = new List<Tower>();
	

	public delegate void GameOver();
	public static GameOver dGameOver; //Used for functions that needs to be called if game is over.

	//Privates 
	private Sprite 		originalSprite;
	private GameObject 	lastTileHit;
	private LayerMask 	tileLayer;
	private Vector3 	placementTilesOriginalPos;
	private bool 		isBuilding = false;
	private bool		towerSelected = false;
	private bool		creepSelected = false;

	private GameObject curTower;
	private List<Tower> arrowTowers = new List<Tower>();
	private List<Tower> poisonTowers = new List<Tower>();
	private List<Tower> bombTowers = new List<Tower>();
	private List<Tower> frostTowers = new List<Tower>();

	private Tower arrowT;
	private Tower poisonT;
	private Tower bombT;
	private Tower frostT;
	private Tower curT;
	private bool showTowerStats = false;

	private float arrowTowerCost;
	private float poisonTowerCost;
	private float bombTowerCost;
	private float frostTowerCost;
	
	private GameObject curSelectedTower;
	private GameObject curCreepSelected;

#endregion

	void Start()
	{
		playerID = Random.Range(1, 1000000);

		if(randomMode) {
			mode = "R";
		} else {
			mode = "E";
		}

		TestResults.Instance.SendStart();

		InteractionHandler.curGold = startGold;

		tileLayer = 1 << LayerMask.NameToLayer("Placement");

		placementTilesOriginalPos = placementTiles.transform.position;

		foreach(Transform t in arrowTowerPool.transform) { arrowTowers.Add(t.GetComponent<Tower>()); }
		foreach(Transform t in poisonTowerPool.transform) { poisonTowers.Add(t.GetComponent<Tower>()); }
		foreach(Transform t in bombTowerPool.transform) { bombTowers.Add(t.GetComponent<Tower>()); }
		foreach(Transform t in frostTowerPool.transform) { frostTowers.Add(t.GetComponent<Tower>()); }

		arrowT = arrowTower.GetComponent<Tower>();
		poisonT = poisonTower.GetComponent<Tower>();
		bombT = bombTower.GetComponent<Tower>();
		frostT = frostTower.GetComponent<Tower>();

		arrowTowerCost = arrowTowers[0].cost;
		poisonTowerCost = poisonTowers[0].cost;
		bombTowerCost = bombTowers[0].cost;
		frostTowerCost = frostTowers[0].cost;
	}

	void Update () 
	{
		if(InteractionHandler.gameOver) {
			if(curTower) {
				RangeIndicator.selected = false;
				curTower.SetActive(false);
				curTower = null;
			}
			return;
		}

		if(isBuilding) {
			TowerBuildingInteraction();
		} else {
			UnitSelectionInteraction();
		}
	}
	
	void TowerBuildingInteraction()
	{
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, tileLayer);
		
		//Seeing if mouse has hit any of the placement tiles if so change the material hihihih
		if(hit.collider != null){
			if(lastTileHit) {
				lastTileHit.GetComponent<SpriteRenderer>().sprite = originalSprite;
			}
			
			lastTileHit = hit.collider.gameObject;
			SpriteRenderer tmp = lastTileHit.GetComponent<SpriteRenderer>();
			originalSprite = tmp.sprite;
			tmp.sprite = selectedSpirit;
		} else {
			if(lastTileHit) {
				lastTileHit.GetComponent<SpriteRenderer>().sprite = originalSprite;
				lastTileHit = null;
			}
		}
		
		//If the player does not want to buy the tower
		if(Input.GetKeyDown(KeyCode.Escape)) {
			placementTiles.transform.position = placementTilesOriginalPos;
			isBuilding = false;
			if(curTower) {
				RangeIndicator.selected = false;
				curTower.transform.position = curTower.transform.parent.position;
				curTower.SetActive(false);
				curTower = null;
			}
		}
		
		//Make the current tower follow the mouse and if mouse is clicked build the tower :)
		if(curTower) {
			Vector3 goToPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			goToPos.z = 0;
			curTower.transform.position = goToPos;
			
			//Should probably change this one so that if the player clicks on something and lastObjectHit is null stop the buildPhase and remove tower
			if(Input.GetMouseButtonDown(0)) {
				if(lastTileHit) {
					BuildTower();
				} else {
					placementTiles.transform.position = placementTilesOriginalPos;
					isBuilding = false;
					if(curTower) {
						RangeIndicator.selected = false;
						curTower.transform.position = curTower.transform.parent.position;
						curTower.SetActive(false);
						curTower = null;
					}
				}
			} 
		}
	}

	void BuildTower()
	{
		curTower.transform.position = lastTileHit.transform.position;

		Tower tmpTower = curTower.GetComponent<Tower>();
		tmpTower.available = false;
		tmpTower.tileReplaced = lastTileHit;
		tmpTower.curNetWorth = tmpTower.cost;
		curGold -= tmpTower.cost;
		tmpTower.cost *= 4f;

		switch(tmpTower.towerType) {
		case TowerType.Arrow:
			currentArrowTowers.Add(tmpTower);
			towerinfo += "A";
			break;
		case TowerType.Poison:
			currentPoisonTowers.Add(tmpTower);
			towerinfo += "P";
			break;
		case TowerType.Bomb:
			currentBombTowers.Add(tmpTower);
			towerinfo += "B";
			break;
		case TowerType.Frost:
			currentFrostTowers.Add(tmpTower);
			towerinfo += "F";
			break;
		}

		towerinfo += WaveHandler.Instance.curWave;
		towerinfo += "P" + lastTileHit.name + "-";

		RangeIndicator.selected = false;
		lastTileHit.SetActive(false);
		curTower = null;
		placementTiles.transform.position = placementTilesOriginalPos;
		isBuilding = false;
	}

	void UpgradeTower()
	{
		Tower tmpTower = curSelectedTower.GetComponent<Tower>();

		curSelectedTower.GetComponent<SpriteRenderer>().sprite = tmpTower.lvlSprites[tmpTower.level];
		tmpTower.level++;
		tmpTower.damage *= 4f;
		tmpTower.dotDamage *= 4f;
		tmpTower.slow += 0.1f;
		tmpTower.curNetWorth += tmpTower.cost;
		curGold -= tmpTower.cost;
		tmpTower.cost *= 4f;

		towerinfo += "U" + tmpTower.level + "P" + tmpTower.tileReplaced.name + "-";
	}

	//This is used for tower and enemy selection
	void UnitSelectionInteraction()
	{
		if (Input.GetMouseButtonUp(0) && towerSelected) {
			towerSelected = false;
		}

		if (Input.GetMouseButtonUp(0) && creepSelected) {
			creepSelected = false;
		}

		if(!towerSelected && !creepSelected) {
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, layersToHit);

			//Check if mouse has hit anything
			if(hit.collider != null) {
				//If I hit a tower I will need to move the range indicator in place. If I don't hit a tower
				//and range indicator is selected, set selected to false
				if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Tower")) {
					RangeIndicator.SetTarget(hit.collider.transform, hit.collider.GetComponent<Tower>().radius);
					curSelectedTower = hit.collider.gameObject;
					RangeIndicator.selected = true;
					curCreepSelected = null;
					creepSelected = false;
				} else {
					towerSelected = false;
					curSelectedTower = null;
					RangeIndicator.selected = false;
					curCreepSelected = hit.collider.gameObject;
				}
			} else {
				RangeIndicator.selected = false;
				curSelectedTower = null;
				curCreepSelected = null;
			}
		}

		if(Input.GetMouseButtonUp(0) && curSelectedTower && !towerSelected) {
			towerSelected = true;
		}

		if(Input.GetMouseButtonUp(0) && curCreepSelected && !creepSelected) {
			creepSelected = true;
		}

	}

#region GUI related

	Rect aRect = new Rect(810, 450, 145, 40);
	Rect pRect = new Rect(810, 400, 145, 40);
	Rect fRect = new Rect(810, 350, 145, 40);
	Rect bRect = new Rect(810, 300, 145, 40);

	Color oriColor;

	bool pause = false;
	bool sheetActice = true;

	int lol = 0;
	void OnGUI()
	{
		if(pause) {
			if(Input.GetMouseButtonDown(0)) {
				pause = false;
				Time.timeScale = 1;
			}
		}

		if(sheetActice) {
			if(Input.GetMouseButtonDown(0)) {
				sheetActice = false;
				infosheet.SetActive(false);
				if(!EAWaveHandler.amIRunning)
					Time.timeScale = 1;
			}
		}

		GUI.skin.label.wordWrap = true;

		oriColor = GUI.contentColor;
		//For economy
		GUI.Label(new Rect(810, 10, 120, 20), "$$: " + (int)curGold, goldStyle);
		
		//For showing lives left
		GUI.Label(new Rect(810, 30, 120, 25), "Lifes: " + lifesRemaining, goldStyle);

		//For waves
		GUI.Label(new Rect(810, 50, 120, 25), "Wave " + (WaveHandler.Instance.curWave + 1) + " / " + WaveHandler.Instance.waves, goldStyle);

		if(gameOver)
			return;

		if(!WaveHandler.amIRunning && !EAWaveHandler.amIRunning)
			if(GUI.Button(new Rect(832, 75, 101, 25), "Start Wave")) {
				WaveHandler.Instance.StartWaveHandler();
			}

		if(WaveHandler.amIRunning) {
			if(!pause) {
				if(GUI.Button(new Rect(832, 75, 101, 25), "Pause")) {
					Time.timeScale = 0;
					pause = true;
				}
			} else {
				GUI.skin.box.wordWrap = true;
				GUI.skin.box.fontStyle = FontStyle.Bold;
				GUI.Box(new Rect(Screen.width / 2 - 75, Screen.height / 2 - 20, 150, 40), "Game is paused, press to continue");
			}
		}

		if(EAWaveHandler.amIRunning) {
			GUI.Label(new Rect(810, 70, 85, 25), "Evolving wave: " + ((float)EAWaveHandler.Instance.curGeneration / EAWaveHandler.Instance.generations * 100) + "%" ,goldStyle);
		}

		if(aRect.Contains(Input.mousePosition)) {
			curT = arrowT;
			showTowerStats = true;
			creepSelected = false;
		} else if (pRect.Contains(Input.mousePosition)) {
			curT = poisonT;
			showTowerStats = true;
			creepSelected = false;
		} else if (bRect.Contains(Input.mousePosition)) {
			curT = bombT;
			showTowerStats = true;
			creepSelected = false;
		} else if (fRect.Contains(Input.mousePosition)) {
			curT = frostT;
			showTowerStats = true;
			creepSelected = false;
		} else {
			showTowerStats = false;
		}

		//Tower building buttons
		if(curGold - arrowTowerCost >= 0) {
			GUI.contentColor = Color.green;
		} else {
			GUI.contentColor = Color.red;
		}
		if(GUI.Button(new Rect(810, 110, 145, 40), "Arrow Tower - " + arrowTowerCost)){ if(curGold - arrowTowerCost >= 0) FindNextTower(arrowTowers); }

		if(curGold - poisonTowerCost >= 0) {
			GUI.contentColor = Color.green;
		} else {
			GUI.contentColor = Color.red;
		}
		if(GUI.Button(new Rect(810, 160, 145, 40), "Poison Tower - " + poisonTowerCost)){ if(curGold - poisonTowerCost >= 0) FindNextTower(poisonTowers); }

		if(curGold - frostTowerCost >= 0) {
			GUI.contentColor = Color.green;
		} else {
			GUI.contentColor = Color.red;
		}
		if(GUI.Button(new Rect(810, 210, 145, 40), "Frost Tower - " + frostTowerCost)){ if(curGold - frostTowerCost >= 0) FindNextTower(frostTowers); }

		if(curGold - bombTowerCost >= 0) {
			GUI.contentColor = Color.green;
		} else {
			GUI.contentColor = Color.red;
		}
		if(GUI.Button(new Rect(810, 260, 145, 40), "Bomb Tower - " + bombTowerCost)){ if(curGold - bombTowerCost >= 0) FindNextTower(bombTowers); }

		if(towerSelected) {
			Vector3 up = Camera.main.WorldToScreenPoint(curSelectedTower.transform.position);
			//I need to minus with Screen.height because the above function gives my coordinates with lower left corner as (0,0)
			//while GUI has top left corner as (0,0).
			Tower tmpTower = curSelectedTower.GetComponent<Tower>();
			int offSetY = 40;
			int width = 100;
			int height = 20;

			if(curGold - tmpTower.cost >= 0) {
				GUI.contentColor = Color.green;
			} else {
				GUI.contentColor = Color.red;
			}

			if(tmpTower.level % 3 != 0) {
				if(GUI.Button(new Rect(up.x - width / 2 - 25, Screen.height - up.y - offSetY - 20, width + 50, height + 20), "Upgrade for " + tmpTower.cost + ", gives \n 4*dmg and +specials")) {
					if(curGold - tmpTower.cost >= 0) {
						UpgradeTower();
					}
				}
			}

			GUI.contentColor = oriColor;

			if(GUI.Button(new Rect(up.x - width / 2, Screen.height - up.y + (offSetY - height), width, height), "Sell for " + (int)((float)tmpTower.curNetWorth * 0.75f))) {

				switch(tmpTower.towerType) {
				case TowerType.Arrow:
					currentArrowTowers.Remove(tmpTower);
					towerinfo += "S" + "A";
					break;
				case TowerType.Poison:
					currentPoisonTowers.Remove(tmpTower);
					towerinfo += "S" + "P";
					break;
				case TowerType.Bomb:
					currentBombTowers.Remove(tmpTower);
					towerinfo += "S" + "B";
					break;
				case TowerType.Frost:
					currentFrostTowers.Remove(tmpTower);
					towerinfo += "S" + "F";
					break;
				}

				towerinfo += tmpTower.tileReplaced.name + "-";

				curGold += (float)tmpTower.curNetWorth * 0.75f;
				tmpTower.ResetTower();
				curSelectedTower = null;
				towerSelected = false;
			}

			int startPos = 320;

			//Show stats for the current tower
			GUI.Label(new Rect(810, startPos, 100, 25), "Name: ", goldStyle);
			GUI.Label(new Rect(855, startPos, 100, 25), tmpTower.name, normalStyle);
			GUI.Label(new Rect(810, startPos + 20, 145, 25), "Level: ", goldStyle);
			GUI.Label(new Rect(850, startPos + 20, 145, 25), tmpTower.level.ToString(), normalStyle);
			GUI.Label(new Rect(810, startPos + 40, 145, 25), "Damage: ", goldStyle);
			GUI.Label(new Rect(870, startPos + 40, 145, 25), tmpTower.damage.ToString(), normalStyle);
			GUI.Label(new Rect(810, startPos + 60, 145, 25), "Attack Cooldown: ", goldStyle);
			GUI.Label(new Rect(923, startPos + 60, 145, 25), tmpTower.fireRateCoolDown + "s", normalStyle);
			if(tmpTower.towerType == TowerType.Arrow) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack:", goldStyle);
				GUI.Label(new Rect(905, startPos + 80, 145, 100), "None", normalStyle);
				GUI.Label(new Rect(810, startPos + 100, 145, 25), tmpTower.attackType + " Attacks: ", goldStyle);
				GUI.Label(new Rect(810, startPos + 113, 145, 100), tmpTower.aboutTower);
				
			} else if (tmpTower.towerType == TowerType.Bomb) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack:", goldStyle);
				GUI.Label(new Rect(810, startPos + 93, 145, 100), "Bombs deal AoE damage");
				GUI.Label(new Rect(810, startPos + 131, 145, 25), tmpTower.attackType + " Attacks:", goldStyle);
				GUI.Label(new Rect(810, startPos + 144, 145, 100), tmpTower.aboutTower);
				
			} else if (tmpTower.towerType == TowerType.Poison) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack:", goldStyle);
				GUI.Label(new Rect(810, startPos + 93, 145, 100), "Applies a DoT to creep");
				GUI.Label(new Rect(810, startPos + 116, 145, 100), "DoT: ", goldStyle);
				GUI.Label(new Rect(842, startPos + 116, 145, 100), tmpTower.dotDamage + "d/1s for 5s", normalStyle);
				GUI.Label(new Rect(810, startPos + 136, 145, 100), tmpTower.attackType + " Attacks:", goldStyle);
				GUI.Label(new Rect(810, startPos + 149, 145, 100),  tmpTower.aboutTower);
			} else if (tmpTower.towerType == TowerType.Frost) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack:", goldStyle);
				GUI.Label(new Rect(810, startPos + 93, 145, 100), "Slows down an enemy creep");
				GUI.Label(new Rect(810, startPos + 131, 145, 100), "Slow: ", goldStyle);
				GUI.Label(new Rect(850, startPos + 131, 145, 100), (tmpTower.slow * 100) + "% for 5s", normalStyle);
				GUI.Label(new Rect(810, startPos + 149, 145, 100), tmpTower.attackType + " Attacks:", goldStyle);
				GUI.Label(new Rect(810, startPos + 162, 145, 100),  tmpTower.aboutTower);
			}
		}

		GUI.contentColor = oriColor;

		if(creepSelected) {
			Enemy tmpEnemy = curCreepSelected.GetComponent<Enemy>();

			GUI.Label(new Rect(810, 320, 145, 25), "Name: ", goldStyle);
			GUI.Label(new Rect(855, 320, 145, 25), tmpEnemy.name, normalStyle);
			GUI.Label(new Rect(810, 340, 145, 25), "Level: ", goldStyle);
			GUI.Label(new Rect(852, 340, 145, 25), tmpEnemy.Level.ToString(), normalStyle);
			GUI.Label(new Rect(810, 360, 145, 25), "HP: ", goldStyle);
			GUI.Label(new Rect(835, 360, 145, 25), tmpEnemy.health + " / " + tmpEnemy.CurStartHealth, normalStyle);
			GUI.Label(new Rect(810, 380, 145, 25), "Run Speed: ", goldStyle);
			GUI.Label(new Rect(885, 380, 145, 25), tmpEnemy.moveSpeed.ToString(), normalStyle);
			GUI.Label(new Rect(810, 400, 145, 25), "Slow Resistance: ", goldStyle);
			GUI.Label(new Rect(920, 400, 145, 25), (tmpEnemy.slowResistance * 100) + "%", normalStyle);
			GUI.Label(new Rect(810, 420, 145, 25), "Poison Armor: ", goldStyle);
			GUI.Label(new Rect(905, 420, 145, 25), tmpEnemy.poisonResistance.ToString(), normalStyle);
			GUI.Label(new Rect(810, 440, 145, 25), "Armor: ", goldStyle);
			GUI.Label(new Rect(857, 440, 145, 25), tmpEnemy.armor.ToString(), normalStyle);
			GUI.Label(new Rect(810, 460, 145, 50), "Armor Type: ", goldStyle);
			GUI.Label(new Rect(810, 473, 145, 50), tmpEnemy.armorType.ToString());

			if(!tmpEnemy.gameObject.activeSelf) {
				creepSelected = false;
				curCreepSelected = null;
			}
		}

		if(showTowerStats && !towerSelected) {

			int startPos = 320;

			GUI.Label(new Rect(810, startPos, 100, 25), "Name: ", goldStyle);
			GUI.Label(new Rect(855, startPos, 100, 25), curT.name, normalStyle);
			GUI.Label(new Rect(810, startPos + 20, 145, 25), "Cost: ", goldStyle);
			GUI.Label(new Rect(845, startPos + 20, 145, 25), curT.cost.ToString(), normalStyle);
			GUI.Label(new Rect(810, startPos + 40, 145, 25), "Damage: ", goldStyle);
			GUI.Label(new Rect(870, startPos + 40, 145, 25), curT.damage.ToString(), normalStyle);
			GUI.Label(new Rect(810, startPos + 60, 145, 25), "Attack Cooldown: ", goldStyle);
			GUI.Label(new Rect(923, startPos + 60, 145, 25), curT.fireRateCoolDown + "s", normalStyle);
			if(curT.towerType == TowerType.Arrow) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack:", goldStyle);
				GUI.Label(new Rect(905, startPos + 80, 145, 100), "None", normalStyle);
				GUI.Label(new Rect(810, startPos + 100, 145, 25), curT.attackType + " Attacks: ", goldStyle);
				GUI.Label(new Rect(810, startPos + 113, 145, 100), curT.aboutTower);

			} else if (curT.towerType == TowerType.Bomb) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack:", goldStyle);
				GUI.Label(new Rect(810, startPos + 93, 145, 100), "Bombs deal AoE damage");
				GUI.Label(new Rect(810, startPos + 131, 145, 25), curT.attackType + " Attacks:", goldStyle);
				GUI.Label(new Rect(810, startPos + 144, 145, 100), curT.aboutTower);

			} else if (curT.towerType == TowerType.Poison) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack:", goldStyle);
				GUI.Label(new Rect(810, startPos + 93, 145, 100), "Applies a DoT to creep");
				GUI.Label(new Rect(810, startPos + 116, 145, 100), "DoT: ", goldStyle);
				GUI.Label(new Rect(842, startPos + 116, 145, 100), curT.dotDamage + "d/1s for 5s", normalStyle);
				GUI.Label(new Rect(810, startPos + 136, 145, 100), curT.attackType + " Attacks:", goldStyle);
				GUI.Label(new Rect(810, startPos + 149, 145, 100),  curT.aboutTower);
			} else if (curT.towerType == TowerType.Frost) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack:", goldStyle);
				GUI.Label(new Rect(810, startPos + 93, 145, 100), "Slows down an enemy creep");
				GUI.Label(new Rect(810, startPos + 131, 145, 100), "Slow: ", goldStyle);
				GUI.Label(new Rect(850, startPos + 131, 145, 100), (curT.slow * 100) + "% for 5s", normalStyle);
				GUI.Label(new Rect(810, startPos + 149, 145, 100), curT.attackType + " Attacks:", goldStyle);
				GUI.Label(new Rect(810, startPos + 162, 145, 100),  curT.aboutTower);
			}
		}
		if(!sheetActice) {
			if(GUI.Button(new Rect(831, 570, 101, 25), "Info Sheet")) {
					sheetActice = true;
					infosheet.SetActive(true);
					if(!EAWaveHandler.amIRunning)
						Time.timeScale = 0;
			}
		}
	}

	void FindNextTower(List<Tower> towers)
	{
		for(int i = 0; i < towers.Count; i++) {
			if(towers[i].available) {
				curTower = towers[i].gameObject;
				RangeIndicator.SetTarget(curTower.transform, towers[i].radius);
				RangeIndicator.selected = true;
				curTower.SetActive(true);
				break;
			}
		}

		towerSelected = false;
		placementTiles.transform.position = backGround.transform.position;
		isBuilding = true;
	}

#endregion
}
