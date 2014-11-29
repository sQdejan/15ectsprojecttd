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

	public GameObject arrowTower;
	public GameObject poisonTower;
	public GameObject bombTower;
	public GameObject frostTower;

	public static float curGold = 100;
	public static int lifesRemaining = 100000;
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
			break;
		case TowerType.Poison:
			currentPoisonTowers.Add(tmpTower);
			break;
		case TowerType.Bomb:
			currentBombTowers.Add(tmpTower);
			break;
		case TowerType.Frost:
			currentFrostTowers.Add(tmpTower);
			break;
		}

		RangeIndicator.selected = false;
		lastTileHit.SetActive(false);
		curTower = null;
		placementTiles.transform.position = placementTilesOriginalPos;
		isBuilding = false;
	}

	void UpgradeTower()
	{
		Tower tmpTower = curSelectedTower.GetComponent<Tower>();

		tmpTower.level++;
		tmpTower.damage *= 4f;
		tmpTower.dotDamage *= 4f;
		tmpTower.slow += 0.1f;
		tmpTower.curNetWorth += tmpTower.cost;
		curGold -= tmpTower.cost;
		tmpTower.cost *= 4f;
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
					creepSelected = false;
				} else {
					towerSelected = false;
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

	Rect aRect = new Rect(810, 550, 145, 40);
	Rect pRect = new Rect(810, 500, 145, 40);
	Rect bRect = new Rect(810, 450, 145, 40);
	Rect fRect = new Rect(810, 400, 145, 40);

	void OnGUI()
	{
		if(aRect.Contains(Input.mousePosition)) {
			curT = arrowT;
			showTowerStats = true;
		} else if (pRect.Contains(Input.mousePosition)) {
			curT = poisonT;
			showTowerStats = true;
		} else if (bRect.Contains(Input.mousePosition)) {
			curT = bombT;
			showTowerStats = true;
		} else if (fRect.Contains(Input.mousePosition)) {
			curT = frostT;
			showTowerStats = true;
		} else {
			showTowerStats = false;
		}

		//Tower building buttons
		if(GUI.Button(new Rect(810, 10, 145, 40), "Arrow Tower")){ if(curGold - arrowTowerCost >= 0) FindNextTower(arrowTowers); }

		if(GUI.Button(new Rect(810, 60, 145, 40), "Poison Tower")){ if(curGold - poisonTowerCost >= 0) FindNextTower(poisonTowers); }

		if(GUI.Button(new Rect(810, 110, 145, 40), "Bomb Tower")){ if(curGold - bombTowerCost >= 0) FindNextTower(bombTowers); }

		if(GUI.Button(new Rect(810, 160, 145, 40), "Frost Tower")){ if(curGold - frostTowerCost >= 0) FindNextTower(frostTowers); }

		//For economy
		GUI.Label(new Rect(810, 240, 120, 20), "Current Gold: " + (int)curGold);

		//For showing lives left
		GUI.Label(new Rect(810, 270, 120, 25), "Lifes Remaining: " + lifesRemaining);

		if(towerSelected) {
			Vector3 up = Camera.main.WorldToScreenPoint(curSelectedTower.transform.position);
			//I need to minus with Screen.height because the above function gives my coordinates with lower left corner as (0,0)
			//while GUI has top left corner as (0,0).
			Tower tmpTower = curSelectedTower.GetComponent<Tower>();
			int offSetY = 40;
			int width = 100;
			int height = 20;

			if(tmpTower.level % 4 != 0) {
				if(GUI.Button(new Rect(up.x - width / 2 - 22, Screen.height - up.y - offSetY - 20, width + 44, height + 20), "Upgrade for " + tmpTower.cost + ", gives \n 4*dmg and +specials")) {
					if(curGold - tmpTower.cost >= 0) {
						UpgradeTower();
					}
				}
			}

			if(GUI.Button(new Rect(up.x - width / 2, Screen.height - up.y + (offSetY - height), width, height), "Sell for " + (int)((float)tmpTower.curNetWorth * 0.75f))) {

				switch(tmpTower.towerType) {
				case TowerType.Arrow:
					currentArrowTowers.Remove(tmpTower);
					break;
				case TowerType.Poison:
					currentPoisonTowers.Remove(tmpTower);
					break;
				case TowerType.Bomb:
					currentBombTowers.Remove(tmpTower);
					break;
				case TowerType.Frost:
					currentFrostTowers.Remove(tmpTower);
					break;
				}

				curGold += (float)tmpTower.curNetWorth * 0.75f;
				tmpTower.ResetTower();
				curSelectedTower = null;
				towerSelected = false;
			}

			int startPos = 320;

			//Show stats for the current tower
			GUI.Label(new Rect(810, startPos, 145, 25), "Name: " + tmpTower.name);
			GUI.Label(new Rect(810, startPos + 20, 145, 25), "Level: " + tmpTower.level);
			GUI.Label(new Rect(810, startPos + 40, 145, 25), "Damage: " + tmpTower.damage);
			GUI.Label(new Rect(810, startPos + 60, 145, 25), "Attack Cooldown: " + tmpTower.fireRateCoolDown);
			if(tmpTower.towerType == TowerType.Arrow) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack: None");
				GUI.Label(new Rect(810, startPos + 100, 145, 100), "Attack Type: " + tmpTower.attackType + " - " + tmpTower.aboutTower);
			} else if (tmpTower.towerType == TowerType.Bomb) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack: Bombs gives AoE damage");
				GUI.Label(new Rect(810, startPos + 120, 145, 100), "Attack Type: " + tmpTower.attackType + " - " + tmpTower.aboutTower);
			} else if (tmpTower.towerType == TowerType.Poison) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack: Applies a DoT to creep");
				GUI.Label(new Rect(810, startPos + 120, 145, 100), "DoT: " + tmpTower.dotDamage + "d/1s for 5s");
				GUI.Label(new Rect(810, startPos + 140, 145, 100), "Attack Type: " + tmpTower.attackType + " - " + tmpTower.aboutTower);
			} else if (tmpTower.towerType == TowerType.Frost) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack: Crippling creeps run speed");
				GUI.Label(new Rect(810, startPos + 120, 145, 100), "Slow: " + (tmpTower.slow * 100) + "% for 5s");
				GUI.Label(new Rect(810, startPos + 140, 145, 100), "Attack Type: " + tmpTower.attackType + " - " + tmpTower.aboutTower);
			}
		}

		if(creepSelected) {
			Enemy tmpEnemy = curCreepSelected.GetComponent<Enemy>();

			GUI.Label(new Rect(810, 400, 145, 25), "Name: " + tmpEnemy.name);
			GUI.Label(new Rect(810, 420, 145, 25), "Level: " + tmpEnemy.Level);
			GUI.Label(new Rect(810, 440, 145, 25), "HP: " + tmpEnemy.health + " / " + tmpEnemy.CurStartHealth);
			GUI.Label(new Rect(810, 460, 145, 25), "Run Speed: " + tmpEnemy.moveSpeed);
			GUI.Label(new Rect(810, 480, 145, 25), "Slow Resistance: " + (tmpEnemy.slowResistance * 100) + "%");
			GUI.Label(new Rect(810, 500, 145, 25), "Poison Armor: " + tmpEnemy.poisonResistance);
			GUI.Label(new Rect(810, 520, 145, 25), "Armor: " + tmpEnemy.armor);
			GUI.Label(new Rect(810, 540, 145, 50), "Armor Type: " + tmpEnemy.armorType);

			if(!tmpEnemy.gameObject.activeSelf) {
				creepSelected = false;
				curCreepSelected = null;
			}
		}

		if(showTowerStats) {

			int startPos = 320;

			GUI.Label(new Rect(810, startPos, 145, 25), "Name: " + curT.name);
			GUI.Label(new Rect(810, startPos + 20, 145, 25), "Level: " + curT.level);
			GUI.Label(new Rect(810, startPos + 40, 145, 25), "Damage: " + curT.damage);
			GUI.Label(new Rect(810, startPos + 60, 145, 25), "Attack Cooldown: " + curT.fireRateCoolDown);
			if(curT.towerType == TowerType.Arrow) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack: None");
				GUI.Label(new Rect(810, startPos + 100, 145, 100), "Attack Type: " + curT.attackType + " - " + curT.aboutTower);
			} else if (curT.towerType == TowerType.Bomb) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack: Bombs gives AoE damage");
				GUI.Label(new Rect(810, startPos + 120, 145, 100), "Attack Type: " + curT.attackType + " - " + curT.aboutTower);
			} else if (curT.towerType == TowerType.Poison) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack: Applies a DoT to creep");
				GUI.Label(new Rect(810, startPos + 120, 145, 100), "DoT: " + curT.dotDamage + "d/1s for 5s");
				GUI.Label(new Rect(810, startPos + 140, 145, 100), "Attack Type: " + curT.attackType + " - " + curT.aboutTower);
			} else if (curT.towerType == TowerType.Frost) {
				GUI.Label(new Rect(810, startPos + 80, 145, 100), "Special Attack: Crippling creeps run speed");
				GUI.Label(new Rect(810, startPos + 120, 145, 100), "Slow: " + (curT.slow * 100) + "% for 5s");
				GUI.Label(new Rect(810, startPos + 140, 145, 100), "Attack Type: " + curT.attackType + " - " + curT.aboutTower);
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
