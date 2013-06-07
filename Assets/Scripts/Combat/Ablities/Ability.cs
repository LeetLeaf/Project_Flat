using UnityEngine;
using System.Collections;

public abstract class Ability : MonoBehaviour 
{

    public bool rangedAttack { get; set; }// checks if the attack is ranged or not if(isPassive){rangedAttack = false}
    public bool isPassive { get; set; } //checks if the ability is passive or requires activtion
    public int level {get; set;}
    public float attackCoolDown { get; set; }

    public abstract void Attack(Animation animation,float directionX);//Allows player to use attack also checks if an enemy is in range
    public abstract void AttackSuccess(GameObject enemyPlayer);//attack hits
    public abstract void AttackMiss();//attack misses

}
