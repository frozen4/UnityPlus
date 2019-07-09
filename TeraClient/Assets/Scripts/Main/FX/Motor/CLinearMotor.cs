using UnityEngine;
using System.Collections;

public class CLinearMotor : CMotor 
{
	//false: stop move
	//true:  continue move
	protected override bool OnMove(float dt)
	{
		Vector3 dest_pos = GetDestPos();
        Vector3 vec_2_target = dest_pos - transform.position;
        float distance = vec_2_target.magnitude;

        if (distance < 0.001f)
            vec_2_target = Vector3.zero;
        else
            vec_2_target = vec_2_target / distance;     //normalized
		float step = dt * _HorzSpeed;

		bool arrived = false;
        if (Mathf.Abs(distance - step) < _Tolerance)
        {
            arrived = true;
            transform.position = dest_pos;
        }
        else
        {
            transform.position += vec_2_target * _HorzSpeed * dt;
        }


		Vector3 up = Vector3.up;
        if (up != vec_2_target)
		{
            Vector3 right = Vector3.Cross(up, vec_2_target);
            up = Vector3.Cross(vec_2_target, right);
		}
        transform.LookAt(dest_pos, up);

		return !arrived;
	}

}
