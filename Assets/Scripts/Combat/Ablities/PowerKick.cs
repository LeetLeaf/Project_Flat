using UnityEngine;
using System.Collections;

public class PowerKick : Ability
{
    Vector3 attackForce;

    public PowerKick(int level)
    {
        this.level = level;
        isPassive = false;
        rangedAttack = false;
        attackCoolDown = 0.5f;
    }

    public override void Attack(Animation animation)
    {
        animation.Play("powerKick");
    }

    public override void AttackSuccess(GameObject enemyPlayer)
    {
        enemyPlayer.transform.networkView.RPC("knockBack", RPCMode.All, attackForce);
    }

    public override void AttackMiss()
    {
        
    }
}
