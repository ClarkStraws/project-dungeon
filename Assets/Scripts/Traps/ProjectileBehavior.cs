using System;
using System.Collections;
using UnityEngine;
public class ProjectileBehavior : MonoBehaviour
{
    public Vector3 directionOfTravel;

    public bool isAtSpawn;

    public void Move()
    {
        if (isAtSpawn)
        {
            isAtSpawn = false;
            return;
        }

        else
        {
            gameObject.transform.position += (Vector3)directionOfTravel;
            return;
        }

    }

    // public bool CollisionPath(){
        
    //     // Let's check to see if we should despawn our projectile or deal damage to something
    //     Vector3 checkPosition = gameObject.transform.position + directionOfTravel;
    //     LayerMask mask = ~(1 << LayerMask.NameToLayer("ObjectPlacementLayer")); // we want to ignore the placement layer that we used for creating objects  in each scene
    //     Collider2D collision = Physics2D.OverlapCircle(checkPosition, 0.1f, mask);

    //     if(collision == null){
    //         return false;
    //     }

    //     else if(collision.gameObject.tag == "Player"){
    //         StartCoroutine(HitPlayer(collision.gameObject))
    //     }

    // }

    IEnumerator HitPlayer(GameObject player){
        gameObject.transform.position += (Vector3)directionOfTravel;

        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);

        
    }
}